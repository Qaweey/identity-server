using settl.identityserver.Application.Contracts.DTO.Banking;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace settl.identityserver.Application.Contracts.DTO.Consumer
{
    public class CreateConsumerResponseDTO
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public ConsumerWalletDTO Data { get; set; }
        public List<string> Errors { get; set; }
    }

    public class ConsumerWalletsResponseDTO
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public ConsumerWallet[] Data { get; set; }
        public List<string> Errors { get; set; }
    }

    public class ConsumerWallet
    {
        [JsonPropertyName("Key")]
        public string Key { get; set; }

        [JsonPropertyName("Name")]
        public string Name { get; set; }

        [JsonPropertyName("Balance")]
        public decimal Balance { get; set; }

        [JsonPropertyName("WalletType")]
        public string WalletType { get; set; }

        [JsonPropertyName("Frequency")]
        public decimal Frequency { get; set; }

        [JsonPropertyName("FrequencyInString")]
        public string FrequencyInString { get; set; }

        [JsonPropertyName("TargetedAmount")]
        public decimal TargetedAmount { get; set; }

        [JsonPropertyName("AmountPerSaving")]
        public decimal AmountPerSaving { get; set; }

        [JsonPropertyName("Goal")]
        public string Goal { get; set; }

        [JsonPropertyName("StartDate")]
        public DateTime StartDate { get; set; }

        [JsonPropertyName("EndDate")]
        public DateTime EndDate { get; set; }

        [JsonPropertyName("Interest")]
        public decimal Interest { get; set; }

        [JsonPropertyName("EarnInterest")]
        public bool EarnInterest { get; set; }

        [JsonPropertyName("FundingSource")]
        public string FundingSource { get; set; }

        [JsonPropertyName("PeriodInMonth")]
        public int PeriodInMonth { get; set; }

        [JsonPropertyName("IsEnabled")]
        public bool IsEnabled { get; set; }

        [JsonPropertyName("CurrentAmount")]
        public decimal CurrentAmount { get; set; }

        [JsonPropertyName("PercentageCompleted")]
        public decimal PercentageCompleted { get; set; }

        public string VirtualCardRequestStatus { get; set; } = "";

        [JsonPropertyName("Transactions")]
        public TransactionDTO[] Transactions { get; set; }
    }

    public class ConsumerWalletDTO
    {
        public ConsumerWalletDTO()
        {
            Bank ??= "";
        }

        [JsonPropertyName("FromUSSD")]
        public bool FromUSSD { get; set; }

        [JsonPropertyName("PhoneNo")]
        public string PhoneNo { get; set; }

        [JsonPropertyName("ActiveAcctNumber")]
        public string ActiveAcctNumber { get; set; }

        [JsonPropertyName("FirstChoiceAcctNumber")]
        public string FirstChoiceAcctNumber { get; set; }

        [JsonPropertyName("SecondChoiceAcctNumber")]
        public string SecondChoiceAcctNumber { get; set; }

        [JsonPropertyName("Nin")]
        public string Nin { get; set; }

        [JsonPropertyName("Bvn")]
        public string Bvn { get; set; }

        [JsonPropertyName("FirstName")]
        public string FirstName { get; set; }

        [JsonPropertyName("LastName")]
        public string LastName { get; set; }

        //[JsonIgnore]
        [JsonPropertyName("UserName")]
        public string UserName { get; set; }

        [JsonPropertyName("Dob")]
        public DateTime Dob { get; set; }

        [JsonPropertyName("Gender")]
        public string Gender { get; set; } = "";

        [JsonPropertyName("Email")]
        public string Email { get; set; }

        [JsonPropertyName("TransactionPin")]
        public string TransactionPin { get; set; }

        [JsonPropertyName("HasTransactionPin")]
        public bool HasTransactionPin { get; set; }

        [JsonPropertyName("BankCode")]
        public string BankCode { get; set; }

        [JsonPropertyName("Bank")]
        public string Bank { get; set; } = "";

        [JsonPropertyName("PassportAttachFile")]
        public string PassportAttachFile { get; set; }

        [JsonPropertyName("Kyclevel")]
        public int Kyclevel { get; set; }

        [JsonPropertyName("ReferralCode")]
        public string ReferralCode { get; set; }

        [JsonPropertyName("UsedReferralCode")]
        public string UsedReferralCode { get; set; }

        [JsonPropertyName("IsReferred")]
        public bool IsReferred { get; set; }

        [JsonPropertyName("HasProfileInformation")]
        public bool HasProfileInformation { get; set; }

        [JsonPropertyName("PostalAddress")]
        public string PostalAddress { get; set; } = "";

        [JsonPropertyName("HomeAddress")]
        public string HomeAddress { get; set; } = "";

        [JsonPropertyName("NearestLandmark")]
        public string NearestLandmark { get; set; } = "";

        [JsonPropertyName("State")]
        public string State { get; set; } = "";

        [JsonPropertyName("LocalGovernment")]
        public string LocalGovernment { get; set; } = "";

        [JsonPropertyName("NoKPhoneNo")]
        public string NoKPhoneNo { get; set; } = "";

        [JsonPropertyName("NoKEmail")]
        public string NoKEmail { get; set; } = "";

        [JsonPropertyName("NoKFirstName")]
        public string NoKFirstName { get; set; } = "";

        [JsonPropertyName("NoKLastName")]
        public string NoKLastName { get; set; } = "";

        [JsonPropertyName("NoKPostalAddress")]
        public string NoKPostalAddress { get; set; } = "";

        [JsonPropertyName("NoKHomeAddress")]
        public string NoKHomeAddress { get; set; } = "";

        [JsonPropertyName("NoKState")]
        public string NoKState { get; set; } = "";

        [JsonPropertyName("NoKLocalGovernment")]
        public string NoKLocalGovernment { get; set; } = "";

        [JsonPropertyName("NoKRelationship")]
        public string NoKRelationship { get; set; } = "";

        [JsonPropertyName("TotalNotificationCount")]
        public int TotalNotificationCount { get; set; }

        [JsonPropertyName("RequestsCount")]
        public int RequestsCount { get; set; }

        [JsonPropertyName("UpdatesCount")]
        public int UpdatesCount { get; set; }

        [JsonPropertyName("Rider")]
        public bool Rider { get; set; }
    }
}