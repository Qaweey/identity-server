using System.ComponentModel.DataAnnotations;

namespace settl.identityserver.Application.Contracts.DTO.Consumer
{
    public class VerifyConsumerPasswordDTO
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
