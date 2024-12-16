using System.ComponentModel.DataAnnotations;

namespace settl.identityserver.Application.Contracts.DTO.Admin
{
    public class SignInAdminDTO
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }

    public class UpdateAdminDto
    {
        public string Email { get; set; }
        public string FullName { get; set; }

        public string PhoneNumber { get; set; }
        public string Role { get; set; }
        public string Department { get; set; }
    }

    public class ChangeAdminPassword : SignInAdminDTO
    {
        [Required]
        public string OldPassword { get; set; }

        [Required]
        [Compare("Password", ErrorMessage = "Passwords do no match")]
        public string ConfirmPassword { get; set; }
    }

    public class DeactivateAdminDTO
    {
        [EmailAddress]
        [Required]
        public string SuperAdminEmail { get; set; }

        [EmailAddress]
        [Required]
        public string Email { get; set; }

        [Required]
        public bool Status { get; set; }
    }

    public class DeleteAdminDTO
    {
        [Required]
        [EmailAddress]
        public string SuperAdminEmail { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }

    public class ForgotAdminPasswordDTO
    {
        [EmailAddress]
        [Required]
        public string Email { get; set; }

        [Required]
        [StringLength(6)]
        public string Otp { get; set; }

        [Required]
        [MinLength(8)]
        public string Password { get; set; }

        [Required]
        [Compare("Password", ErrorMessage = "Passwords do no match")]
        public string ConfirmPassword { get; set; }
    }
}