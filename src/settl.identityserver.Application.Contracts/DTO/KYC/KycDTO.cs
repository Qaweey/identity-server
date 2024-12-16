using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using static settl.identityserver.Domain.Shared.Enums.KYC;

namespace settl.identityserver.Application.Contracts.DTO.KYC
{
    public class KycDTO
    {
        [Required]
        public IdType Id_type { get; set; }

        [Required]
        public string Id_number { get; set; }

        [Required]
        public string First_name { get; set; }

        [Required]
        public string Last_name { get; set; }

        [Required]
        [StringLength(11, ErrorMessage = "Invalid Phone Number Length", MinimumLength = 11)]
        [RegularExpression(@"0([7][0]|[8,9][0,1])\d{8}$", ErrorMessage = "Invalid Phone Number Format")]
        public string Phone_number { get; set; }

        // [Required]
        public string Dob { get; set; }

        [StringLength(6, ErrorMessage = "Pass male or female", MinimumLength = 4)]
        public string Gender { get; set; }

        public string Company { get; set; }
    }

    public class SelfieDTO
    {
        [Required]
        public string file_name { get; set; }

        [Required]
        public string timestamp { get; set; }

        [Required]
        public string smile_client_id { get; set; }

        public string sec_key { get; set; }

        [Required]
        [JsonPropertyName("partner_params")]
        public PartnerSelfieParams partner_params { get; set; }

        public string model_parameters { get; set; }
        public string callback_url { get; set; }
    }

    public class SelfieKycUploadResponseDTO
    {
        public string upload_url { get; set; }
        public string ref_id { get; set; }
        public string smile_job_id { get; set; }
        public string camera_config { get; set; }
        public string code { get; set; }

        public DateTime CreatedOn { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public DateTime? DeletedOn { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class SmileRequestDTO
    {
        public SmileRequestDTO(string secKey, string timeStamp, string partnerId)
        {
            Sec_key = secKey;
            Timestamp = timeStamp;
            Partner_id = partnerId;
            Partner_Params = new PartnerParams();
        }

        [JsonPropertyName("sec_key")]
        public string Sec_key { get; set; }

        [JsonPropertyName("timestamp")]
        public string Timestamp { get; set; }

        [JsonPropertyName("partner_params")]
        public PartnerParams Partner_Params { get; set; }

        [JsonPropertyName("country")]
        public string Country { get; set; } = "NG";

        [JsonPropertyName("id_type")]
        public string Id_Type { get; set; }

        [JsonPropertyName("id_number")]
        public string Id_Number { get; set; }

        [JsonPropertyName("company")]
        public string Company { get; set; }

        [JsonPropertyName("first_name")]
        public string First_name { get; set; }

        [JsonPropertyName("middle_name")]
        public string Middle_name { get; set; }

        [JsonPropertyName("last_name")]
        public string Last_name { get; set; }

        [JsonPropertyName("phone_number")]
        public string Phone_number { get; set; }

        [JsonPropertyName("dob")]
        public string Dob { get; set; }

        [JsonPropertyName("gender")]
        public string Gender { get; set; }

        [JsonPropertyName("partner_id")]
        public string Partner_id { get; set; }
    }

    public class PartnerSelfieParams
    {
        [JsonPropertyName("job_id")]
        public string job_id { get; set; } = Guid.NewGuid().ToString();

        [JsonPropertyName("user_id")]
        public string user_id { get; set; } = Guid.NewGuid().ToString();

        [JsonPropertyName("job_type")]
        public int job_type { get; set; } = 1;
    }

    public class PartnerParams
    {
        [JsonPropertyName("job_id")]
        public string JobId { get; set; } = Guid.NewGuid().ToString();

        [JsonPropertyName("user_id")]
        public string UserId { get; set; } = Guid.NewGuid().ToString();

        [JsonPropertyName("job_type")]
        public int JobType { get; set; } = 5;
    }

    public class Actions
    {
        [JsonPropertyName("Return_Personal_Info")]
        public string Return_Personal_Info { get; set; }

        [JsonPropertyName("Verify_ID_Number")]
        public string Verify_ID_Number { get; set; }

        [JsonPropertyName("Names")]
        public string Names { get; set; }

        [JsonPropertyName("DOB")]
        public string DOB { get; set; }

        [JsonPropertyName("Gender")]
        public string Gender { get; set; }

        [JsonPropertyName("Phone_Number")]
        public string Phone_Number { get; set; }

        [JsonPropertyName("Human_Review_Compare")]
        public string HumanReviewCompare { get; set; }

        [JsonPropertyName("Human_Review_Liveness_Check")]
        public string HumanReviewLivenessCheck { get; set; }

        [JsonPropertyName("Human_Review_Update_Selfie")]
        public string HumanReviewUpdateSelfie { get; set; }

        [JsonPropertyName("Liveness_Check")]
        public string LivenessCheck { get; set; }

        [JsonPropertyName("Register_Selfie")]
        public string RegisterSelfie { get; set; }

        [JsonPropertyName("Selfie_Provided")]
        public string SelfieProvided { get; set; }

        [JsonPropertyName("Selfie_To_ID_Authority_Compare")]
        public string SelfieToIDAuthorityCompare { get; set; }

        [JsonPropertyName("Selfie_To_ID_Card_Compare")]
        public string SelfieToIDCardCompare { get; set; }

        [JsonPropertyName("Selfie_To_Registered_Selfie_Compare")]
        public string SelfieToRegisteredSelfieCompare { get; set; }

        [JsonPropertyName("Update_Registered_Selfie_On_File")]
        public string UpdateRegisteredSelfieOnFile { get; set; }

        [JsonPropertyName("ID_Verification")]
        public string ID_Verification { get; set; }
    }

    public class ErrorResponse
    {
        public string Error { get; set; }
        public string Code { get; set; }
    }

    public class VerificationResponseDTO
    {
        public string Error { get; set; }

        [JsonPropertyName("JSONVersion")]
        public string JSONVersion { get; set; }

        [JsonPropertyName("smileJobId")]
        public string SmileJobID { get; set; }

        [JsonPropertyName("partnerParams")]
        public PartnerParams PartnerParams { get; set; }

        [JsonPropertyName("resultType")]
        public string ResultType { get; set; }

        [JsonPropertyName("resultText")]
        public string ResultText { get; set; }

        [JsonPropertyName("resultCode")]
        public string ResultCode { get; set; }

        [JsonPropertyName("isFinalResult")]
        public string IsFinalResult { get; set; }

        [JsonPropertyName("actions")]
        public Actions Actions { get; set; }

        [JsonPropertyName("ConfidenceValue")]
        public string ConfidenceValue { get; set; }

        [JsonPropertyName("source")]
        public string Source { get; set; }

        [JsonPropertyName("sec_key")]
        public string Sec_Key { get; set; }

        [JsonPropertyName("signature")]
        public string Signature { get; set; }

        [JsonPropertyName("timestamp")]
        public string Timestamp { get; set; }
    }

    public partial class Welcome2
    {
        [JsonProperty("package_information")]
        public PackageInformation package_information { get; set; }

        [JsonProperty("id_info")]
        public IdInfo id_info { get; set; }

        [JsonProperty("images")]
        public Image[] images { get; set; }
    }

    public partial class IdInfo
    {
        [JsonProperty("dob")]
        public string dob { get; set; }

        [JsonProperty("country")]
        public string country { get; set; }

        [JsonProperty("entered")]
        public bool entered { get; set; }

        [JsonProperty("id_type")]
        public string id_type { get; set; }

        [JsonProperty("id_number")]
        public string id_number { get; set; }

        [JsonProperty("last_name")]
        public string last_name { get; set; }

        [JsonProperty("first_name")]
        public string first_name { get; set; }

        [JsonProperty("middle_name")]
        public string middle_name { get; set; }
    }

    public partial class Image
    {
        [JsonProperty("image_type_id")]
        public long image_type_id { get; set; }

        [JsonProperty("image")]
        public string image { get; set; }

        [JsonProperty("file_name")]
        public string file_name { get; set; }
    }

    public partial class PackageInformation
    {
        [JsonProperty("apiVersion")]
        public ApiVersion apiVersion { get; set; }
    }

    public partial class ApiVersion
    {
        [JsonProperty("buildNumber")]
        public long buildNumber { get; set; } = 0;

        [JsonProperty("majorVersion")]
        public long majorVersion { get; set; } = 2;

        [JsonProperty("minorVersion")]
        public long minorVersion { get; set; } = 0;
    }
}