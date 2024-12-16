using AutoMapper.Configuration.Annotations;
using System.ComponentModel.DataAnnotations;

namespace settl.identityserver.Application.Contracts.DTO.TransactionPIN
{
    public class ChangeTransactionPinDTO
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public string OldTransactionPin { get; set; }

        [Required]
        public string TransactionPin { get; set; }

        [Required]
        [Compare("TransactionPIN")]
        [Ignore]
        public string ConfirmTransactionPin { get; set; }
    }
}
