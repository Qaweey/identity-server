using settl.identityserver.Domain.Shared;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace settl.identityserver.Domain.Entities
{
    [Table("tbl_IdentityServer_IdVerifications")]
    public class SmileIdVerification : Entity
    {
        public string JSONVersion { get; set; }

        [Required]
        public string SmileJobId { get; set; }

        [Required]
        public string Job_Id { get; set; }

        [Required]
        public string User_Id { get; set; }

        [Required]
        public int Job_Type { get; set; }

        [Required]
        public string ResultType { get; set; }

        [Required]
        public string ResultText { get; set; }

        [Required]
        public string ResultCode { get; set; }

        [Required]
        public string IsFinalResult { get; set; }

        [Required]
        public string Return_Personal_Info { get; set; }

        [Required]
        public string Verify_ID_Number { get; set; }

        public string ConfidenceValue { get; set; }

        public string Names { get; set; }
        public string DOB { get; set; }
        public string Gender { get; set; }
        public string Phone_Number { get; set; }
        public string ID_Verification { get; set; }

        [Required]
        public string Source { get; set; }

        [Required]
        public string SecKey { get; set; }

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

        public string Signature { get; set; }

        [Required]
        public string TimeStamp { get; set; }
    }

   
}