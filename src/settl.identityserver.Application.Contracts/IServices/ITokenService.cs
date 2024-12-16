using IdentityModel.Client;
using System.Security.Claims;
using System.Threading.Tasks;

namespace settl.identityserver.Application.Contracts.IServices
{
    public interface ITokenService
    {
        Task<(string, int)> GenerateJWT(string[] scopes, string email, string phone, string firstName, string lastName);

        Task<TokenResponse> UseRefreshToken(string refreshToken);

        Task<TokenResponse> GetClientToken();

        Task<TokenResponse> GetClientToken(string username, string password);

        (bool, string, ClaimsPrincipal) ProcessTokenValidation(string token, bool checkTokenNotExpired = false, bool checkTokenHasExpired = false);

        string GetTokenClaims(ClaimsPrincipal validatedToken);

        string GetTokenUserTypeClaims(ClaimsPrincipal validatedToken);

        void SetUserTokens(string phoneNumber, string accessToken, string refreshToken);

        void RemoveUserTokens(string phoneNumber);
    }
}