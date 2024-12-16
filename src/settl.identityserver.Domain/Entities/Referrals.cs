using settl.identityserver.Domain.Shared;
using System;

namespace settl.identityserver.Domain.Entities
{
    public class Referrals : Entity
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNo { get; set; }
        public string RefCode { get; set; }
        public string UFirstName { get; set; }
        public string ULastName { get; set; }
        public string UPhoneNo { get; set; }
        public DateTime UsedDate { get; set; }
        public DateTime GeneratedDate { get; set; }
        public bool Status { get; set; }
    }
}
