using Microsoft.AspNetCore.Mvc.Filters;
using settl.identityserver.API.Controllers;
using settl.identityserver.Application.Contracts.IServices;
using settl.identityserver.Domain.Shared.Helpers;
using System.Threading.Tasks;

namespace settl.identityserver.API
{
    /// <summary>
    /// Filter to validate Bearer Token Authorization for requests
    /// </summary>
    public class TokenValidationFilterAttribute : ActionFilterAttribute
    {
        private readonly ITokenService tokenService;
        private readonly IUserService userService;
        private readonly IAdminService adminService;

        public TokenValidationFilterAttribute(ITokenService tokenService, IUserService userService, IAdminService adminService)
        {
            this.userService = userService;
            this.tokenService = tokenService;
            this.adminService = adminService;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var controller = context.Controller as BaseApiController;
            var (message, err) = StringUtility.HasJWTToken(context.HttpContext.Request);
            if (err)
            {
                context.Result = controller.ApiUnauthorized(message);
            }
            else
            {
                var (hasError, errorMessage, validatedToken) = tokenService.ProcessTokenValidation(message, false, true);

                if (hasError)
                {
                    context.Result = controller.ApiUnauthorized(errorMessage);
                }

                var phone = tokenService.GetTokenClaims(validatedToken);

                dynamic admin = null;
                var user = await userService.Get(phone);

                if (user is null) admin = await adminService.GetAdminUser(0, phone: phone);

                if (admin is null) context.Result = controller.ApiUnauthorized("Caller is not authorized to make use of the API service.");

                if (user is not null && !user.IsActive) context.Result = controller.ApiUnauthorized("User has been frozen");

                controller.SettlUser = user;
                await next();
            }
        }
    }
}