using System.ComponentModel.DataAnnotations;

namespace settl.identityserver.Application.Contracts.DTO.Agency
{
    public class SignInAgencyDTO
    {
        [Required]
        [RegularExpression(@"0([7][0]|[8,9][0,1])\d{8}$", ErrorMessage = "Invalid Phone Number Format")]
        public string Phone { get; set; }

        [Required]
        public string Password { get; set; }
    }
}