using System.ComponentModel.DataAnnotations;

namespace settl.identityserver.Application.Contracts.DTO.Users
{
    public class RefreshTokenDTO
    {
        [Required]
        public string RefreshToken { get; set; }
    }

    public class AccessTokenDTO
    {
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
    }

    public class TokenResponseDTO
    {
        public string AccessToken { get; set; }
        public string TokenType { get; set; }
        public int ExpriresIn { get; set; }
        public string RefreshToken { get; set; }
    }
}