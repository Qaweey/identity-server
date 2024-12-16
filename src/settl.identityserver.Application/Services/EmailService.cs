using Dapper;
using Newtonsoft.Json;
using Serilog;
using settl.identityserver.Application.Contracts.DTO;
using settl.identityserver.Application.Contracts.DTO.SMS;
using settl.identityserver.Application.Contracts.DTO.Users;
using settl.identityserver.Application.Contracts.IServices;
using settl.identityserver.Dapper;
using settl.identityserver.Domain.Entities;
using settl.identityserver.Domain.Shared.Enums;
using settl.identityserver.Domain.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace settl.identityserver.Application.Services
{
    public class EmailService : IEmailService
    {
        private readonly IDapper dapper;

        public EmailService(IDapper dapperService)
        {
            dapper = dapperService;
        }

        public async Task<EmailTemplate> GetTemplate(string code)
        {
            var sql = $"SELECT * FROM [tbl_email_template] WHERE EmailCode = @Code";

            var dbArgs = new DynamicParameters();
            dbArgs.Add("Code", code);

            var template = await Task.FromResult(dapper.Get<EmailTemplate>(sql, dbArgs, commandType: CommandType.Text));

            return template;
        }

        public async Task<(bool, BaseSettlApiDTO)> SendSettlEmail(EmailRequest request)
        {
            HttpClient client = new();
            var emailUrl = $"{Constants.EMAILSERVICES_URL}/email/send/settl";
            var response = await client.PostAsJsonAsync(emailUrl, request);

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<BaseSettlApiDTO>(json);
            return (response.IsSuccessStatusCode && result?.Code == "00", result);
        }

        public async Task<(bool, List<BaseSettlApiDTO>)> SendBulkSettlEmail(BulkEmailRequest request)
        {
            HttpClient client = new();
            var emailUrl = $"{Constants.EMAILSERVICES_URL}/email/sendbulk/settl";
            var response = await client.PostAsJsonAsync(emailUrl, request);

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<List<BaseSettlApiDTO>>(json);
            return (response.IsSuccessStatusCode, result);
        }

        public async Task<(bool, List<BaseSettlApiDTO>)> SendBulkTemplateEmail(AWSBulkEmailDTO request)
        {
            HttpClient client = new();
            var emailUrl = $"{Constants.EMAILSERVICES_URL}/email/template/bulk";
            var response = await client.PostAsJsonAsync(emailUrl, request);

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<List<BaseSettlApiDTO>>(json);
            return (response.IsSuccessStatusCode, result);
        }

        public async Task<bool> SendRegistrationEmail(UserDTO user, bool isReferred)
        {
            try
            {
                var emailTemplate = await GetTemplate("004");

                if (emailTemplate is null) Log.Error("No Email Template for code 004");

                var recipients = new List<EmailRequest>
                {
                    new EmailRequest("Promise Osagie", "promise@settl.me"),
                    new EmailRequest("James Morgan", "james@settl.me"),
                    new EmailRequest("Olawumi Tayo", "olawumi@settl.me"),
                    new EmailRequest("Adeyemi Adekunle", "adeyemi@settl.me")
                };

                var body = emailTemplate.Templates.Replace("{FirstName}", user.FirstName)
                            .Replace("{LastName}", user.LastName).Replace("{Phone}", user.Phone)
                            .Replace("{PhoneModel}", user.phone_model_no).Replace("{PhoneName}", user.phone_name)
                            .Replace("{IMEI}", user.Imei_no).Replace("{Gender}", user.Gender).Replace("{Email}", user.Email)
                            .Replace("{Referred}", isReferred.ToString()).Replace("{UserName}", user.UserName);

                var request = new AWSBulkEmailDTO
                {
                    Recipients = recipients,
                    Description = emailTemplate.Description,
                    FromEmail = emailTemplate.FromEmail,
                    FromName = emailTemplate.FromName,
                    Subject = $"{emailTemplate.Subject} -  {Constants.ASPNETCORE_ENVIRONMENT ?? "Unknown"}",
                    Templates = JsonHelper.SerializeObject(body),
                    Type = emailTemplate.EmailType
                };

                var (success, response) = await SendBulkTemplateEmail(request);
                Log.Information("Email Template bulk response" + JsonHelper.SerializeObject(response));
                return success;
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
                return false;
            }
        }

        public async Task<(bool, BaseSettlApiDTO)> SendSettlEmail(EmailRequestTemplate request)
        {
            HttpClient client = new();
            var emailUrl = $"{Constants.EMAILSERVICES_URL}/email/template";
            request.templates = JsonConvert.SerializeObject(request.templates);

            var response = await client.PostAsJsonAsync(emailUrl, request);

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<BaseSettlApiDTO>(json);
            return (response.IsSuccessStatusCode && result?.Code == "00", result);
        }
    }
}