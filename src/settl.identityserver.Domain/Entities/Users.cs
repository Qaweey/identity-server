using settl.identityserver.Domain.Shared;
using settl.identityserver.Domain.Shared.Helpers;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace settl.identityserver.Domain.Entities
{
    public class Users : Entity
    {
        public Users()
        {
            Username = "SETTL-" + Guid.NewGuid().ToString()[..4];
            Email = Phone + "@settl.me";
            LastLoginDate = DateHelper.GetCurrentLocalTime();
        }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public new int Id { get; set; }

        public string Email { get; set; }

        [Key]
        public string Phone { get; set; }

        public string Password { get; set; }
        public bool IsApproved { get; set; }
        public DateTime LastLoginDate { get; set; }
        public DateTime? LastLogOutDate { get; set; }
        public string LoggedOnStatus { get; set; }
        public bool PhoneNoEnabled { get; set; }
        public string LockOutStatus { get; set; }
        public DateTime? LockOutEndDate { get; set; }
        public int AccessFailedCount { get; set; }
        public string PhoneName { get; set; }
        public string PhoneModelNo { get; set; }
        public string PhoneSerialNo { get; set; }
        public string IMEINo { get; set; }

        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Gender { get; set; }
        public string Bvn { get; set; }
        public string State { get; set; }
        public string Lga { get; set; }
        public string TransactionPin { get; set; }
        public string Dob { get; set; }
        public string Username { get; set; }
        public string BusinessName { get; set; }
        public string BusinessAddress { get; set; }
        public int NoSubAgt { get; set; }
        public string CACDocument { get; set; }
        public string UtilityBill { get; set; }
        public int UserTypeId { get; set; }
        public string Selfie { get; set; }
        public string ResAddress { get; set; }
        public bool IsFirstTimeLogin { get; set; }

        [ForeignKey("UserTypeId")]
        public UserTypes UserType { get; set; }

        public string JwtToken { get; set; }

        public string RefreshToken { get; set; }
    }
}