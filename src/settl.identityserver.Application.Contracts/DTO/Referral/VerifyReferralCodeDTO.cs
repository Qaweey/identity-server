using System.ComponentModel.DataAnnotations;

namespace settl.identityserver.Application.Contracts.DTO.Referral
{
    public class VerifyReferralCodeDTO
    {
        [Required]
        public string Code { get; set; }
    }
}
