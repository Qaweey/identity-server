using System.ComponentModel.DataAnnotations;

namespace settl.identityserver.Application.Contracts.DTO.TransactionPIN
{
    public class ResetTransactionPinDTO
    {
        [Required]
        public int Id { get; set; }
    }
}
