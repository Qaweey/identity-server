using System.ComponentModel.DataAnnotations;

namespace settl.identityserver.Application.Contracts.DTO.Admin
{
    public class FreezeWalletDTO
    {
        [Required]
        [EmailAddress]
        public string AdminEmail { get; set; }

        [Required]
        [StringLength(11, ErrorMessage = "Invalid Phone Number", MinimumLength = 11)]
        [RegularExpression(@"0([7][0]|[8,9][0,1])\d{8}$", ErrorMessage = "Invalid Phone Number Format")]
        public string CustomerPhone { get; set; }

        [Required]
        public bool Freeze { get; set; }
    }
}