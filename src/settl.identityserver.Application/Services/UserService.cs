using AutoMapper;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Serilog;
using settl.identityserver.Appication.Contracts.RepositoryInterfaces;
using settl.identityserver.Application.Contracts.DTO.Admin;
using settl.identityserver.Application.Contracts.DTO.Consumer;
using settl.identityserver.Application.Contracts.DTO.Onboarding;
using settl.identityserver.Application.Contracts.DTO.Referral;
using settl.identityserver.Application.Contracts.DTO.SecurityAnswer;
using settl.identityserver.Application.Contracts.DTO.Users;
using settl.identityserver.Application.Contracts.IServices;
using settl.identityserver.Dapper;
using settl.identityserver.Domain.Entities;
using settl.identityserver.Domain.Shared;
using settl.identityserver.Domain.Shared.Enums;
using settl.identityserver.Domain.Shared.Helpers;
using settl.identityserver.Domain.Shared.Helpers.Authentication;
using settl.identityserver.Domain.Shared.Helpers.Cryptography;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace settl.identityserver.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IDapper _dapper;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;
        private readonly IAdminService _admin;
        private readonly IGenericRepository<Auth> _usersRepository;
        private readonly IGenericRepository<Referrals> _referralsRepository;
        private readonly IGenericRepository<tbl_user_on_boarding> _userOnboardingRepository;
        private readonly IGenericRepository<SecurityAnswer> _securityAnswerRepository;

        public UserService(IAdminService adminService, IMapper mapper, IGenericRepository<TblTempUser> tempUsersRepository, IGenericRepository<Auth> userRepository, IGenericRepository<tbl_user_on_boarding> userOnboardingRepository, IGenericRepository<SecurityAnswer> securityAnswerRepository, IGenericRepository<Referrals> referralsRepository, IDapper dapper, ITokenService tokenService)
        {
            _dapper = dapper;
            _tokenService = tokenService;
            _mapper = mapper;
            _admin = adminService;
            _usersRepository = userRepository;
            _referralsRepository = referralsRepository;
            _userOnboardingRepository = userOnboardingRepository;
            _securityAnswerRepository = securityAnswerRepository;
        }

        public async Task<ResponsesDTO> SignUpUser(CreateUsersDTO model)
        {
            try
            {
                var isListed = await IsBlacklistedNumber(model.Phone);

                if (isListed) return Responses.BadRequest(message: "This number has been blacklisted");

                var sql = $"Select * from [tbl_auth] where [Email] = @Email or [Phone] = @Phone or Username = @username and consumer = 1 and deleted = 0";

                var dbArgs = new DynamicParameters();
                dbArgs.Add("Email", model.Email);
                dbArgs.Add("Phone", model.Phone);
                dbArgs.Add("Username", model.UserName);

                var existingUser = await Task.FromResult(_dapper.Get<Auth>(sql, dbArgs, commandType: CommandType.Text));

                if (existingUser?.phone_name == "USSD")
                {
                    var userEntity = _mapper.Map(model, existingUser);
                    userEntity.secret = BCrypt.Net.BCrypt.HashPassword(userEntity.secret);
                    _usersRepository.Update(userEntity);
                    await _usersRepository.Save();
                    return Responses.Ok(new { model.Phone }, "You have updated your account successfully!");
                }

                if (existingUser != null) return Responses.Conflict("Email, Phone number or Username already exists.");

                var user = _mapper.Map<Auth>(model);

                user.secret = BCrypt.Net.BCrypt.HashPassword(model.Password);
                user.enabled = false;
                user.approved = false;
                user.referral_code = model.ReferralCode;
                user.user_on_boarding_id = 0;

                var query = $"Select * from [tbl_auth] where [Phone] = @Phone and consumer = 1 and enabled = 0";

                var queryArgs = new DynamicParameters();
                queryArgs.Add("Phone", model.Phone);

                var tempUser = await Task.FromResult(_dapper.Get<Auth>(query, dbArgs, commandType: CommandType.Text));

                if (tempUser is not null)
                {
                    user.Id = tempUser.Id;
                    user.CreatedOn = tempUser.CreatedOn;
                    _mapper.Map(user, tempUser);
                    _usersRepository.Update(tempUser);
                }
                else await _usersRepository.Create(user);

                var saved = await _usersRepository.Save();

                return saved ? Responses.Ok(new { model.Phone }, "You have signed up successfully!") : Responses.BadRequest(message: "Sorry, we couldn't take you in right now, please try again later");
            }
            catch (CustomException ex)
            {
                Log.Error(ex, ex.Message);
                throw;
            }
        }

        public async Task<ResponsesDTO> VerifyConsumerPassword(VerifyConsumerPasswordDTO verifyConsumerPasswordDTO)
        {
            try
            {
                var user = await _usersRepository.Query().Where(x => x.Id == verifyConsumerPasswordDTO.Id).FirstOrDefaultAsync();

                if (user == null || !BCrypt.Net.BCrypt.Verify(verifyConsumerPasswordDTO.Password, user.secret))
                {
                    return Responses.NotFound("Invalid login details. User not found.");
                }

                user.online = true;
                _usersRepository.Update(user);
                await _usersRepository.Save();

                return Responses.Ok(user);
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
                throw;
            }
        }

        public async Task<(ResponsesDTO, UsersDTO, RefreshTokenRequest)> SignInConsumer(SignInConsumerDTO signInConsumerDTO)
        {
            try
            {
                var sqlQuery = "SELECT * FROM [tbl_auth] WHERE [Phone] = @Phone AND consumer = 1";

                var parameters = new DynamicParameters();
                parameters.Add("Phone", signInConsumerDTO.Phone);
                var user = await Task.FromResult(_dapper.Get<Auth>(sqlQuery, parameters, commandType: CommandType.Text));

                if (user is null) return (Responses.BadRequest(message: "User does not exist"), null, null);

                if (user.IsBlacklisted) return (Responses.BadRequest(message: "This device has been blacklisted, please contact support. Thank you."), null, null);

                if (user.IsDeleted) return (Responses.BadRequest(message: "Your account has been suspended, please contact support. Thank you."), null, null);

                if (!BCrypt.Net.BCrypt.Verify(signInConsumerDTO.Password, user.secret)) return (Responses.Conflict("Invalid password."), null, null);

                bool deviceMatch = (user.phone_model_no == signInConsumerDTO.PhoneModelNo) && (user.imei_no == signInConsumerDTO.IMEINo);
                bool deviceIsDefault = (user.phone_model_no == "Default" && user.imei_no == "Default") || (string.IsNullOrEmpty(user.phone_model_no) && string.IsNullOrEmpty(user.imei_no));

                if (deviceIsDefault && (user.merchant || user.consumer))
                    deviceMatch = true;

                if (!deviceMatch && user.phone != Constants.SETTL_STORE_PHONE) return (Responses.Conflict("You cannot login into your account with this device. Please use the device you used to create your account. If you no longer have access to that device, please contact support."), null, null);

                if (user.online && !deviceMatch && user.phone != Constants.SETTL_STORE_PHONE) return (Responses.BadRequest(message: "You are already signed in."), null, null);

                if (!user.IsBlacklisted)
                {
                    user.imei_no = signInConsumerDTO.IMEINo;
                    user.phone_model_no = signInConsumerDTO.PhoneModelNo;
                }

                if (signInConsumerDTO.FromUSSD)
                {
                    user.last_seen = DateHelper.GetCurrentLocalTime();
                    _usersRepository.Update(user);
                    await _usersRepository.Save();
                    return signInConsumerDTO.Password == user.secret ? (Responses.Ok(user, "USSD"), _mapper.Map<UsersDTO>(user), null) : (Responses.Conflict("Invalid password."), null, null);
                }

                if (user.IsFirstTimeLogin)
                {
                    var onboardingDTO = new OnboardingDTO
                    {
                        Phone = signInConsumerDTO.Phone,
                        Stage = Constants.SIGNIN_USER
                    };

                    await OnboardingService.Create(onboardingDTO, _mapper, _userOnboardingRepository);
                }
                var userDto = _mapper.Map<UserDTO>(user);

                var (refreshTokenRequest, _) = JWT.GenerateJwtToken(signInConsumerDTO.Phone, userDto, Usertype.UserType.CONSUMER.ToString());

                var refreshToken = Hashing.Base64StringEncode($"{refreshTokenRequest.RefreshToken}phone{signInConsumerDTO.Phone}|{signInConsumerDTO.PhoneModelNo}|{signInConsumerDTO.IMEINo}");

                user.JwtToken = refreshTokenRequest.Token;
                user.RefreshToken = Hashing.Base64StringEncode($"{refreshTokenRequest.RefreshToken}phone{user.phone}|{user.phone_model_no}|{user.imei_no}");
                user.last_seen = DateHelper.GetCurrentLocalTime();
                user.online = true;
                user.phone_model_no = signInConsumerDTO.PhoneModelNo;
                user.imei_no = signInConsumerDTO.IMEINo;
                refreshTokenRequest.RefreshToken = user.RefreshToken;
                _usersRepository.Update(user);
                var saved = await _usersRepository.Save();
                var userDTO = _mapper.Map<UsersDTO>(user);
                refreshTokenRequest.RefreshToken = refreshToken;

                _tokenService.SetUserTokens(signInConsumerDTO.Phone, refreshTokenRequest.Token, refreshTokenRequest.RefreshToken);

                return user.phone_name != "USSD" ? (Responses.Ok(user), userDTO, refreshTokenRequest) : (Responses.Conflict("USSD"), userDTO, null);
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
                throw;
            }
        }

        public async Task<ResponsesDTO> ChangePassword(ChangePasswordDTO forgotConsumerPasswordDTO)
        {
            var sqlQuery = "SELECT * FROM [tbl_auth] WHERE [Phone] = @Phone AND active = 1";

            var parameters = new DynamicParameters();
            parameters.Add("Phone", forgotConsumerPasswordDTO.Phone);
            var user = await Task.FromResult(_dapper.Get<Auth>(sqlQuery, parameters, commandType: CommandType.Text));

            if (user is null) return Responses.NotFound("User does not exist");

            if (string.IsNullOrEmpty(user.secret)) return Responses.BadRequest(null, "USER SIGNED UP WITH USSD");

            if (!BCrypt.Net.BCrypt.Verify(forgotConsumerPasswordDTO.OldPassword, user.secret)) return Responses.Conflict("Invalid password.");

            if (BCrypt.Net.BCrypt.Verify(forgotConsumerPasswordDTO.Password, user.secret)) return Responses.Conflict("New password cannot be the same as old password");

            user.secret = BCrypt.Net.BCrypt.HashPassword(forgotConsumerPasswordDTO.Password);

            _usersRepository.Update(user);
            var success = await _usersRepository.Save();

            return success ? Responses.Ok<object>("", "Password changed successfully") : Responses.Conflict("Could not change password");
        }

        public async Task<ResponsesDTO> CreateConsumerSecurityAnswer(List<CreateSecurityAnswerDTO> createSecurityAnswerDTO)
        {
            foreach (var ans in createSecurityAnswerDTO)
            {
                if (string.IsNullOrEmpty(ans.Answer))
                {
                    return Responses.BadRequest(message: "Security answer cannot be empty.");
                }
            }

            List<SecurityAnswer> csa = _mapper.Map<List<SecurityAnswer>>(createSecurityAnswerDTO);

            await _securityAnswerRepository.AddRange(csa);
            await _securityAnswerRepository.Save();

            return Responses.Ok("Security Answer saved successfully.");
        }

        public async Task<ResponsesDTO> VerifyConsumerReferralCode(VerifyReferralCodeDTO verifyReferralCodeDTO)
        {
            string hashCode = BCrypt.Net.BCrypt.HashPassword(verifyReferralCodeDTO.Code);

            Referrals referrals = await _referralsRepository.Query().Where(x => x.RefCode == hashCode).FirstOrDefaultAsync();

            if (referrals == null)
            {
                return Responses.NotFound("Invalid referral code.");
            }

            referrals.Status = true;

            _referralsRepository.Update(referrals);
            await _referralsRepository.Save();

            return Responses.Ok("Referral Code verification successful.");
        }

        public async Task<(ConsumerWalletDTO, List<ConsumerWallet>)> GetConsumerWallets(string phoneNumber, string token)
        {
            var client = new HttpClient();

            client.DefaultRequestHeaders.Add("X-RequestId", Guid.NewGuid().ToString());
            client.DefaultRequestHeaders.Add("X-Settl-Api-Token", Constants.SETTL_API_TOKEN);

            var consumerResponse = await client.GetAsync($"{Constants.CONSUMER_URL}/ConsumerWallet/getbyphoneno?phoneno={phoneNumber}");
            var walletsResponse = await client.GetAsync($"{Constants.CONSUMER_URL}/ConsumerWallet/getallwalletsbyphoneno?phoneno={phoneNumber}");
            var profile = await client.GetAsync($"{Constants.CONSUMER_URL}/consumerprofile/getbyphoneno?phoneno={phoneNumber}");

            if (!(consumerResponse.IsSuccessStatusCode && walletsResponse.IsSuccessStatusCode)) return (null, null);

            var walletResult = await walletsResponse.Content.ReadAsStringAsync();
            var consumerResult = await consumerResponse.Content.ReadAsStringAsync();
            var profileResult = await profile.Content.ReadAsStringAsync();

            Log.Information($"Consumer wallet response - {consumerResult} | HTTP Status Code - {consumerResponse.StatusCode}");
            Log.Information($"Consumer wallets response - {walletResult} | HTTP Status Code - {walletsResponse.StatusCode}");
            Log.Information($"Consumer profile response - {profileResult} | HTTP Status Code - {profile.StatusCode}");

            var walletData = JsonConvert.DeserializeObject<ConsumerWalletsResponseDTO>(walletResult);
            var consumerData = JsonConvert.DeserializeObject<ConsumerWalletResponseDTO>(consumerResult);
            var profileData = JsonConvert.DeserializeObject<ConsumerProfileResponseDTO>(profileResult);

            _mapper.Map(profileData?.Data, consumerData.Data);

            if (walletData is null || consumerData?.Data is null) throw new CustomException(consumerData.Message ?? "Unable to fetch consumer data");

            var walletDataList = walletData.Data?.ToList();
            return (consumerData.Data, walletDataList);
        }

        public Task<ResponsesDTO> GetAgencyWallet(string phoneNumber)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<UserDTO>> Get()
        {
            var sql = $"Select * from [tbl_auth] WHERE deleted = 0";

            var results = await Task.FromResult(_dapper.GetAll<Auth>(sql, null, commandType: CommandType.Text));

            var users = _mapper.Map<IEnumerable<UserDTO>>(results);

            return users;
        }

        public async Task<UserDTO> Get(string phone)
        {
            var sql = $"Select * from [tbl_auth] where [Phone] = @Phone AND consumer = 1";
            var dbArgs = new DynamicParameters();
            dbArgs.Add("Phone", phone);
            var user = await Task.FromResult(_dapper.Get<Auth>(sql, dbArgs, commandType: CommandType.Text));

            var result = _mapper.Map<UserDTO>(user);

            return result;
        }

        public async Task<Auth> GetUser(string phone)
        {
            var sql = $"Select * from [tbl_auth] where [Phone] = @Phone and deleted = 0";
            var dbArgs = new DynamicParameters();
            dbArgs.Add("Phone", phone);
            var user = await Task.FromResult(_dapper.Get<Auth>(sql, dbArgs, commandType: CommandType.Text));

            return user;
        }

        public async Task<(bool, PhoneOSAndFcmTokenModel)> GetFcmTokenAndPhoneOS(string phone)
        {
            var sql = $"Select [FcmToken], [phone_model_no] AS PhoneModelNo from [tbl_auth] WHERE [Phone] = @Phone and deleted = 0";
            var dbArgs = new DynamicParameters();
            dbArgs.Add("Phone", phone);

            var result = await Task.FromResult(_dapper.Get<PhoneNameAndTokenModel>(sql, dbArgs, commandType: CommandType.Text));

            if (result == null)
                return (false, null);

            if (string.IsNullOrEmpty(result.PhoneModelNo) || string.IsNullOrEmpty(result.FcmToken))
                return (false, null);

            var model = new PhoneOSAndFcmTokenModel
            {
                FcmToken = result.FcmToken,
                PhoneOS = GetPhoneOSFromName(result.PhoneModelNo)
            };

            return (true, model);
        }

        public async Task<(bool, string)> UpdateFcmToken(UpdateFcmTokenDTO model)
        {
            try
            {
                var sql = $"Update [tbl_auth] set [FcmToken] = @FcmToken, updated_on = GETDATE() WHERE [phone] = @Phone and deleted = 0";
                var dbArgs = new DynamicParameters();
                dbArgs.Add("Phone", model.Phone);
                dbArgs.Add("FcmToken", model.FcmToken);

                var rows = await Task.FromResult(_dapper.Execute(sql, dbArgs, commandType: CommandType.Text));

                return rows > 0 ? (true, "Fcm token has been updated") : (false, "Error updating fcm token");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<ResponsesDTO> ResetConsumerPassword(ResetConsumerPasswordDTO resetConsumerPasswordDTO)
        {
            var sqlQuery = "SELECT * FROM [tbl_auth] WHERE [Phone] = @Phone AND active = 1";

            var parameters = new DynamicParameters();
            parameters.Add("Phone", resetConsumerPasswordDTO.Phone);
            var user = await Task.FromResult(_dapper.Get<Auth>(sqlQuery, parameters, commandType: CommandType.Text));

            if (user is null) return Responses.NotFound("User does not exist");

            if (string.IsNullOrEmpty(user.secret)) return Responses.BadRequest(null, "USER SIGNED UP WITH USSD");

            if (resetConsumerPasswordDTO.TransactionPin != Constants.DefaultTransactionPIN)
            {
                var client = new HttpClient();
                client.DefaultRequestHeaders.Add("X-RequestId", Guid.NewGuid().ToString());
                client.DefaultRequestHeaders.Add("X-Settl-Api-Token", Constants.SETTL_API_TOKEN);
                var pinDto = new { PhoneNo = resetConsumerPasswordDTO.Phone, resetConsumerPasswordDTO.TransactionPin };
                var consumerResponse = await client.PostAsJsonAsync($"{Constants.CONSUMER_URL}/ConsumerWallet/verifytransactionpin", pinDto);

                if (!consumerResponse.IsSuccessStatusCode && consumerResponse.Content is null) return Responses.Conflict("Unable to connect to consumer service");
                var consumerResult = await consumerResponse.Content.ReadAsStringAsync();
                Log.Information($"Verify pin response = {consumerResult}");
                var response = JsonConvert.DeserializeObject<TransactionPinResponse>(consumerResult);

                if (response.Code != "00") return Responses.Conflict(response.Message);
            }

            if (BCrypt.Net.BCrypt.Verify(resetConsumerPasswordDTO.Password, user.secret)) return Responses.Conflict("New password cannot be the same as old password");

            user.secret = BCrypt.Net.BCrypt.HashPassword(resetConsumerPasswordDTO.Password);

            _usersRepository.Update(user);
            var success = await _usersRepository.Save();

            return success ? Responses.Ok<object>("", "Password reset successful") : Responses.Conflict("Could not reset password");
        }

        private async Task<List<UserDTO>> GetUsers(int? days, string startDate, string endDate)
        {
            var sqlQuery = "SELECT * FROM [tbl_auth] WHERE created_on BETWEEN @start AND @end and deleted = 0";

            var parameters = new DynamicParameters();

            if (string.IsNullOrEmpty(startDate) && string.IsNullOrEmpty(endDate))
            {
                var start = DateTime.Now.AddDays((double)-days).Date;
                var end = days == 0 ? start : DateTime.Now.Date;
                parameters.Add("start", start);
                parameters.Add("end", end);
                Log.Information($"Getting users for {start} to {end}");
            }
            else
            {
                parameters.Add("start", startDate);
                parameters.Add("end", endDate);
                Log.Information($"Getting users for {startDate} to {endDate}");
            }

            var users = await Task.FromResult(_dapper.GetAll<Auth>(sqlQuery, parameters, commandType: CommandType.Text));

            var result = _mapper.Map<List<UserDTO>>(users);

            return result;
        }

        private async Task<List<UserDTO>> GetPreviousUsers(int? days, string startDate, string userEndDate)
        {
            var span = !string.IsNullOrEmpty(userEndDate) ? (DateTime.Parse(userEndDate).Date - DateTime.Parse(startDate).Date).Days : 0;

            var prevStartDate = string.IsNullOrEmpty(userEndDate) ? DateTime.Parse(startDate).AddDays((double)-days).ToString("yyyy-MM-dd") : DateTime.Parse(startDate).AddDays(-span).ToString("yyyy-MM-dd");
            var prevEndDate = !string.IsNullOrEmpty(userEndDate) ? startDate : days == 1 ? prevStartDate : startDate;

            var users = await GetUsers(days, prevStartDate, prevEndDate);

            return users;
        }

        public async Task<UsersDateFilterResponse> GetUsersFilter(int? days, string start, string end)
        {
            var startDate = days is not null ? DateTime.Now.AddDays((double)-days).ToString("yyyy-MM-dd") : start;

            var users = await GetUsers(days, start, end);
            var prevUsers = await GetPreviousUsers(days, startDate, end);

            return new UsersDateFilterResponse { Users = users, PreviousUsers = prevUsers };
        }

        public async Task<(bool, string)> FreezeWallet(FreezeWalletDTO request)
        {
            var admin = await _admin.GetUser(request.AdminEmail, "");

            if (admin is null) return (false, "Admin does not exist");

            var user = await Get(request.CustomerPhone);

            if (user is null) return (false, "Customer does not exist");

            if (!request.Freeze) return await UnfreezeWallet(request.CustomerPhone);

            var sql = $"UPDATE [tbl_auth] SET deleted = 1 updated_on = GETDATE() WHERE Phone = @Phone";

            // call consumer endpoint to set wallet inactive

            var dbArgs = new DynamicParameters();
            dbArgs.Add("Phone", request.CustomerPhone);

            var rows = await Task.FromResult(_dapper.Execute(sql, dbArgs, commandType: CommandType.Text));

            return rows > 0 ? (true, "Customer wallet has been frozen") : (false, "Error freezing customer wallet");
        }

        private async Task<(bool, string)> UnfreezeWallet(string phone)
        {
            var sql = $"UPDATE tblUsers SET deleted = 0, updated_on = GETDATE() WHERE Phone = @Phone";

            // call consumer endpoint to set wallet inactive

            var dbArgs = new DynamicParameters();
            dbArgs.Add("Phone", phone);

            var rows = await Task.FromResult(_dapper.Execute(sql, dbArgs, commandType: CommandType.Text));

            return rows > 0 ? (true, "Customer wallet has been unfrozen") : (false, "Error unfreezing customer wallet");
        }

        public async Task<(bool, string)> BlacklistDevice(BlacklistDeviceDTO request)
        {
            var admin = await _admin.GetUser(request.AdminEmail, "");

            if (admin is null) return (false, "Admin does not exist");

            var user = await Get(request.CustomerPhone);

            if (user is null) return (false, "Customer does not exist");

            if (!request.Blacklist) return await UnblacklistDevice(request.CustomerPhone);

            var sql = $"UPDATE [tbl_auth] SET IsBlacklisted = 1, updated_on = GETDATE() WHERE Phone = @Phone";
            var dbArgs = new DynamicParameters();
            dbArgs.Add("Phone", request.CustomerPhone);

            var rows = await Task.FromResult(_dapper.Execute(sql, dbArgs, commandType: CommandType.Text));

            return rows > 0 ? (true, "Customer has been blacklisted") : (false, "Error blacklisting customer");
        }

        private async Task<(bool, string)> UnblacklistDevice(string phone)
        {
            var sql = $"UPDATE [tbl_auth] SET IsBlacklisted = 0, updated_on = GETDATE() WHERE Phone = @Phone";
            var dbArgs = new DynamicParameters();
            dbArgs.Add("Phone", phone);

            var rows = await Task.FromResult(_dapper.Execute(sql, dbArgs, commandType: CommandType.Text));

            return rows > 0 ? (true, "Customer's device has been whitelisted") : (false, "Error whitelisting customer's device");
        }

        private static string GetPhoneOSFromName(string phoneName)
        {
            phoneName = phoneName.ToLower();

            if (phoneName == "ussd" || phoneName == "blacklisted" || string.IsNullOrEmpty(phoneName))
                return "";

            if (phoneName.Contains("iphone"))
                return PHONE_OS.PhoneOS.IOS.ToString();
            else return PHONE_OS.PhoneOS.ANDROID.ToString();
        }

        public async Task<bool> IsBlacklistedNumber(string phone)
        {
            var sql = $"Select * from [tbl_auth] where [Phone] = @Phone";
            var dbArgs = new DynamicParameters();
            dbArgs.Add("Phone", phone);
            var user = await Task.FromResult(_dapper.Get<Auth>(sql, dbArgs, commandType: CommandType.Text));

            return user is not null && user.IsBlacklisted;
        }

        public async Task<AppDownloads> AppDownloads(int days, string startDate, string endDate)
        {
            var response = new List<UserDTO>();
            if (startDate is null && endDate is null)
            {
                var startDay = DateAndTimeHelper.GetCurrentServerTime().AddDays(-days).Date;
                var endDay = days == 0 ? startDay : DateAndTimeHelper.GetCurrentServerTime().Date;
                response = await GetUsers(days, startDay.ToString(), endDay.ToString());
            }
            else
            {
                response = await GetUsers(days, startDate, endDate);
            }

            var users = response.Where(user => !string.IsNullOrEmpty(user.phone_model_no) && user.phone_model_no != "USSD").ToList();

            var iosConsumers = users.Where(user => user.Consumer == true && user.phone_model_no.Contains("iPhone")).ToList().Count;
            var androidConsumers = users.Where(user => user.Consumer == true && user.phone_model_no.Contains("iPhone")).ToList().Count;

            //var iosAgents = users.Where(user => user.UserTypeId < 4 && user.PhoneModelNo.Contains("iPhone")).ToList().Count;
            //var androidAgents = users.Where(user => user.UserTypeId < 4 && !user.PhoneModelNo.Contains("iPhone")).ToList().Count;

            return new AppDownloads
            {
                Consumers = new Downloads { AppStore = iosConsumers, PlayStore = androidConsumers },
                Agents = new Downloads { AppStore = 0, PlayStore = 0 }
            };
        }

        public async Task<bool> SignOutConsumer(string phone)
        {
            var sql = $"UPDATE [tbl_auth] SET online = 0, last_seen = @datetime, JwtToken = @token, RefreshToken = @token, updated_on = @datetime WHERE Phone = @Phone";
            var dbArgs = new DynamicParameters();
            dbArgs.Add("Inactive", Constants.InActiveStatus);
            dbArgs.Add("datetime", DateAndTimeHelper.GetCurrentServerTime());
            dbArgs.Add("Phone", phone);
            dbArgs.Add("token", string.Empty);

            var rows = await Task.FromResult(_dapper.Execute(sql, dbArgs, commandType: CommandType.Text));

            _tokenService.RemoveUserTokens(phone);

            return rows > 0;
        }

        public void SignOut(string phone)
        {
            try
            {
                SignOutConsumer(phone).Wait();
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
                throw;
            }
        }

        public async Task<bool> PhoneExists(CreateConsumerDTO consumer)
        {
            var sql = $"SELECT * FROM [tbl_auth] WHERE imei_no = @IMEI";
            var dsql = $"SELECT * FROM [tbl_auth] WHERE phone_name LIKE @PhoneName";

            var dbArgs = new DynamicParameters();
            dbArgs.Add("PhoneName", consumer.PhoneName);
            dbArgs.Add("PhoneModel", consumer.PhoneModelNo);
            dbArgs.Add("IMEI", consumer.IMEINo);

            var user = await Task.FromResult(_dapper.Get<Auth>(sql, dbArgs, commandType: CommandType.Text));
            var puser = await Task.FromResult(_dapper.Get<Auth>(dsql, dbArgs, commandType: CommandType.Text));

            if (user is null && puser is null) return false;

            if (user is not null && user.IsDeleted) return true;

            if (puser is not null && !puser.IsActive) return true;

            if (user is null) return false;

            if (user.phone_name == consumer.PhoneName && user.phone_model_no == consumer.PhoneModelNo && user.imei_no == consumer.IMEINo) return true;

            if (user.phone_model_no == consumer.PhoneModelNo && user.imei_no == consumer.IMEINo) return true;

            if (user.imei_no == consumer.IMEINo) return true;

            if (puser.phone_name == consumer.PhoneName && puser.phone_model_no == consumer.PhoneModelNo && puser.imei_no == consumer.IMEINo) return true;

            if (puser.phone_model_no == consumer.PhoneModelNo || puser.imei_no == consumer.IMEINo) return true;

            if (puser.imei_no == consumer.IMEINo) return true;

            return false;
        }
    }
}