using settl.identityserver.Domain.Shared;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace settl.identityserver.Domain.Entities
{
    [Table("tbl_IdentityServer_TempUsers")]
    public class TblTempUser : Entity
    {
        public TblTempUser()
        {
            Email = Phone + "@settl.me";
        }

        public int UserTypeId { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        [StringLength(11, ErrorMessage = "Invalid Phone Number", MinimumLength = 11)]
        [RegularExpression("^[0]\\d{10}$", ErrorMessage = "Invalid Phone Number Format")]
        public string Phone { get; set; }

        public string UserName { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "Invalid password length", MinimumLength = 8)]
        public string Password { get; set; }

        [Required]
        public string Gender { get; set; }

        [Required]
        public string PhoneName { get; set; }

        [Required]
        public string PhoneModelNo { get; set; }

        [Required]
        public string IMEINo { get; set; }

        public string ReferralCode { get; set; }

        [ForeignKey("UserTypeId")]
        public UserTypes UserType { get; set; }
    }
}