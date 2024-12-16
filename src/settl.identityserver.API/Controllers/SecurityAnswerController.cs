using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using settl.identityserver.Application.Contracts.DTO.PushNotification;
using settl.identityserver.Application.Contracts.DTO.SecurityAnswer;
using settl.identityserver.Application.Contracts.IServices;
using settl.identityserver.Application.Services;
using settl.identityserver.Domain.Shared.Helpers;
using settl.identityserver.Domain.Shared.Helpers.Cryptography;
using System.Linq;
using System.Threading.Tasks;

namespace settl.identityserver.API.Controllers
{
    [Route("identityserver")]
    public class SecurityAnswerController : BaseApiController
    {
        private readonly ISecurityAnswerService _securityAnswerService;
        private readonly ITransactionService _transactionService;
        private readonly IUserService _userService;
        private readonly IEmailService _emailService;

        public SecurityAnswerController(IUserService userService, ITransactionService transactionService, ISecurityAnswerService securityAnswerService, IHttpContextAccessor httpContextAccessor, IEmailService emailService) : base(httpContextAccessor)
        {
            _userService = userService;
            _securityAnswerService = securityAnswerService;
            _transactionService = transactionService;
            _emailService = emailService;
        }

        [Route("security-answer/create")]
        [HttpPost]
        public async Task<IActionResult> CreateSecurityAnswer([FromBody] CreateSecurityAnswerForm request)
        {
            var responses = CustomApiResponse.Get();
            if (!ModelState.IsValid)
            {
                var modelErrors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage).ToList();
                return ApiBad(modelErrors, modelErrors[0]);
            }

            Log.Information(SerializeUtility.SerializeJSON(request));

            try
            {
                var isListed = await _userService.IsBlacklistedNumber(request.Phone);

                if (isListed) return ApiBad(null, message: "This number has been blacklisted");

                var (_, refreshTokenRequest) = await _securityAnswerService.CreateConsumerSecurityAnswer(request);
                var ussdUser = await _userService.Get(request.Phone);
                var (Consumer, Wallets) = await _userService.GetConsumerWallets(request.Phone, refreshTokenRequest.Token);
                Consumer.UserName = ussdUser.UserName;
                Consumer.Gender = ussdUser.Gender;
                var (transactions, message) = await _transactionService.Get(request.Phone);
                var refreshToken = Hashing.Base64StringEncode($"{refreshTokenRequest.RefreshToken}phone{request.Phone}|{ussdUser?.phone_model_no}|{ussdUser?.Imei_no}");

                var success = await _emailService.SendRegistrationEmail(ussdUser, Consumer.IsReferred);

                if (!success) Log.Information("Failed to send new user email");

                await PushNotificationService.SendAsync(new PushNotificationRequestDTO
                {
                    Phone = Consumer.PhoneNo,
                    Title = $"Welcome to Settl, {Consumer.FirstName}🤩",
                    Body = $"Hello {Consumer.FirstName}👋  Welcome on board, we are super excited to have you here! 😃 \n" +
                            $"Start transacting immediately by funding your wallet via your unique NUBAN or with your card.\n" +
                            $"Save, pay bills, send & request money and make payments with your Settl card all in one App. Cool right ?😆 \n" +
                            $"There's more, refer a friend and earn a bonus! Your friend gets to earn too🤩 " +
                            $"\n\nLet us know if you need help setting up, we are a call / message away!📞",
                    Type = Domain.Shared.Enums.PUSHNOTIFICATION_TYPE.PushNotificationType.FIRST_TIME_LOGIN
                });

                if (Consumer.IsReferred)
                {
                    await PushNotificationService.SendAsync(new PushNotificationRequestDTO
                    {
                        Phone = Consumer.PhoneNo,
                        Title = "Referral Bonus Update🚨💸",
                        Body = $"Hi {StringUtility.Capitalize(Consumer.FirstName)}👋 You have a referral bonus of N500🥳 Fund your wallet with at least N2,000, pay a bill(DSTV/GOTV) or upgrade your account by securely providing your BVN to claim your bonus.",
                        Type = Domain.Shared.Enums.PUSHNOTIFICATION_TYPE.PushNotificationType.FIRST_TIME_LOGIN
                    });
                }

                return ApiOk(new
                {
                    AccessToken = refreshTokenRequest.Token,
                    refreshTokenRequest.ExpiresIn,
                    TokenType = "Bearer",
                    refreshToken,
                    Consumer,
                    Wallets,
                    transactions
                });
            }
            catch (CustomException ex)
            {
                Log.Error(ex.Message);
                return ApiBad(null, ex.Message);
            }
        }
    }
}