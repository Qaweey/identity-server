using settl.identityserver.Domain.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace settl.identityserver.Application.Contracts.DTO.OTP
{
    public class SendSMSDTO
    {
        [StringLength(11, ErrorMessage = "Invalid Phone Number", MinimumLength = 11)]
        [RegularExpression(@"0([7][0]|[8,9][0,1])\d{8}$", ErrorMessage = "Invalid Phone Number Format")]
        public string Phone { get; set; }

        [Required]
        public OtpType OtpType { get; set; }

        [EmailAddress]
        public string Email { get; set; }
    }

    public class SMSRequest
    {
        [RegularExpression(@"0([7][0]|[8,9][0,1])\d{8}$", ErrorMessage = "Invalid Phone Number Format")]
        public string Phone { get; set; }

        public string Body { get; set; }

        public string ReceiverName { get; set; }

        public string MicroserviceName { get; set; } = Constants.IDENTITYSERVER_URL.ToUpper();
    }
}