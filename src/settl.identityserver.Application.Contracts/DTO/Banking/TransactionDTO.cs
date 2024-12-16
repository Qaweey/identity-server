using System;
using System.Collections.Generic;

namespace settl.identityserver.Application.Contracts.DTO.Banking
{
    public class TransactionResponseDTO : SettlBaseApiDTO
    {
        public TransRoot Data { get; set; }
    }

    public class TransRoot
    {
        public decimal TotalTransValue { get; set; }
        public int TotalTransVolume { get; set; }
        public List<TransactionDTO> Transactions { get; set; }
    }

    public class Time
    {
        public int Hours { get; set; }
        public int Minutes { get; set; }
        public int Seconds { get; set; }
    }

    public class TransactionDTO
    {
        public TransactionDTO()
        {
            FromCustomerId ??= "";
            FromCustomerName ??= "";
            FromCustomerAcctNo ??= "";
            Narration ??= "";
        }

        public string TransId { get; set; }

        public DateTime TransDate { get; set; }

        public Time TransTime { get; set; }

        public string CustomerId { get; set; }
        public string FromCustomerId { get; set; } = "";

        public string FromCustomerName { get; set; } = "";

        public string FromCustomerAcctNo { get; set; } = "";

        public string ToCustomerId { get; set; } = "";
        public string ToCustomerName { get; set; } = "";
        public string ToCustomerAcctNo { get; set; } = "";

        public string TransDescription { get; set; }

        public string Narration { get; set; } = "";

        public string PayMethod { get; set; }

        public decimal TransAmount { get; set; }

        public string TransCurrency { get; set; }

        public string TransType { get; set; }

        public string TransEntry { get; set; }

        public string TransDomain { get; set; }

        public string FundSource { get; set; }

        public string TransStatus { get; set; }

        public string TransChannel { get; set; }
        public string WalletTypeId { get; set; }
        public string WalletId { get; set; }
    }
}