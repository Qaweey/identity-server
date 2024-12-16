using System.ComponentModel.DataAnnotations;

namespace settl.identityserver.Application.Contracts.DTO.Admin
{
    public class AdminResponseDTO : SettlBaseApiDTO
    {
        public AdminData Data { get; set; }
    }

    public class AdminData
    {
        public string Id { get; set; }
        public string EmployeeName { get; set; }
        public string EmailAddress { get; set; }

        [RegularExpression("^[0]\\d{10}$", ErrorMessage = "Invalid Phone Number Format")]
        [StringLength(11, MinimumLength = 11)]
        public string PhoneNumber { get; set; }

        public string AdminRole { get; set; }
        public string Position { get; set; }
        public string Level { get; set; }

        public string Avatar { get; set; }

        public string Status { get; set; }
        public bool IsActive { get; set; }
    }
}