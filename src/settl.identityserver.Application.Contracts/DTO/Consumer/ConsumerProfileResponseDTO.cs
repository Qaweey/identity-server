using System.Text.Json.Serialization;

namespace settl.identityserver.Application.Contracts.DTO.Consumer
{
    public class ConsumerProfileResponseDTO
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public ConsumerProfile Data { get; set; }
        public object Errors { get; set; }
    }

    public class ConsumerProfile
    {
        [JsonPropertyName("Id")]
        public string Id { get; set; }

        [JsonPropertyName("PhoneNo")]
        public string PhoneNo { get; set; }

        [JsonPropertyName("PostalAddress")]
        public string PostalAddress { get; set; } = "";

        [JsonPropertyName("HomeAddress")]
        public string HomeAddress { get; set; } = "";

        [JsonPropertyName("NearestLandmark")]
        public string NearestLandmark { get; set; } = "";

        [JsonPropertyName("State")]
        public string State { get; set; } = "";

        [JsonPropertyName("LocalGovernment")]
        public string LocalGovernment { get; set; } = "";

        [JsonPropertyName("NokPhone")]
        public string NokPhoneNo { get; set; } = "";

        [JsonPropertyName("NokEmail")]
        public string NokEmail { get; set; } = "";

        [JsonPropertyName("NokFirstName")]
        public string NokFirstName { get; set; } = "";

        [JsonPropertyName("NokLastName")]
        public string NokLastName { get; set; } = "";

        [JsonPropertyName("NokPostalAddress")]
        public string NokPostalAddress { get; set; } = "";

        [JsonPropertyName("NokHomeAddress")]
        public string NokHomeAddress { get; set; } = "";

        [JsonPropertyName("NokState")]
        public string NokState { get; set; } = "";

        [JsonPropertyName("NokLocalGovernment")]
        public string NokLocalGovernment { get; set; } = "";

        [JsonPropertyName("NokRelationship")]
        public string NokRelationship { get; set; } = "";
    }
}