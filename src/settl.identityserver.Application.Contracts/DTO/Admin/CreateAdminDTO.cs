using System.ComponentModel.DataAnnotations;

namespace settl.identityserver.Application.Contracts.DTO.Admin
{
    public class CreateAdminDTO
    {
        [Required]
        [StringLength(300)]
        public string Fullname { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }

        [Required]
        [StringLength(11, ErrorMessage = "Invalid Phone Number", MinimumLength = 11)]
        [RegularExpression(@"0([7][0]|[8,9][0,1])\d{8}$", ErrorMessage = "Invalid Phone Number Format")]
        public string PhoneNumber { get; set; }

        [Required]
        public string Role { get; set; }

        [Required]
        public string Department { get; set; }

        public string Position { get; set; } = "";
    }

    public class ReadAdminDTO
    {
        public string Id { get; set; }
        public string StaffId { get; set; }

        public string Fullname { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        [RegularExpression(@"0([7][0]|[8,9][0,1])\d{8}$", ErrorMessage = "Invalid Phone Number Format")]
        public string PhoneNumber { get; set; }

        public string Role { get; set; }
        public string Department { get; set; }

        public string Position { get; set; }
        public string Avatar { get; set; }

        public bool IsActive { get; set; }
    }
}