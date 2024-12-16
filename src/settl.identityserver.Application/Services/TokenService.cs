using IdentityModel.Client;
using IdentityServer4;
using settl.identityserver.Application.Contracts.DTO.Users;
using settl.identityserver.Application.Contracts.IServices;
using settl.identityserver.Domain.Shared.Caching;
using settl.identityserver.Domain.Shared.Helpers;
using settl.identityserver.Domain.Shared.Helpers.Authentication;
using System;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace settl.identityserver.Application.Services
{
    public class TokenService : ITokenService
    {
        private readonly IJWT jWT;
        private readonly IdentityServerTools tools;
        private readonly ICacheService _cacheService;

        public TokenService(IdentityServerTools identityserverTools, IJWT jwt, ICacheService cacheService)
        {
            tools = identityserverTools;
            jWT = jwt;
            _cacheService = cacheService;
        }

        public async Task<(string, int)> GenerateJWT(string[] scopes, string email, string phone, string firstName, string lastName)
        {
            int timeLimit = 86400;
            var token = await tools.IssueClientJwtAsync(
               clientId: Environment.GetEnvironmentVariable("ClientId"),
               lifetime: timeLimit,
               scopes: scopes,
               additionalClaims: new[] {
                    new Claim("email", email),
                    new Claim("phone", phone),
                    new Claim("firstName", firstName),
                    new Claim("lastName", lastName),
               }
            );

            return (token, timeLimit);
        }

        public async Task<TokenResponse> GetClientToken()
        {
            var username = Environment.GetEnvironmentVariable("ClientId");
            var password = Environment.GetEnvironmentVariable("ClientSecret");

            var client = new HttpClient();

            var request = new TokenRequest
            {
                Address = $"https://{Environment.GetEnvironmentVariable("IDENTITYSERVER_TOKEN_ENDPOINT")}identityserver.settl.me/connect/token",

                GrantType = "password",
                ClientId = Environment.GetEnvironmentVariable("ClientId"),
                ClientSecret = Environment.GetEnvironmentVariable("ClientSecret"),
                Parameters =
                {
                    { "username", username },
                    { "password", password },
                    { "email", "someone@email.me" }
                },

                RequestUri = new Uri("https://settl.me")
            };

            if (request.Address is null || request.ClientId is null || request.ClientSecret is null) throw new Exception("request is missing a key parameter");
            var response = await client.RequestTokenAsync(request);

            return response;
        }

        public async Task<TokenResponse> GetClientToken(string username, string password)
        {
            var client = new HttpClient();

            var request = new TokenRequest
            {
                Address = Environment.GetEnvironmentVariable("IDENTITYSERVER_TOKEN_ENDPOINT"),

                GrantType = "password",
                ClientId = Environment.GetEnvironmentVariable("ClientId"),
                ClientSecret = Environment.GetEnvironmentVariable("ClientSecret"),
                Parameters =
                {
                    { "username", username },
                    { "password", password },
                },

                RequestUri = new Uri("https://settl.me")
            };

            if (request.Address is null || request.ClientId is null || request.ClientSecret is null) throw new Exception("request is missing a key parameter");
            var response = await client.RequestTokenAsync(request);

            return response;
        }

        private ClaimsPrincipal ValidateTokenSigningCredentials(string token)
        {
            return jWT.GetPrincipalFromToken(token);
        }

        public (bool, string, ClaimsPrincipal) ProcessTokenValidation(string token, bool checkTokenNotExpired = false, bool checkTokenHasExpired = false)
        {
            try
            {
                var validatedToken = ValidateTokenSigningCredentials(token);
                if (validatedToken == null) return (true, "Invalid token", validatedToken);
                var expiryDateUtc = jWT.GetTokenExpiryTime(validatedToken);

                if (checkTokenNotExpired && expiryDateUtc > DateTime.UtcNow) return (true, "Token hasn't expired yet", validatedToken);

                if (checkTokenHasExpired && expiryDateUtc < DateTime.UtcNow) return (true, "Your session has ended. Please re-login", validatedToken);

                return (false, string.Empty, validatedToken);
            }
            catch (Exception)
            {
                return (true, "Invalid token", null);
            }
        }

        public string GetTokenClaims(ClaimsPrincipal validatedToken)
        {
            var phone = validatedToken.Claims.Single(x => x.Type == "phone").Value;

            return phone;
        }

        public async Task<TokenResponse> UseRefreshToken(string refreshToken)
        {
            var client = new HttpClient();

            var request = new IdentityModel.Client.RefreshTokenRequest
            {
                Address = Environment.GetEnvironmentVariable("IDENTITYSERVER_TOKEN_ENDPOINT"),
                ClientId = Environment.GetEnvironmentVariable("ClientId"),
                ClientSecret = Environment.GetEnvironmentVariable("ClientSecret"),
                GrantType = "refresh_token",
                RefreshToken = refreshToken
            };

            var response = await client.RequestRefreshTokenAsync(request);

            return response;
        }

        public string GetTokenUserTypeClaims(ClaimsPrincipal validatedToken)
        {
            var userType = validatedToken.Claims.Single(x => x.Type == "usertype").Value;

            return userType;
        }

        public void SetUserTokens(string phoneNumber, string accessToken, string refreshToken)
        {
            var tokenInfo = new UserTokenInfoDto
            {
                Token = accessToken,
                RefreshToken = refreshToken
            };
            var serializedTokenInfo = JsonHelper.SerializeObject(tokenInfo);
            _cacheService.Set($"{phoneNumber}-tokens", serializedTokenInfo);
        }

        public void RemoveUserTokens(string phoneNumber)
        {
            _cacheService.Remove($"{phoneNumber}-tokens");
        }
    }
}