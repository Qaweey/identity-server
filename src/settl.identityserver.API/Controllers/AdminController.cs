using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using settl.identityserver.Application.Contracts.DTO.Admin;
using settl.identityserver.Application.Contracts.IServices;
using settl.identityserver.Domain.Shared.Enums;
using settl.identityserver.Domain.Shared.Helpers;
using settl.identityserver.Domain.Shared.Helpers.Authentication;
using settl.identityserver.Domain.Shared.Helpers.Cryptography;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace settl.identityserver.API.Controllers
{
    [Route("identityserver/admin")]
    [ApiController]
    public class AdminController : BaseApiController
    {
        private readonly IAdminService _adminService;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;

        public AdminController(IMapper mapper, IAdminService adminService, IHttpContextAccessor httpContextAccessor, ITokenService tokenService) : base(httpContextAccessor)
        {
            _mapper = mapper;
            _adminService = adminService;
            _tokenService = tokenService;
        }

        /// <summary>
        /// Creates an admin
        /// </summary>
        /// <param name="createAdminDTO"></param>
        /// <returns></returns>
        [HttpPost("create")]
        public async Task<IActionResult> CreateAdmin([FromBody] CreateAdminDTO createAdminDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var modelErrors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage).ToList();
                    return ApiBad(modelErrors, modelErrors[0]);
                }

                var requiredHeader = _httpContextAccessor.HttpContext.Request.Headers["X-ADMINEMAIL"];
                System.Console.WriteLine(requiredHeader);
                if (requiredHeader.Count == 0 || !Regex.IsMatch(requiredHeader, @"[^@ \t\r\n]+@[^@ \t\r\n]+\.[^@ \t\r\n]+")) return ApiUnauthorized("Unauthorized to create this admin");

                createAdminDTO.Fullname = createAdminDTO.Fullname.Trim();
                createAdminDTO.Email = createAdminDTO.Email.Trim();
                createAdminDTO.PhoneNumber = createAdminDTO.PhoneNumber.Trim();
                createAdminDTO.Department = createAdminDTO.Department.Trim();
                var (success, admin) = await _adminService.CreateAdmin(createAdminDTO);

                return success ? ApiOk(admin.Data, admin.Message) : ApiConflict(admin?.Message);
            }
            catch (CustomException ex)
            {
                Log.Error(ex.Message);
                return ApiBad(null, ex.Message);
            }
        }

        /// <summary>
        /// Allows admin users to log into their dashboard
        /// </summary>
        /// <param name="signInAdminDTO"></param>
        /// <returns></returns>
        [HttpPost("signin")]
        public async Task<IActionResult> Login([FromBody] SignInAdminDTO signInAdminDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var modelErrors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage).ToList();
                    return ApiBad(modelErrors, modelErrors[0]);
                }

                var (success, signin) = await _adminService.SignInAdmin(signInAdminDTO);

                if (!success) return ApiBad(null, signin.Message);

                var admin = _mapper.Map<ReadAdminDTO>(signin.Data);

                var (isSuccess, adminresponse) = await _adminService.GetAdminManagement(admin.Email);

                if (!isSuccess) return ApiBad(null, adminresponse?.Message + " in backoffice");

                admin.Fullname = adminresponse.Data.EmployeeName;
                admin.Role = adminresponse.Data.AdminRole;
                admin.Avatar = adminresponse.Data?.Avatar;
                admin.Position = adminresponse.Data.Position;

                var (refreshTokenRequest, token) = JWT.GenerateJwtToken(admin.PhoneNumber, admin, Usertype.UserType.ADMIN.ToString());

                var refreshToken = Hashing.Base64StringEncode($"{refreshTokenRequest.RefreshToken}phone{admin.PhoneNumber}");

                _tokenService.SetUserTokens(admin.PhoneNumber, refreshTokenRequest.Token, refreshTokenRequest.RefreshToken);

                var response = new
                {
                    refreshTokenRequest.Token,
                    refreshTokenRequest.ExpiresIn,
                    tokenType = "Bearer",
                    refreshToken,
                    admin
                };

                return ApiOk(response, signin.Message);
            }
            catch (CustomException ex)
            {
                Log.Error(ex, ex.Message);
                return ApiBad(null, ex.Message);
            }
        }

        /// <summary>
        /// Allows an admin to change their password
        /// </summary>
        [HttpPost("changepassword")]
        public async Task<IActionResult> ChangePassword(ChangeAdminPassword request)
        {
            if (!ModelState.IsValid)
            {
                var modelErrors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage).ToList();
                return ApiBad(modelErrors, modelErrors[0]);
            }

            var (success, response) = await _adminService.ChangePassword(request);

            return success ? ApiOk(response.Data, response.Message) : ApiConflict(response?.Message);
        }

        /// <summary>
        /// Allows an admin to reset their password in case they forget the old one
        /// </summary>
        [HttpPost("forgotpassword")]
        public async Task<IActionResult> ForgotPassword(ForgotAdminPasswordDTO request)
        {
            if (!ModelState.IsValid)
            {
                var modelErrors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage).ToList();

                return ApiBad(modelErrors, modelErrors[0]);
            }
            var (success, response) = await _adminService.ForgotPassword(request);

            return success ? ApiOk(response.Data, response.Message) : ApiConflict(response?.Message);
        }

        /// <summary>
        /// Allows an authorized admin to deactivate or reactivate an admin user
        /// </summary>
        [HttpPatch("UpdateStatus")]
        public async Task<IActionResult> Update(DeactivateAdminDTO request)
        {
            if (!ModelState.IsValid)
            {
                var modelErrors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage).ToList();

                return ApiBad(modelErrors, modelErrors[0]);
            }
            var requiredHeader = _httpContextAccessor.HttpContext.Request.Headers["X-ADMINEMAIL"];
            if (requiredHeader.Count == 0) return ApiUnauthorized("Missing required header. Unauthorized to change admin status");
            var (success, response) = await _adminService.UpdateStatus(request);

            return success ? ApiOk(response, response.Message) : ApiConflict(response?.Message);
        }

        /// <summary>
        /// Updating admin profile
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>

        [HttpPut("UpdateAdmin")]
        public async Task<IActionResult> UpdateAdmin(UpdateAdminDto request)
        {
            if (!ModelState.IsValid)
            {
                var modelErrors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage).ToList();

                return ApiBad(modelErrors, modelErrors[0]);
            }
            var requiredHeader = _httpContextAccessor.HttpContext.Request.Headers["X-ADMINEMAIL"];
            if (requiredHeader.Count == 0) return ApiUnauthorized("Missing required header. Unauthorized to change admin status");
            var (success, response) = await _adminService.UpdateAdmin(request);

            return success ? ApiOk(request, response.Message) : ApiConflict(response?.Message);
        }

        /// <summary>
        /// Allows an authorized admin to delete an admin user
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpDelete]
        public async Task<IActionResult> DeleteUser(DeleteAdminDTO request)
        {
            if (!ModelState.IsValid)
            {
                var modelErrors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage).ToList();

                return ApiBad(modelErrors, modelErrors[0]);
            }

            var (success, response) = await _adminService.DeleteUser(request);

            return success ? ApiOk(response, response.Message) : ApiConflict(response?.Message);
        }
    }
}