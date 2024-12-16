using System.ComponentModel.DataAnnotations;

namespace settl.identityserver.Application.Contracts.DTO.Users
{
    public class LoginUsersDTO
    {
        [Required]
        public string Phone { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
