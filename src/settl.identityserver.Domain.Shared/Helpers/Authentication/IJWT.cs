using System;
using System.Security.Claims;

namespace settl.identityserver.Domain.Shared.Helpers.Authentication
{
    public interface IJWT
    {
        string ConvertToken(string idToken);
        ClaimsPrincipal GetPrincipalFromToken(string token);
        DateTime GetTokenExpiryTime(ClaimsPrincipal validatedToken);
    }
}