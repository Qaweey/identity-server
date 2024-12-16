using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using settl.identityserver.Application.Contracts.DTO.OTP;
using settl.identityserver.Application.Contracts.IServices;
using settl.identityserver.Domain.Shared.Helpers;
using System.Linq;
using System.Threading.Tasks;

namespace settl.identityserver.API.Controllers
{
    [ApiController]
    [Route("identityserver")]
    public class OTPController : BaseApiController
    {
        private readonly IOTPService _otpService;
        private readonly IUserService _userService;

        public OTPController(IOTPService otpService, IHttpContextAccessor httpContextAccessor, IUserService userService) : base(httpContextAccessor)
        {
            _otpService = otpService;
            _userService = userService;
        }

        /// <summary>
        /// Send One Time Passwords to Settl Users to complete authenticated processes
        /// </summary>
        ///<remarks>For OtpTypes, SIGN_IN = 1, PHONE_NUMBER_VALIDATION = 2, FORGOT_PASSWORD = 3, CHANGE_PASSWORD=4, 5=REGISTER_NEWDEVICE</remarks>
        ///<param></param>
        [Route("send/otp")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SendSMSDTO request)
        {
            var responses = CustomApiResponse.Get();
            IActionResult dataResult = null;

            if (!ModelState.IsValid)
            {
                var modelErrors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage).ToList();
                dataResult = ApiBad(modelErrors, modelErrors[0]);
                return dataResult;
            }

            Log.Information(SerializeUtility.SerializeJSON(request));

            try
            {
                var isListed = await _userService.IsBlacklistedNumber(request.Phone);

                if (isListed) return ApiBad(null, message: "This number has been blacklisted");

                var result = await _otpService.SendOTP(request);

                dataResult = ApiOk(result);
            }
            catch (CustomException ex)
            {
                dataResult = ApiBad(null, ex.Message);
            }

            return dataResult;
        }

        /// <summary>
        /// Verify OTPs sent to customers
        /// </summary>
        /// <param name="verifyOTPDTO"></param>
        /// <returns></returns>
        [HttpPost("verify/otp")]
        public async Task<IActionResult> Verify([FromBody] VerifyOTPDTO verifyOTPDTO)
        {
            if (!ModelState.IsValid)
            {
                var modelErrors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage).ToList();
                return ApiBad(modelErrors, modelErrors[0]);
            }

            Log.Information(SerializeUtility.SerializeJSON(verifyOTPDTO));

            var otp = await _otpService.VerifyOTP(verifyOTPDTO);

            return otp.Code == "00" ? Ok(otp) : BadRequest(otp);
        }
    }
}