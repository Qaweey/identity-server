using AutoMapper.Configuration.Annotations;
using System.ComponentModel.DataAnnotations;

namespace settl.identityserver.Application.Contracts.DTO.Users
{
    public class CreateUsersDTO
    {
        public CreateUsersDTO()
        {
            UserName = Email;
            Email = Phone + "@settl.me";
        }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        [StringLength(11, ErrorMessage = "Invalid Phone Number", MinimumLength = 11)]
        [RegularExpression("^[0]\\d{10}$", ErrorMessage = "Invalid Phone Number Format")]
        public string Phone { get; set; }

        public string UserName { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MinLength(8)]
        public string Password { get; set; }

        [Required]
        [Compare("Password")]
        [MinLength(8)]
        [Ignore]
        public string ConfirmPassword { get; set; }

        [Required]
        public string Gender { get; set; }

        [Required]
        public string PhoneName { get; set; }

        [Required]
        public string PhoneModelNo { get; set; }

        [Required]
        public string IMEINo { get; set; }

        public string ReferralCode { get; set; } = "";

        public bool Consumer { get; set; }
    }
}