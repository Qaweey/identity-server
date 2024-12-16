using AutoMapper;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using settl.identityserver.Appication.Contracts.RepositoryInterfaces;
using settl.identityserver.Application.Contracts.DTO.Admin;
using settl.identityserver.Application.Contracts.DTO.Consumer;
using settl.identityserver.Application.Contracts.DTO.Referral;
using settl.identityserver.Application.Contracts.DTO.SecurityAnswer;
using settl.identityserver.Application.Contracts.DTO.Users;
using settl.identityserver.Application.Contracts.IServices;
using settl.identityserver.Domain.Entities;
using settl.identityserver.Domain.Shared;
using settl.identityserver.Domain.Shared.Helpers;
using settl.identityserver.Domain.Shared.Helpers.Authentication;
using settl.identityserver.Domain.Shared.Helpers.Cryptography;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using settl.identityserver.Domain.Shared.Enums;
using static settl.identityserver.Domain.Shared.Helpers.CustomApiResponse;

namespace settl.identityserver.API.Controllers
{
    [ApiController]
    [EnableCors("AllowAll")]
    [Route("identityserver/consumer")]
    public class ConsumerController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly IConsumerService _consumer;
        private readonly ITokenService _tokenService;
        private readonly ITransactionService _transactionService;
        private readonly IGenericRepository<Auth> _usersRepository;
        private readonly ISecurityQuestionService _securityQuestionService;
        private readonly Dictionary<Status, string> CustomResponses = Get();

        public ConsumerController(IMapper mapper, ITransactionService transactionService, IUserService userService, ITokenService tokenService, IGenericRepository<Auth> usersRepository, IConsumerService consumer, ISecurityQuestionService securityQuestionService)
        {
            _mapper = mapper;
            _consumer = consumer;
            _userService = userService;
            _tokenService = tokenService;
            _usersRepository = usersRepository;
            _transactionService = transactionService;
            _securityQuestionService = securityQuestionService;
        }

        /// <summary>
        /// Create a consumer user on Settl
        /// </summary>
        /// <param name="createUserDTO"></param>
        /// <returns></returns>
        [HttpPost("signup")]
        public async Task<IActionResult> SignUp([FromBody] CreateConsumerDTO createUserDTO)
        {
            if (!ModelState.IsValid)
            {
                var modelErrors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage).ToList();
                return BadRequest(Responses.BadRequest(modelErrors, modelErrors[0]));
            }

            if (createUserDTO.Password.Length < 8) return BadRequest(Responses.BadRequest(message: "Password must be at least 8 characters"));

            Log.Information($"Consumer sign up");

            var phoneExists = await _userService.PhoneExists(createUserDTO);

            if (phoneExists) return BadRequest(Responses.BadRequest(message: "This device has already been used to create an account."));

            var user = _mapper.Map<CreateUsersDTO>(createUserDTO);
            user.Consumer = true;
            createUserDTO.Email = createUserDTO.Email.Trim();
            createUserDTO.FirstName = createUserDTO.FirstName.Trim();
            createUserDTO.LastName = createUserDTO.LastName.Trim();
            createUserDTO.Password = createUserDTO.Password.Trim();
            createUserDTO.UserName = createUserDTO.UserName.Trim();
            createUserDTO.Phone = createUserDTO.Phone.Trim();
            var signup = await _userService.SignUpUser(user);

            return signup.Code == CustomResponses[Status.SUCCESS] ? Ok(Responses.Ok(signup.Data, signup.Message)) : Conflict(Responses.Conflict(signup.Message));
        }

        /// <summary>
        /// Verifying transaction pin and sending OTP to register new device
        /// </summary>
        /// <param name="verifypin"></param>
        /// <returns></returns>
        [HttpPost("VerifyPin")]
        public async Task<IActionResult> VerifyTransactionPin([FromBody] VerifyTransPin verifypin)
        {
            try
            {
                var useremail = await _consumer.VerifyPinAndSendOTP(verifypin.Phoneno, verifypin.TransactionPin);
                return Ok(Responses.Ok(useremail, "Email sent successfully"));
            }
            catch (CustomException ex)
            {
                return BadRequest(Responses.BadRequest(message: ex.Message));
            }
        }

        /// <summary>
        /// to Register new device
        /// </summary>
        /// <param name="newDeviceDto"></param>
        /// <returns></returns>
        [HttpPost("registernewdevice")]
        public async Task<IActionResult> RegisterNewDevice([FromBody] RegisterNewDeviceDto newDeviceDto)
        {
            try
            {
                var createdDevice = await _consumer.RegisterNewDevice(newDeviceDto);
                if (createdDevice.Code != "00")
                {
                    return BadRequest(Responses.BadRequest(message: createdDevice.Message));
                }
                var refreshTokenRequest = (RefreshTokenRequest)createdDevice.Data;

                var (Consumer, Wallets) = await _userService.GetConsumerWallets(newDeviceDto.PhoneNo, refreshTokenRequest.Token);
                var (transactions, _) = await _transactionService.Get(newDeviceDto.PhoneNo);

                return Ok(Responses.Ok(new
                {
                    AccessToken = refreshTokenRequest.Token,
                    refreshTokenRequest.ExpiresIn,
                    tokenType = "Bearer",
                    refreshTokenRequest.RefreshToken,
                    Consumer,
                    Wallets,
                    transactions
                }));
            }
            catch (CustomException ex)
            {
                return BadRequest(Responses.BadRequest(message: ex.Message));
            }
        }

        /// <summary>
        /// Log consumers into their mobile app
        /// </summary>
        /// <param name="signInConsumerDTO"></param>
        /// <returns></returns>
        [HttpPost("signin")]
        public async Task<IActionResult> Login([FromBody] SignInConsumerDTO signInConsumerDTO)
        {
            if (!ModelState.IsValid)
            {
                var modelErrors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage).ToList();
                return BadRequest(Responses.BadRequest(modelErrors, modelErrors[0]));
            }

            Log.Information($"Consumer sign in");

            var (resp, user, refreshTokenRequest) = await _userService.SignInConsumer(signInConsumerDTO);
            if (user is null && resp.Data is null or "") return BadRequest(resp);

            if (user is null) return BadRequest(resp);

            var (Consumer, Wallets) = await _userService.GetConsumerWallets(signInConsumerDTO.Phone, refreshTokenRequest.Token);
            var (transactions, _) = await _transactionService.Get(signInConsumerDTO.Phone);

            if (resp.Message == "USSD")
            {
                Consumer.FromUSSD = true;
                return Ok(Responses.Ok(new
                {
                    AccessToken = refreshTokenRequest.Token,
                    refreshTokenRequest.ExpiresIn,
                    tokenType = "Bearer",
                    refreshTokenRequest.RefreshToken,
                    Consumer,
                    Wallets,
                    transactions
                }));
            }

            if (Consumer is not null) Consumer.UserName = user?.UserName?.Trim();

            return Ok(Responses.Ok(new
            {
                AccessToken = refreshTokenRequest.Token,
                refreshTokenRequest.ExpiresIn,
                tokenType = "Bearer",
                refreshTokenRequest.RefreshToken,
                Consumer,
                Wallets,
                transactions
            }));
        }

        /// <summary>
        /// Log consumers out of their mobile app
        /// </summary>
        /// <param name="phone"></param>
        /// <returns></returns>
        [HttpPost("signout")]
        public async Task<IActionResult> SignOutConsumer([FromQuery, Required] string phone)
        {
            if (string.IsNullOrEmpty(phone)) return BadRequest(Responses.BadRequest(message: "Phone number is required"));

            var user = await _userService.Get(phone);

            if (user is null) return BadRequest(Responses.BadRequest(message: "This user does not exist."));

            var success = await _userService.SignOutConsumer(phone);

            if (!success) return BadRequest(Responses.BadRequest(message: "An unexpected error occurred."));

            return Ok(Responses.Ok(""));
        }

        /// <summary>
        ///  Returns consumer's new information when users refresh their home screen
        /// </summary>
        /// <returns></returns>
        [HttpGet("refreshpage")]
        public async Task<IActionResult> PullDownToRefresh()
        {
            var (token, err) = StringUtility.HasJWTToken(HttpContext.Request);

            if (err) return Unauthorized(Responses.BadRequest(message: token));

            var (hasError, message, validatedToken) = _tokenService.ProcessTokenValidation(token, false, true);

            if (hasError) return Unauthorized(Responses.BadRequest(message: message));

            var phone = _tokenService.GetTokenClaims(validatedToken);
            var userType = _tokenService.GetTokenUserTypeClaims(validatedToken);

            var user = await _userService.Get(phone);

            if (user is null) return Unauthorized(Responses.BadRequest(message: "User does not exist."));
            var (refreshTokenRequest, _) = JWT.GenerateJwtToken(user.Phone, user, userType);

            var (Consumer, Wallets) = await _userService.GetConsumerWallets(user.Phone, refreshTokenRequest.Token);
            var (transactions, _) = await _transactionService.Get(user.Phone);

            return Ok(Responses.Ok(new
            {
                Consumer,
                Wallets,
                transactions
            }));
        }

        /// <summary>
        /// Allows consumers to reset their password with transaction pin
        /// </summary>
        /// <param name="resetPassword"></param>
        /// <returns></returns>
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ResetConsumerPasswordDTO resetPassword)
        {
            if (!ModelState.IsValid)
            {
                var modelErrors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage).ToList();
                return BadRequest(Responses.BadRequest(modelErrors, modelErrors[0]));
            }

            Log.Information($"Consumer forgot password - {SerializeUtility.SerializeJSON(resetPassword)}");

            var forgot = await _userService.ResetConsumerPassword(resetPassword);
            if (forgot.Code != "00") return Conflict(forgot);
            return Ok(forgot);
        }

        /// <summary>
        /// Allows consumers to change their password to a new one
        /// </summary>
        /// <param name="forgotConsumerPasswordDTO"></param>
        /// <returns></returns>
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDTO forgotConsumerPasswordDTO)
        {
            if (!ModelState.IsValid)
            {
                var modelErrors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage).ToList();
                return BadRequest(Responses.BadRequest(modelErrors, modelErrors[0]));
            }

            var forgot = await _userService.ChangePassword(forgotConsumerPasswordDTO);
            if (forgot.Code != "00") return Conflict(forgot);
            return Ok(forgot);
        }

        /// <summary>
        /// Authenticate consumers using previously obtained refresh token during log in and sign up
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        [HttpPost("refreshtoken")]
        public async Task<IActionResult> RefreshConsumerToken([FromBody] RefreshTokenDTO payload)
        {
            if (!ModelState.IsValid)
            {
                var modelErrors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage).ToList();
                return BadRequest(Responses.BadRequest(modelErrors, modelErrors[0]));
            }

            if (!Hashing.IsBase64String(payload.RefreshToken)) return BadRequest(Responses.BadRequest(message: "Invalid Refresh Token"));

            var decrypted = Hashing.Base64StringDecode(payload.RefreshToken).Split("phone");

            var phoneNumber = decrypted[1].Split("|")[0];
            var phoneModel = decrypted[1].Split("|")[1];
            var IMEI = decrypted[1].Split("|")[2];

            var user = await _userService.GetUser(phoneNumber);

            if (user is null) return Unauthorized(Responses.BadRequest(message: "User does not exist"));

            if (user.RefreshToken != payload.RefreshToken) return Unauthorized(Responses.BadRequest(message: "Refresh token expired"));

            if (user.IsDeleted) return Unauthorized(Responses.BadRequest(message: "User has been frozen"));

            if (user.IsBlacklisted) return Unauthorized(Responses.BadRequest(message: "This device has been blacklisted, please contact support. Thank you."));

            if (string.IsNullOrEmpty(IMEI) || string.IsNullOrEmpty(phoneModel)) return Unauthorized(Responses.BadRequest(message: "Please login with your password"));

            var deviceMatch = user.phone_model_no == phoneModel && user.imei_no == IMEI;

            if (user.online && !deviceMatch && user.phone != Constants.SETTL_STORE_PHONE) return Unauthorized(Responses.BadRequest(message: "You are already signed in."));

            if (!deviceMatch && user.phone != Constants.SETTL_STORE_PHONE) return Unauthorized(Responses.BadRequest(message: "You cannot login into your account with this device. Please use the device you used to create your account. If you no longer have access to that device, please contact support."));

            var userDTO = _mapper.Map<UserDTO>(user);

            var (refreshTokenRequest, token) = JWT.GenerateJwtToken(phoneNumber, userDTO);

            var refreshToken = Hashing.Base64StringEncode($"{refreshTokenRequest.RefreshToken}phone{user.phone}|{user.phone_model_no}|{user.imei_no}");

            user.last_seen = DateHelper.GetCurrentLocalTime();
            user.online = true;
            user.JwtToken = refreshTokenRequest.Token;
            user.RefreshToken = refreshToken;

            _usersRepository.Update(user);
            await _usersRepository.Save();

            _tokenService.SetUserTokens(phoneNumber, refreshTokenRequest.Token, refreshToken);

            var (Consumer, Wallets) = await _userService.GetConsumerWallets(phoneNumber, refreshTokenRequest.Token);
            var (transactions, message) = await _transactionService.Get(phoneNumber);

            return Ok(Responses.Ok(new
            {
                AccessToken = refreshTokenRequest.Token,
                refreshTokenRequest.ExpiresIn,
                TokenType = "Bearer",
                refreshToken,
                Consumer,
                Wallets,
                transactions
            }));
        }

        /// <summary>
        /// Freeze/Unfreeze a consumer's wallet
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("freezewallet")]
        public async Task<IActionResult> FreezeWallet([FromBody] FreezeWalletDTO request)
        {
            if (!ModelState.IsValid)
            {
                var modelErrors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage).ToList();
                return BadRequest(Responses.BadRequest(modelErrors, modelErrors[0]));
            }

            var (success, response) = await _userService.FreezeWallet(request);

            return success ? Ok(Responses.Ok(response)) : Conflict(Responses.Conflict(response));
        }

        /// <summary>
        /// Blacklist/Whitelist a consumer's device
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("blacklistdevice")]
        public async Task<IActionResult> BlackListDevice([FromBody] BlacklistDeviceDTO request)
        {
            if (!ModelState.IsValid)
            {
                var modelErrors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage).ToList();
                return BadRequest(Responses.BadRequest(modelErrors, modelErrors[0]));
            }

            Log.Information($"Consumer blacklist device - {SerializeUtility.SerializeJSON(request)}");
            var (success, response) = await _userService.BlacklistDevice(request);

            return success ? Ok(Responses.Ok(response)) : Conflict(Responses.Conflict(response));
        }

        [HttpPost("verify/referral-code")]
        public async Task<IActionResult> VerifyRefferalCode([FromBody] VerifyReferralCodeDTO verifyReferralCodeDTO)
        {
            var referral = await _userService.VerifyConsumerReferralCode(verifyReferralCodeDTO);

            return Ok(Responses.Ok(referral, "Referral Code verification successfully."));
        }

        /// <summary>
        /// Verify answers to the security questions a consumer set up during registration
        /// </summary>
        /// <param name="answerForm"></param>
        /// <returns></returns>
        [HttpPost("verify-securityanswer")]
        public async Task<IActionResult> VerifySecurityAnswers([FromBody] VerifyAnswerForm answerForm)
        {
            if (!ModelState.IsValid)
            {
                var modelErrors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage).ToList();
                return BadRequest(Responses.BadRequest(modelErrors, modelErrors[0]));
            }

            Log.Information($"Consumer verify security answers ");
            var (success, response) = await _securityQuestionService.VerifyAnswers(answerForm);

            return success ? Ok(Responses.Ok(response)) : BadRequest(Responses.Conflict(response));
        }
    }
}