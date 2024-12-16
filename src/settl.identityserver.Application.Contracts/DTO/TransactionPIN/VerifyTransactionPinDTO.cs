using System.ComponentModel.DataAnnotations;

namespace settl.identityserver.Application.Contracts.DTO.TransactionPIN
{
    public class VerifyTransactionPinDTO
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public string TransactionPin { get; set; }
    }
}
