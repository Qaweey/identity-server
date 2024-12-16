using AutoMapper.Configuration.Annotations;
using System.ComponentModel.DataAnnotations;

namespace settl.identityserver.Application.Contracts.DTO.Consumer
{
    public class CreateConsumerDTO
    {
        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        [StringLength(11, ErrorMessage = "Invalid Phone Number", MinimumLength = 11)]
        [RegularExpression(@"0([7][0]|[8,9][0,1])\d{8}$", ErrorMessage = "Invalid Phone Number Format")]
        public string Phone { get; set; }

        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(200, ErrorMessage = "Password should be at least 8 characters", MinimumLength = 8)]
        public string Password { get; set; }

        [Required]
        [Compare("Password")]
        [Ignore]
        public string ConfirmPassword { get; set; }

        [StringLength(10)]
        [Required]
        public string Gender { get; set; }

        [Required]
        public string PhoneName { get; set; }

        [Required]
        public string PhoneModelNo { get; set; }

        [Required]
        public string IMEINo { get; set; }

        public string ReferralCode { get; set; } = "";
    }
}