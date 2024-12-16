using AutoMapper.Configuration.Annotations;
using System.ComponentModel.DataAnnotations;

namespace settl.identityserver.Application.Contracts.DTO.Agency
{
    public class CreateAgencyDTO
    {
        [Required]
        public string FirstName { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        [RegularExpression(@"0([7][0]|[8,9][0,1])\d{8}$", ErrorMessage = "Invalid Phone Number Format")]
        public string Phone { get; set; }

        [Required]
        [MinLength(8)]
        public string Password { get; set; }

        [Required]
        [Compare("Password")]
        [MinLength(8)]
        [Ignore]
        public string ConfirmPassword { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public string Dob { get; set; }

        [Required]
        public string State { get; set; }

        [Required]
        public string Lga { get; set; }

        [Required]
        public string ResAddress { get; set; }
    }
}