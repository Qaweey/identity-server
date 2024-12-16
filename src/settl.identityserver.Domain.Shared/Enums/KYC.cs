namespace settl.identityserver.Domain.Shared.Enums
{
    public class KYC
    {
        public enum IdType
        {
            NIN = 1,
            NIN_SLIP,
            DRIVERS_LICENSE,
            VOTER_ID,
            CAC,
            TIN,
            BVN,
            BANK_ACCOUNT,
            PHONE_NUMBER
        }
    }
}