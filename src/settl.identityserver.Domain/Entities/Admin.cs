using settl.identityserver.Domain.Shared;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace settl.identityserver.Domain.Entities
{
    [Table("tbl_BackOffice_Admin")]
    public class Admin : Entity
    {
        public Admin()
        {
            StaffId = "Settl-" + Guid.NewGuid().ToString().Substring(0, 4).ToUpper();
        }

        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public new int Id { get; set; }

        [Key]
        [Required]
        [StringLength(10)]
        public string StaffId { get; set; }

        [Required]
        [StringLength(100)]
        public string Firstname { get; set; }

        [Required]
        [StringLength(100)]
        public string Lastname { get; set; }

        [StringLength(50)]
        public string Middlename { get; set; }

        [Required]
        [StringLength(50)]
        public string Role { get; set; }

        [StringLength(50)]
        public string Password { get; set; }

        [StringLength(20)]
        public string Gender { get; set; }

        [StringLength(20)]
        public string MaritalStatus { get; set; }

        [DataType(DataType.Date)]
        public DateTime Dob { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }

        [Required]
        [StringLength(11)]
        public string PhoneNumber { get; set; }

        [StringLength(500)]
        public string ContactAddress { get; set; }

        [StringLength(5)]
        public string Title { get; set; }

        public int StateId { get; set; }

        [StringLength(100)]
        public string City { get; set; }

        [StringLength(100)]
        public string Nationality { get; set; }

        [StringLength(50)]
        public string NIN { get; set; }

        [StringLength(50)]
        public string BVN { get; set; }

        public int StateOfOrigin { get; set; }

        public int LgaOfOrigin { get; set; }

        [StringLength(100)]
        public string PlaceOfBirth { get; set; }

        [StringLength(10)]
        public string AccountNumber { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DateOfEmployment { get; set; }

        [DataType(DataType.Date)]
        public DateTime? ResumptionDate { get; set; }

        [StringLength(200)]
        public string Branch { get; set; }

        [StringLength(100)]
        public string Department { get; set; }

        [StringLength(100)]
        public string Unit { get; set; }

        [StringLength(100)]
        public string Position { get; set; }

        [StringLength(250)]
        public string SpouseName { get; set; }

        [StringLength(500)]
        public string SpouseAddress { get; set; }

        [StringLength(11)]
        public string SpousePhoneNumber { get; set; }

        [Required]
        public bool IsConfirmed { get; set; }

        [StringLength(5)]
        public string NokTitle { get; set; }

        [StringLength(100)]
        public string NokLastname { get; set; }

        [StringLength(500)]
        public string NokContactAddress { get; set; }

        public int NokState { get; set; }

        [StringLength(100)]
        public string NokCity { get; set; }

        public int NokLga { get; set; }

        [EmailAddress]
        [StringLength(100)]
        public string NokEmail { get; set; }

        [StringLength(11)]
        public string NokPhone { get; set; }

        [StringLength(50)]
        public string NokRelationship { get; set; }

        [DataType(DataType.Date)]
        public DateTime? ConfirmationDate { get; set; }

        [StringLength(100)]
        public string PassportPhoto { get; set; }

        [StringLength(5)]
        public string OfficeExtension { get; set; }
    }
}