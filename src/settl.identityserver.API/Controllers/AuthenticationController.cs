using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using settl.identityserver.Application.Contracts.DTO.Users;
using settl.identityserver.Application.Contracts.IServices;
using System.Threading.Tasks;

namespace settl.identityserver.API.Controllers
{
    [ApiController]
    [Route("identityserver/validate")]
    public class AuthenticationController : BaseApiController
    {
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly IAdminService _adminService;

        public AuthenticationController(IUserService userService, IMapper mapper, ITokenService tokenService, IHttpContextAccessor httpContextAccessor, IAdminService adminService) : base(httpContextAccessor)
        {
            _tokenService = tokenService;
            _mapper = mapper;
            _userService = userService;
            _adminService = adminService;
        }

        [HttpPost("authentication/accessToken")]
        public async Task<IActionResult> GetToken([FromBody] AccessTokenDTO request)
        {
            if (!ModelState.IsValid) return ApiBadModel(ModelState);

            var response = await _tokenService.GetClientToken(request.Username, request.Password);

            if (response.IsError) return ApiConflict(response.ErrorDescription);

            var result = _mapper.Map<TokenResponseDTO>(response);
            result.ExpriresIn = response.ExpiresIn;

            return ApiOk(result);
        }

        /// <summary>
        /// Obtain new access token after expiry using the refresh token that came with the expired access token
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("authentication/refreshtoken")]
        public async Task<IActionResult> GetRefreshToken([FromBody] RefreshTokenDTO request)
        {
            var response = await _tokenService.UseRefreshToken(request.RefreshToken);

            if (response.IsError && response.ErrorDescription is null) return ApiConflict(response.Error);

            if (response.IsError) ApiConflict(response.ErrorDescription);

            var result = _mapper.Map<TokenResponseDTO>(response);

            result.ExpriresIn = response.ExpiresIn;
            return ApiOk(result);
        }

        /// <summary>
        /// Validates a settl user's access token
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("accesstoken")]
        public async Task<IActionResult> ValidateToken(string token)
        {
            var (hasError, errorMessage, validatedToken) = _tokenService.ProcessTokenValidation(token, false, true);

            if (errorMessage == "Your session has ended. Please re-login")
            {
                var phoneNo = _tokenService.GetTokenClaims(validatedToken);
                var success = await _userService.SignOutConsumer(phoneNo);

                if (!success) Log.Error("Error signing out consumer");
            }

            if (hasError) return ApiUnauthorized(errorMessage);

            var phone = _tokenService.GetTokenClaims(validatedToken);

            var user = await _userService.GetUser(phone);

            if (user is null) return ApiUnauthorized("Caller is not authorized to make use of the API service.");

            if (token != user.JwtToken) return ApiUnauthorized("Your session has ended. Please re-login.");

            return ApiOk(user);
        }

        /// <summary>
        /// Validates an admin user's access token
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("admin/accesstoken")]
        public async Task<IActionResult> ValidateAccessToken(string token)
        {
            var (hasError, errorMessage, validatedToken) = _tokenService.ProcessTokenValidation(token, false, true);

            if (hasError) return ApiUnauthorized(errorMessage);
            var phone = _tokenService.GetTokenClaims(validatedToken);
            var admin = await _adminService.GetAdminUser(0, phone: phone);

            if (admin is null) return ApiUnauthorized("Caller is not authorized to make use of the API service.");

            return ApiOk(admin);
        }
    }
}