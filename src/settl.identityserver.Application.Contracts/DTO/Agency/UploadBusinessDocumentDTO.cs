using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace settl.identityserver.Application.Contracts.DTO.Agency
{
    public class UploadBusinessDocumentDTO
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public string ReferenceCode { get; set; }

        [Required]
        public string Phone { get; set; }

        [Required]
        public string BusinessName { get; set; }

        [Required]
        public string BusinessAddress { get; set; }

        [Required]
        public IFormFile CACDocument { get; set; }

        [Required]
        public IFormFile UtilityBill { get; set; }

        [Required]
        public string NoSubAgt { get; set; }
    }
}
