using AutoMapper;
using Dapper;
using Microsoft.EntityFrameworkCore;
using OtpNet;
using Serilog;
using settl.identityserver.Appication.Contracts.RepositoryInterfaces;
using settl.identityserver.Application.Contracts.DTO;
using settl.identityserver.Application.Contracts.DTO.Onboarding;
using settl.identityserver.Application.Contracts.DTO.OTP;
using settl.identityserver.Application.Contracts.IServices;
using settl.identityserver.Dapper;
using settl.identityserver.Domain.Entities;
using settl.identityserver.Domain.Shared;
using settl.identityserver.Domain.Shared.Enums;
using settl.identityserver.Domain.Shared.Helpers;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace settl.identityserver.Application.Services
{
    public class OTPService : IOTPService
    {
        private readonly IMapper _mapper;
        private readonly IDapper _dapper;
        private readonly ISmsService _smsService;
        private readonly IEmailService _emailService;
        private readonly IGenericRepository<tbl_OTP> _otpRepository;
        private readonly IGenericRepository<tbl_user_on_boarding> _userOnboardingRepository;

        public OTPService(IEmailService emailService, IDapper dapper, IMapper mapper, ISmsService smsService, IGenericRepository<tbl_OTP> otpRepository, IGenericRepository<tbl_user_on_boarding> userOnboardingRepository)
        {
            _mapper = mapper;
            _dapper = dapper;
            _smsService = smsService;
            _emailService = emailService;
            _otpRepository = otpRepository;
            _userOnboardingRepository = userOnboardingRepository;
        }

        private async Task<Auth> UserExists(string phone)
        {
            var sql = $"Select * from [dbo].[tbl_auth] where [Phone] = @Phone OR Email = @Phone and deleted = 0";
            var dbArgs = new DynamicParameters();
            dbArgs.Add("Phone", phone);
            var user = await Task.FromResult(_dapper.Get<Auth>(sql, dbArgs, commandType: CommandType.Text));
            return user;
        }

        private async Task<Auth> AdminExists(string email)
        {
            var sql = $"Select * from [dbo].[tbl_auth] where deleted = 0 and Email = @Email and admin = 1";
            var dbArgs = new DynamicParameters();
            dbArgs.Add("Email", email);
            var admin = await Task.FromResult(_dapper.Get<Auth>(sql, dbArgs, commandType: CommandType.Text));
            return admin;
        }

        public async Task<bool> SendOTP(SendSMSDTO sendSMSDTO)
        {
            try
            {
                var isEmail = !string.IsNullOrEmpty(sendSMSDTO.Email);

                Auth user = isEmail ? await AdminExists(sendSMSDTO.Email) : await UserExists(sendSMSDTO.Phone);

                if (sendSMSDTO.OtpType == OtpType.REGISTER_NEWDEVICE && isEmail) user = await UserExists(sendSMSDTO.Email);

                var userExists = !isEmail && user is not null;
                var adminExists = isEmail && user is not null;

                if (userExists && (sendSMSDTO.OtpType == OtpType.PHONE_NUMBER_VALIDATION)) throw new CustomException("User already exists. Proceed to login");

                if (!userExists && !adminExists && (sendSMSDTO.OtpType == OtpType.FORGOT_PASSWORD)) throw new CustomException("This user doesn't exist.");

                if (user is null && sendSMSDTO.OtpType == OtpType.REGISTER_NEWDEVICE) throw new CustomException("This user doesn't exist.");

                var otpCode = await GenerateRandomOTP(isEmail: isEmail, otp: sendSMSDTO.OtpType);

                var otpExpiresBy = DateTime.UtcNow.AddHours(1).AddMinutes(5).ToString("dd/MM/yyyy hh:mm:ss tt");

                string messageBody = string.Empty;

                if (!isEmail)
                {
                    messageBody = GenerateOTPMessage(sendSMSDTO.OtpType, otpCode);
                    messageBody = messageBody.Replace("[FName]", sendSMSDTO.Phone);
                    messageBody = messageBody.Replace("[DateTime]", otpExpiresBy);
                }

                bool success = false;
                dynamic response;

                if (isEmail)
                {
                    var emailRequest = new EmailRequest
                    {
                        Name = sendSMSDTO.OtpType == OtpType.REGISTER_NEWDEVICE ? $"{user.Firstname} {user.Lastname}" : sendSMSDTO.Email,
                        Email = sendSMSDTO.Email,
                        EmailCode = sendSMSDTO.OtpType.ToString() == "REGISTER_NEWDEVICE" ? Constants.EMAIL_TEMPLATE_FOR_OTP_VERIFYNEWDEVICE : Constants.EMAIL_TEMPLATE_FOR_OTP_VERIFICATION,
                        OtpCode = otpCode
                    };
                    (success, response) = await _emailService.SendSettlEmail(emailRequest);
                }
                else
                {
                    var smsRequest = new SMSRequest
                    {
                        Phone = sendSMSDTO.Phone,
                        Body = messageBody,
                        ReceiverName = user is not null ? $"{user.Firstname} {user.Lastname}" : "NEW USER"
                    };

                    (success, response) = await _smsService.SendSMS(smsRequest);
                }

                if (!success)
                {
                    Log.Error(response.Message);
                    throw new CustomException($"Oops! Problem sending verification code.");
                }

                CreateOTPDTO createOTPDTO = new()
                {
                    Code = otpCode,
                    Phone = sendSMSDTO?.Phone ?? string.Empty,
                    Email = sendSMSDTO?.Email ?? string.Empty
                };

                var otp = _mapper.Map<tbl_OTP>(createOTPDTO);

                await _otpRepository.Create(otp);
                await _otpRepository.Save();

                return true;
            }
            catch (CustomException ex)
            {
                Log.Error(ex, ex.Message);
                throw;
            }
        }

        public async Task<ResponsesDTO> VerifyOTP(VerifyOTPDTO verifyOTPDTO)
        {
            var otpExist = await _otpRepository.Query().FirstOrDefaultAsync(x => (x.phone == verifyOTPDTO.Phone || x.email == verifyOTPDTO.Email) && x.code == verifyOTPDTO.OTPNumber);

            if (otpExist == null) return Responses.BadRequest(message: "Invalid verification code or already used.");

            var MinutesNow = DateAndTimeHelper.GetCurrentServerTime();
            var otpMinutes = otpExist.CreatedOn;

            if ((MinutesNow - otpMinutes).Minutes > 5) return Responses.BadRequest(message: "OTP expired.");

            _otpRepository.Delete(otpExist);
            await _otpRepository.Save();

            var entity = string.IsNullOrEmpty(verifyOTPDTO.Email) ? "Phone Number" : "Email Address";

            return Responses.Ok($"{entity} verified successfully.");
        }

        public async Task<(bool, string)> Verify(string otp, string phone, string email = "")
        {
            var otpExist = await _otpRepository.Query().FirstOrDefaultAsync(x => x.phone == phone && x.code == otp || x.email == email && x.code == otp);

            if (otpExist == null) return (false, "Invalid verification code or already used.");

            var MinutesNow = DateTime.UtcNow;
            var otpMinutes = otpExist.CreatedOn;

            if ((MinutesNow - otpMinutes).Minutes > 5) return (false, "OTP expired.");

            return (true, "Phone number or Email Address verified successfully.");
        }

        private async Task<string> GenerateRandomOTP(double randomDays = 0, bool isEmail = false, OtpType otp = 0)
        {
            // TODO: Verify if OTP exists
            var randomString = Path.GetRandomFileName().Replace(".", "");
            var _secret = Environment.GetEnvironmentVariable("OTPSecret") + randomString;
            var secret = Encoding.Default.GetBytes(_secret);

            int size = 4;

            if (isEmail) size = 6;

            if (otp == OtpType.REGISTER_NEWDEVICE) size = 5;

            var totp = new Totp(secret, 600, totpSize: size);
            var totpCode = totp.ComputeTotp(DateHelper.GetCurrentLocalTime().AddDays(randomDays));

            var otpExists = await _otpRepository.Query().FirstOrDefaultAsync(x => x.code == totpCode) is not null;

            var remainingSeconds = totp.RemainingSeconds(DateTime.UtcNow);

            if (otpExists)
                totpCode = await GenerateRandomOTP(remainingSeconds, isEmail, otp: otp);

            return totpCode;
        }

        private async Task<string> GetMessage(string code)
        {
            var sql = $"Select Message from [tbl_Global_Message] where [Code] = @Code";
            var dbArgs = new DynamicParameters();
            dbArgs.Add("Code", code);
            var message = await Task.FromResult(_dapper.Get<string>(sql, dbArgs, commandType: CommandType.Text));

            if (string.IsNullOrEmpty(message)) throw new Exception($"No message exists for code {code}");

            return message;
        }

        private string GenerateOTPMessage(OtpType otpType, string OtpCode)
        {
            try
            {
                return otpType switch
                {
                    OtpType.PHONE_NUMBER_VALIDATION => GetMessage(Constants.SMS_TEMPLATE_FOR_PHONE_VALIDATION).Result.Replace("[OTPCode]", OtpCode),
                    OtpType.FORGOT_PASSWORD => GetMessage(Constants.SMS_TEMPLATE_FOR_RESET_PASSWORD).Result.Replace("[Pin Code]", OtpCode),
                    _ => $"Kindly use this OTP: {OtpCode} to complete your process.",
                };
            }
            catch (CustomException ex)
            {
                Log.Error(ex, ex.Message);
                throw;
            }
        }
    }
}