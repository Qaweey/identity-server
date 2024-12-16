using System.ComponentModel.DataAnnotations;

namespace settl.identityserver.Application.Contracts.DTO.Onboarding
{
    public class OnboardingDTO
    {
        [Required]
        public string Phone { get; set; }

        [Required]
        public string Stage { get; set; }

        public int OTP_id { get; set; }
    }
}
