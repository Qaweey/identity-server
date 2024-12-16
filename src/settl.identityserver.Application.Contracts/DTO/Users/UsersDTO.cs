using System;
using System.Text.Json.Serialization;

namespace settl.identityserver.Application.Contracts.DTO.Users
{
    [Serializable]
    public class UserTokenInfoDto
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
    }

    public class UsersDTO
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public bool IsActive { get; set; }
    }

    public class UserDTO
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public DateTime? date_of_birth { get; set; }
        public bool? Consumer { get; set; }
        public bool Admin { get; set; }
        public bool Merchant { get; set; }

        public string Gender { get; set; }

        public string residential_address { get; set; }
        public string selfie { get; set; }
        public string referral_code { get; set; }
        public string state { get; set; }
        public string address { get; set; }
        public string lg { get; set; }
        public string nearest_landmark { get; set; }

        [JsonPropertyName("imeiNo")]
        public string Imei_no { get; set; }

        [JsonPropertyName("phoneModelNo")]
        public string phone_model_no { get; set; }

        [JsonPropertyName("phoneName")]
        public string phone_name { get; set; }

        public bool Enabled { get; set; }
        public string UserName { get; set; }
        public int UserTypeId { get; set; } = 4;

        public DateTime CreatedOn { get; set; }

        public bool IsActive { get; set; }
    }
}