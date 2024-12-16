using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using settl.identityserver.Domain.Shared.Enums;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace settl.identityserver.Domain.Shared.Helpers.Authentication
{
    public class JWT : IJWT
    {
        private readonly TokenValidationParameters _tokenValidationParameters = new();

        public static (RefreshTokenRequest, SecurityToken) GenerateJwtToken(string phone, object user = null, string usertype = "CONSUMER")
        {
            // authentication successful so generate jwt token
            var serializedUser = JsonHelper.SerializeObject(user);
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(Constants.JWT_SECRET);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Iss, Constants.IDENTITYSERVER_URL),
                    new Claim("phone", phone),
                    new Claim("usertype", usertype),
                    new Claim("user", serializedUser),
                    new Claim(JwtRegisteredClaimNames.Aud, "settluser")
                }),
                Expires = DateTime.UtcNow.AddSeconds(1801),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var refreshToken = StringUtility.GenerateRefreshToken();

            DateTime currentDate = DateTime.UtcNow;
            long elapsedTicks = tokenDescriptor.Expires.Value.Ticks - currentDate.Ticks;

            var elapsedSpan = new TimeSpan(elapsedTicks);
            return (new RefreshTokenRequest
            {
                Token = tokenHandler.WriteToken(token),
                RefreshToken = refreshToken,
                ExpiresIn = Math.Floor(elapsedSpan.TotalSeconds)
            }, token);
        }

        private static bool IsJwtWithValidSecurityAlgorithm(SecurityToken validatedToken)
        {
            return (validatedToken is JwtSecurityToken jwtSecurityToken) && jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase);
        }

        public ClaimsPrincipal GetPrincipalFromToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                _tokenValidationParameters.ValidateIssuer = true;
                _tokenValidationParameters.ValidIssuer = Constants.IDENTITYSERVER_URL;
                _tokenValidationParameters.ValidateIssuerSigningKey = true;
                _tokenValidationParameters.ValidAudience = "settluser";
                _tokenValidationParameters.IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Constants.JWT_SECRET));
                _tokenValidationParameters.ValidateLifetime = false;
                var principal = tokenHandler.ValidateToken(token, _tokenValidationParameters, out var validatedToken);
                if (!IsJwtWithValidSecurityAlgorithm(validatedToken)) return null;
                return principal;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return null;
            }
        }

        public DateTime GetTokenExpiryTime(ClaimsPrincipal validatedToken)
        {
            var expiryDateUnix = long.Parse(validatedToken.Claims.Single(x => x.Type == JwtRegisteredClaimNames.Exp).Value);
            return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                .AddSeconds(expiryDateUnix);
        }

        public string ConvertToken(string idToken)
        {
            var jwtHandler = new JwtSecurityTokenHandler();
            var jwtInput = idToken;
            var decodedTokenDetails = string.Empty;

            //Check if readable token (string is in a JWT format)
            var readableToken = jwtHandler.CanReadToken(jwtInput);

            if (readableToken != true)
            {
                decodedTokenDetails = "The token doesn't seem to be in a proper JWT format.";
            }
            if (readableToken == true)
            {
                var token = jwtHandler.ReadJwtToken(jwtInput);

                //Extract the payload of the JWT
                var claims = token.Claims;
                var jwtPayload = "{";
                if (claims.FirstOrDefault().Type == "actort") return claims.FirstOrDefault().Value;

                foreach (Claim c in claims)
                {
                    jwtPayload += '"' + c.Type + "\":\"" + c.Value.ToString() + "\",";
                }
                jwtPayload += "}";
                decodedTokenDetails = JToken.Parse(jwtPayload).ToString(Formatting.Indented);
            }

            return decodedTokenDetails;
        }
    }
}