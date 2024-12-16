using AutoMapper;
using Dapper;
using RestSharp;
using Serilog;
using settl.identityserver.Appication.Contracts.RepositoryInterfaces;
using settl.identityserver.Application.Contracts.DTO;
using settl.identityserver.Application.Contracts.DTO.Consumer;
using settl.identityserver.Application.Contracts.DTO.OTP;
using settl.identityserver.Application.Contracts.DTO.Users;
using settl.identityserver.Application.Contracts.IServices;
using settl.identityserver.Dapper;
using settl.identityserver.Domain.Entities;
using settl.identityserver.Domain.Shared;
using settl.identityserver.Domain.Shared.Enums;
using settl.identityserver.Domain.Shared.Helpers;
using settl.identityserver.Domain.Shared.Helpers.Authentication;
using settl.identityserver.Domain.Shared.Helpers.Cryptography;
using settl.identityserver.EntityFrameworkCore.RepositoryImplementations;
using System;
using System.Data;
using System.Net.Http;
using System.Threading.Tasks;

namespace settl.identityserver.Application.Services
{
    public class ConsumerService : ConsumerRepository<CreateConsumerResponseDTO>, IConsumerService
    {
        private readonly IDapper _dapper;
        private readonly IMapper _mapper;
        private readonly IOTPService _oTPService;
        private readonly ISmsService _smsService;
        private readonly IEmailService _emailService;
        private readonly ITokenService _tokenService;
        private readonly IGenericRepository<Auth> _userRepository;

        public ConsumerService(IDapper dapper, IGenericRepository<Auth> userRepository, IOTPService oTPService, ISmsService smsService, IEmailService emailService, IMapper mapper, ITokenService tokenService)
        {
            _dapper = dapper;
            _mapper = mapper;
            _oTPService = oTPService;
            _smsService = smsService;
            _emailService = emailService;
            _tokenService = tokenService;
            _userRepository = userRepository;
        }

        public void InitializeApiRequest(string token = "")
        {
            Connect(token);
        }

        public async Task<(bool, CreateConsumerResponseDTO)> Create(CreateConsumerDTO model)
        {
            try
            {
                InitializeApiRequest();
                var url = $"/consumerwallet/create";

                var response = await PostCreateConsumerAsync(model, url, Method.POST);
                Log.Error(response.Item1?.Message);

                if (response.Item2 != System.Net.HttpStatusCode.OK) throw new CustomException();

                if (response.Item1.Code == "00") return (true, response.Item1);
                else throw new CustomException(response.Item1.Message);
            }
            catch (CustomException ex)
            {
                Log.Error(ex.Message);
                return (false, new CreateConsumerResponseDTO { Message = ex.Message });
            }
        }

        public async Task<(bool, ConsumerProfileResponseDTO)> GetProfile(string phone, string token)
        {
            try
            {
                InitializeApiRequest(token);
                var url = $"/consumerprofile/getbyphoneno?phoneno={phone}";

                var (response, statusCode) = await GetProfile(null, url, Method.GET);

                if (response.Code == "00") return (true, response);
                else return (false, response);
            }
            catch (CustomException ex)
            {
                Log.Error(ex.Message);

                return (false, null);
            }
        }

        private async Task<(bool, ConsumerWalletDTO)> GetWalletByPhone(string phone)
        {
            var client = new HttpClient();

            client.DefaultRequestHeaders.Add("X-RequestId", Guid.NewGuid().ToString());
            client.DefaultRequestHeaders.Add("X-Settl-Api-Token", Constants.SETTL_API_TOKEN);

            var consumerResponse = await client.GetAsync($"{Constants.CONSUMER_URL}/ConsumerWallet/getbyphoneno?phoneno={phone}");
            var consumerResult = await consumerResponse.Content.ReadAsStringAsync();

            Log.Information($"Consumer wallet response - {consumerResult} | HTTP Status Code - {consumerResponse.StatusCode}");

            if (!consumerResponse.IsSuccessStatusCode) return (false, null);

            var consumerData = JsonHelper.DeserializeObject<ConsumerWalletResponseDTO>(consumerResult);

            if (consumerData is null) return (false, null);

            return (consumerData?.Code == "00", consumerData.Data);
        }

        public async Task<ResponsesDTO> RegisterNewDevice(RegisterNewDeviceDto newDeviceDto)
        {
            //get user info
            var getUsers = await _userRepository.Get(x => x.phone == newDeviceDto.PhoneNo);

            if (getUsers is null)
            {
                throw new CustomException($"The user with phone no {newDeviceDto.PhoneNo} does not exist");
            }

            if (getUsers.IsBlacklisted) throw new CustomException($"The user with phone no {newDeviceDto.PhoneNo} is blacklisted");

            if (getUsers.IsDeleted) throw new CustomException($"The user with phone no {newDeviceDto.PhoneNo} is frozen");

            if (getUsers.phone_name == newDeviceDto.PhoneName && getUsers.phone_model_no == newDeviceDto.PhoneModelNo && getUsers.imei_no == newDeviceDto.IMEINO)
            {
                throw new CustomException("This is your active device, please log in.");
            }
            // Verify OTP
            var validUser = new VerifyOTPDTO
            {
                Email = getUsers.email,
                Phone = newDeviceDto.PhoneNo,
                OTPNumber = newDeviceDto.OTP
            };
            var response = await _oTPService.VerifyOTP(validUser);
            if (response.Code != "00")
            {
                throw new CustomException($"{response.Message}");
            }

            var query = "INSERT INTO tbl_registered_new_devices ([Id],[PhoneNo],[IMEINO], [IsVerified],[DateAndTimeOfLogin],[Email],[PhoneName],[PhoneModelNo],[RegistrationDate],[IpAddress])" +
                "VALUES(@id,@phone,@IMEINO,@isverified,@dateoflogin,@email,@phonename,@modelno,@regdate,@ipaddress)";

            var dbArg = new DynamicParameters();
            dbArg.Add("@id", Guid.NewGuid().ToString());
            dbArg.Add("@phone", newDeviceDto.PhoneNo);
            dbArg.Add("@IMEINO", newDeviceDto.IMEINO);
            dbArg.Add("@isverified", true);
            dbArg.Add("@dateoflogin", DateAndTimeHelper.GetCurrentServerTime());
            dbArg.Add("@email", getUsers.email);

            dbArg.Add("@phonename", newDeviceDto.PhoneName);
            dbArg.Add("@modelno", newDeviceDto.PhoneModelNo);
            dbArg.Add("@regdate", DateHelper.GetCurrentLocalTime());
            dbArg.Add("@ipaddress", newDeviceDto.IPAddress);
            await Task.FromResult(_dapper.Execute(query, dbArg, commandType: CommandType.Text));

            var userDTO = _mapper.Map<UserDTO>(getUsers);

            var (refreshTokenRequest, _) = JWT.GenerateJwtToken(newDeviceDto.PhoneNo, userDTO);
            refreshTokenRequest.RefreshToken = Hashing.Base64StringEncode($"{refreshTokenRequest.RefreshToken}phone{newDeviceDto.PhoneNo}|{newDeviceDto.PhoneModelNo}|{newDeviceDto.IMEINO}");

            var userquery = $"Update [tbl_auth] set [phone_name] = @phonename, [phone_model_no] = @phonemodelno, [imei_no]=@imei, JwtToken = @token, RefreshToken = @refreshToken where [phone]=@phone";
            var dbArgsuser = new DynamicParameters();
            dbArgsuser.Add("@phonename", newDeviceDto.PhoneName);
            dbArgsuser.Add("@phonemodelno", newDeviceDto.PhoneModelNo);
            dbArgsuser.Add("@phone", newDeviceDto.PhoneNo);
            dbArgsuser.Add("@imei", newDeviceDto.IMEINO);
            dbArgsuser.Add("@token", refreshTokenRequest.Token);
            dbArgsuser.Add("@refreshToken", refreshTokenRequest.RefreshToken);
            var rows = await Task.FromResult(_dapper.Execute(userquery, dbArgsuser, commandType: CommandType.Text));

            if (rows <= 0) throw new CustomException("Unable to register new device at this time, please try again later");

            _tokenService.SetUserTokens(newDeviceDto.PhoneNo, refreshTokenRequest.Token, refreshTokenRequest.RefreshToken);

            var sqls = $"Select * from [tbl_email_template] where active = 1 and [EmailCode] = @emailcode";
            var dbArgsd = new DynamicParameters();
            dbArgsd.Add("@emailcode", Constants.EMAIL_TEMPLATE_FOR_NEWDEVICE_REGISTRATION);
            var emailres = await Task.FromResult(_dapper.Get<EmailEntityDto>(sqls, dbArgsd, commandType: CommandType.Text));
            var emailreq = new EmailRequestTemplate
            {
                description = emailres.Description,
                fromEmail = emailres.FromEmail,
                emailCode = Constants.EMAIL_TEMPLATE_FOR_NEWDEVICE_REGISTRATION,
                fromName = emailres.FromName,
                type = emailres.EmailType,
                templates = emailres.Templates.Replace("{name}", getUsers.Firstname).Replace("{dateoflogin}", DateAndTimeHelper.GetCurrentServerTime().ToString("dd/MM/yyyy HH:mm tt"))
               .Replace("{phonename}", newDeviceDto.PhoneModelNo).Replace("{ipaddress}", newDeviceDto.IPAddress),
                email = getUsers.email,
                microserviceName = Constants.IDENTITYSERVER_URL,
                name = getUsers.Firstname,
                subject = "Settl New Device Registration"
            };
            var (success, respons) = await _emailService.SendSettlEmail(emailreq);
            if (!success)
            {
                Log.Error($"Error occurred while sending Emails:{response.Message}");
            }

            var messageBody = $"Hi {getUsers.Firstname},your Settl.me account was used to log in on the device with details below.\n" +
                "Please review the details to confirm it was you.\n" +
                $"Date & Time of login:{DateAndTimeHelper.GetCurrentServerTime():dd/MM/yyyy HH:mm tt}\n" +
                $"Type of device:{newDeviceDto.PhoneModelNo}\n" +
                $"IP address:{newDeviceDto.IPAddress}\n" +
                "Don't recognise this activity? Please contact our support team immediately.Thank you.";

            var smsRequest = new SMSRequest
            {
                Phone = newDeviceDto.PhoneNo,
                ReceiverName = $"{getUsers.Firstname} {getUsers.Lastname}",
                Body = messageBody,
                MicroserviceName = Constants.IDENTITYSERVER_URL
            };
            var (status, res) = await _smsService.SendSMS(smsRequest);
            if (!status)
            {
                Log.Error($"{res.Message}");
            }
            return new ResponsesDTO { Code = "00", Message = "Registration is successful", Data = refreshTokenRequest };
        }

        public async Task<string> VerifyPinAndSendOTP(string phone, string transPin)
        {
            //get user info
            var getUsers = await _userRepository.Get(x => x.phone == phone);

            if (getUsers is null)
            {
                throw new CustomException($"The user with phone no {phone} does not exist");
            }
            if (getUsers.IsBlacklisted)
            {
                throw new CustomException($"The user with phone no {phone} has been blacklisted");
            }
            if (getUsers.IsDeleted)
            {
                throw new CustomException($"The user with phone no {phone} has been frozen");
            }
            // Getting the transaction Pin
            var (success, consumerWallet) = await GetWalletByPhone(phone);

            if (!success) throw new CustomException("Unable to verify transaction pin");

            if (!consumerWallet.HasTransactionPin)
            {
                Log.Error($"The Phone no {phone} does not have a transaction pin");
                throw new CustomException($"The Phone no {phone} does not have a transaction pin");
            }

            if (consumerWallet.TransactionPin != transPin) throw new CustomException("The transaction PIN is not correct");

            //send mail
            var sendOtp = new SendSMSDTO
            {
                Email = getUsers.email,
                OtpType = OtpType.REGISTER_NEWDEVICE
            };

            var result = await _oTPService.SendOTP(sendOtp);

            if (!result)
            {
                Log.Error("Error Occured : Unable to send OTP");
                throw new CustomException($"Unable to send OTP to the Email {getUsers.email}");
            }

            return getUsers.email;
        }
    }
}