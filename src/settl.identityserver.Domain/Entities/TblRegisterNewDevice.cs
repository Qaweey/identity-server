using settl.identityserver.Domain.Shared.Helpers;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace settl.identityserver.Domain.Entities
{
    [Table("tbl_registered_new_devices")]
    public class TblRegisterNewDeviceLog
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public DateTime DateAndTimeOfLogin { get; set; }
        public string LoginCountry { get; set; }
        public string LoginCity { get; set; }

        public string PhoneNo { get; set; }
        public string Email { get; set; }
        public string IMEINO { get; set; }
        public string PhoneName { get; set; }
        public string PhoneModelNo { get; set; }
        public bool IsVerified { get; set; }
        public string IpAddress { get; set; }
        public DateTime RegistrationDate { get; set; } = DateHelper.GetCurrentLocalTime();
    }
}