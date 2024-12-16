using AutoMapper.Configuration.Annotations;
using System.ComponentModel.DataAnnotations;

namespace settl.identityserver.Application.Contracts.DTO.Agency
{
    public class UpdateAgencyPasswordDTO
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [MinLength(8)]
        public string Password { get; set; }

        [Required]
        [Compare("Password")]
        [MinLength(8)]
        [Ignore]
        public string ConfirmPassword { get; set; }
    }
}
