using System.Collections.Generic;

namespace settl.identityserver.Application.Contracts.DTO.Consumer
{
    public class ConsumerDTO
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string UsedReferralCode { get; set; }
    }

    public class ConsumerWalletResponseDTO
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public ConsumerWalletDTO Data { get; set; }
        public List<string> Errors { get; set; }
    }

    public class TransactionPinResponse
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public ConsumerWalletDTO Data { get; set; }
        public object Errors { get; set; }
    }
}