using System.Text.Json.Serialization;

namespace settl.identityserver.Domain.Shared.Helpers.Authentication
{
    public class RefreshTokenRequest
    {
        [JsonPropertyName("token")]
        public string Token { get; set; }

        [JsonPropertyName("expiresIn")]
        public double ExpiresIn { get; set; }

        [JsonPropertyName("refreshToken")]
        public string RefreshToken { get; set; }
    }
}