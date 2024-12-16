using System.ComponentModel.DataAnnotations;

namespace settl.identityserver.Application.Contracts.DTO.Consumer
{
    public class SignInConsumerDTO
    {
        [Required]
        [RegularExpression(@"0([7][0]|[8,9][0,1])\d{8}$", ErrorMessage = "Invalid Phone Number Format")]
        [StringLength(11, ErrorMessage = "Invalid Phone Number Length", MinimumLength = 11)]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; }

        [Required]
        public string PhoneModelNo { get; set; }

        [Required]
        public string IMEINo { get; set; }

        public bool FromUSSD { get; set; }
    }
}