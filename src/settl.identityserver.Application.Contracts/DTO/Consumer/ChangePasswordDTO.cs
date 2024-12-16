using AutoMapper.Configuration.Annotations;
using System.ComponentModel.DataAnnotations;

namespace settl.identityserver.Application.Contracts.DTO.Consumer
{
    public class ChangePasswordDTO
    {
        [Required]
        [RegularExpression(@"0([7][0]|[8,9][0,1])\d{8}$", ErrorMessage = "Invalid Phone Number Format")]
        [StringLength(11, MinimumLength = 11)]
        public string Phone { get; set; }

        [Required]
        public string OldPassword { get; set; }

        [Required]
        [MinLength(8)]
        public string Password { get; set; }

        [Required]
        [Compare("Password")]
        [MinLength(8)]
        [Ignore]
        public string ConfirmPassword { get; set; }
    }

    public class ResetConsumerPasswordDTO
    {
        [Required]
        [RegularExpression(@"0([7][0]|[8,9][0,1])\d{8}$", ErrorMessage = "Invalid Phone Number Format")]
        public string Phone { get; set; }

        [Required]
        public string TransactionPin { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }
    }
}