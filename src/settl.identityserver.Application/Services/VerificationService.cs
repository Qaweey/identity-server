using AutoMapper;
using Dapper;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.OpenSsl;
using RestSharp;
using Serilog;
using settl.identityserver.Application.Contracts.DTO.KYC;
using settl.identityserver.Application.Contracts.IServices;
using settl.identityserver.Dapper;
using settl.identityserver.Domain.Entities;
using settl.identityserver.Domain.Shared.Enums;
using settl.identityserver.Domain.Shared.Helpers;
using settl.identityserver.Domain.Shared.Helpers.ApiConnect;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace settl.identityserver.Application.Services
{
    public class VerificationService : IVerificationService
    {
        private readonly string PartnerId;
        private readonly IMapper _mapper;
        private readonly IDapper _dapper;

        public VerificationService(IConfiguration configuration, IMapper mapper, IDapper dapper)
        {
            PartnerId = configuration.GetSection("SmileIdentity:partner_id").Value;
            _mapper = mapper;
            _dapper = dapper;
        }

        private static string RsaEncryptWithPublic(string clearText, string publicKey)
        {
            var bytesToEncrypt = Encoding.UTF8.GetBytes(clearText);
            var encryptEngine = new Pkcs1Encoding(new RsaEngine());
            using (var txtreader = new System.IO.StringReader(publicKey))
            {
                var keyParameter = (AsymmetricKeyParameter)new PemReader(txtreader).ReadObject();
                encryptEngine.Init(true, keyParameter);
            }
            var encrypted = Convert.ToBase64String(encryptEngine.ProcessBlock(bytesToEncrypt, 0, bytesToEncrypt.Length));
            return encrypted;
        }

        private static string ByteArrayToHexString(byte[] ba)
        {
            var hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);

            return hex.ToString();
        }

        public (string, string) GenerateSecCredentials(string api_key, string partnerId)
        {
            // But for the sec_key calculation we need an int.
            int partnerID = int.Parse(partnerId);

            // Get a Uxix timestamp
            int timestamp = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

            string plaintext = partnerID + ":" + timestamp;

            // Hash the plaintext
            SHA256 mySHA256 = SHA256.Create();
            var bytesToEncrypt = mySHA256.ComputeHash(Encoding.UTF8.GetBytes(plaintext));

            var encrypted = RsaEncryptWithPublic(ByteArrayToHexString(bytesToEncrypt), api_key);

            string sec_key = encrypted + "|" + ByteArrayToHexString(bytesToEncrypt);

            return (timestamp.ToString(), sec_key);
        }

        public async Task<(bool, dynamic)> VerifyKYC(KycDTO kyc)
        {
            var smileApiKey = Constants.SMILE_APIKEY.Replace("@", Environment.NewLine);

            var (timestamp, sec_key) = GenerateSecCredentials(smileApiKey, PartnerId);

            var smileRequest = new SmileRequestDTO(sec_key, timestamp, PartnerId)
            {
                Id_Type = kyc.Id_type.ToString()
            };

            _mapper.Map(kyc, smileRequest);

            if (!string.IsNullOrEmpty(smileRequest?.Gender)) smileRequest.Gender = smileRequest.Gender[..1].ToUpper();
            HttpClient client = new();
            var response = await client.PostAsJsonAsync(Constants.SMILEIDENTITY_URL, smileRequest);
            var json = await response.Content.ReadAsStringAsync();
            Log.Information($"Smile Response - {json}");
            var success = response.IsSuccessStatusCode;
            dynamic result;

            if (!success)
            {
                Log.Error(json);
                result = JsonConvert.DeserializeObject<ErrorResponse>(json);
                return (success, result);
            }

            result = JsonConvert.DeserializeObject<VerificationResponseDTO>(json);

            if (!result.ResultCode.StartsWith("102")) success = false;

            return (success, result);
        }

        public async Task<(bool, dynamic)> VerifySelfie(Welcome2 uploadselfie)
        {
            var smileApiKey = Constants.SMILE_APIKEY.Replace("@", Environment.NewLine);

            var (timestamp, sec_key) = GenerateSecCredentials(smileApiKey, PartnerId);
            SelfieDTO selfie = new SelfieDTO();
            selfie.file_name = uploadselfie.images.FirstOrDefault().file_name;
            selfie.smile_client_id = PartnerId;
            selfie.timestamp = timestamp;
            selfie.sec_key = sec_key;
            var param = new PartnerSelfieParams();

            selfie.partner_params = param;
            //_mapper.Map(selfie, smileRequest);
            selfie.callback_url = Constants.SMILEIDENTITY_CALLBACK;
            HttpClient client = new();
            var response = await client.PostAsJsonAsync(Constants.SMILEIDENTITY_SELFIE, selfie);
            var json = await response.Content.ReadAsStringAsync();

            Log.Information($"Smile Response - {json}");
            var selfieKycResponse = JsonConvert.DeserializeObject<SelfieKycUploadResponseDTO>(json);
            var success = response.IsSuccessStatusCode;
            dynamic result;

            if (!success)
            {
                Log.Error(json);
                result = JsonConvert.DeserializeObject<ErrorResponse>(json);
                return (success, result);
            }

            result = JsonConvert.DeserializeObject<SelfieKycUploadResponseDTO>(json);

            var uploadResponse = await ExecutePutAsync(selfieKycResponse.upload_url);
            var users = new SmileIdVerification();
            if (uploadResponse.IsSuccessful)
            {
                var sql = $"Select * from [tbl_IdentityServer_IdVerifications] where [SmileJobId] = @smilejobid";
                var dbArgs = new DynamicParameters();
                dbArgs.Add("smilejobid", selfieKycResponse.smile_job_id);
                users = await Task.FromResult(_dapper.Get<SmileIdVerification>(sql, dbArgs, commandType: CommandType.Text));
            }

            return (success, users);
        }

        public async Task<bool> UpdateSelfie(SelfieKycUploadResponseDTO selfie)
        {
            try
            {
                using SqlConnection connection = new(Constants.DB_CONNECTION);
                await connection.OpenAsync();
                await connection.ExecuteAsync("dbo.spUpdateSelfie @upload_url,@ref_id,@smile_job_id,@camera_config,@code,@CreatedOn,@DeletedOn,@UpdatedOn,@IsActive,@IsDeleted ", new
                {
                    upload_url = selfie.upload_url,
                    ref_id = selfie.ref_id,
                    smile_job_id = selfie.smile_job_id,
                    camera_config = selfie.camera_config,
                    code = selfie.code,
                    CreatedOn = DateAndTimeHelper.GetCurrentServerTime(),
                    UpdatedOn = DateAndTimeHelper.GetCurrentServerTime(),
                    @DeletedOn = DateAndTimeHelper.GetCurrentServerTime(),
                    IsActive = true,
                    IsDeleted = false
                });

                return true;
            }
            catch (CustomException ex)
            {
                Log.Error(ex, ex.Message);
                throw;
            }
        }

        public async Task<IRestResponse> ExecutePutAsync(string url)
        {
            var client = new RestClient(url);
            client.Timeout = -1;
            var request = new RestRequest(Method.PUT);
            request.AddHeader("Content-Type", "application/zip");
            request.AddParameter("application/zip", "C:\\Users\\OlawumiTayo\\Pictures\\Camera Roll\\abcd.zip", ParameterType.RequestBody);
            var response = await client.ExecuteAsync(request);

            return response;
        }
    }
}