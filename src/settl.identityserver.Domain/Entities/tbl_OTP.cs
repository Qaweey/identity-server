using settl.identityserver.Domain.Shared;
using settl.identityserver.Domain.Shared.Helpers;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace settl.identityserver.Domain.Entities
{
    [Table("tbl_OTP")]
    public partial class tbl_OTP : Entity
    {
        public tbl_OTP()
        {
            tbl_user_on_boardings = new HashSet<tbl_user_on_boarding>();
            phone = string.Empty;
            email = string.Empty;
            IsActive = true;
            CreatedOn = DateHelper.GetCurrentLocalTime();
        }

        public string phone { get; set; }
        public string code { get; set; }
        public string email { get; set; }

        public virtual ICollection<tbl_user_on_boarding> tbl_user_on_boardings { get; set; }
    }
}