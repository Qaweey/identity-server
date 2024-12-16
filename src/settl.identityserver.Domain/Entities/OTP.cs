using settl.identityserver.Domain.Shared;
using System.ComponentModel.DataAnnotations;

namespace settl.identityserver.Domain.Entities
{
    public class OTP : Entity
    {
        public string Phone { get; set; } = "";

        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = "";

        public string OTPNumber { get; set; }

        //TODO: add OTP type
    }
}