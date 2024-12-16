using System.ComponentModel.DataAnnotations;

namespace settl.identityserver.Application.Contracts.DTO.Agency
{
    public class UploadSelfieDTO
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public string ReferenceCode { get; set; }

        [Required]
        public string Phone { get; set; }

        [Required]
        public string Selfie { get; set; }
    }
}
