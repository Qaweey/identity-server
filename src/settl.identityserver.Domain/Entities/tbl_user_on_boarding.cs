using settl.identityserver.Domain.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace settl.identityserver.Domain.Entities
{
    [Table("tbl_user_on_boarding")]
    public partial class tbl_user_on_boarding : Entity
    {
        public tbl_user_on_boarding()
        {
            tbl_auths = new HashSet<Auth>();
        }

        public string phone { get; set; }
        public string stage { get; set; }

        [Column("OTP_id")]
        public int OTP_id { get; set; }

        public string referrer { get; set; }

        [ForeignKey("OTP_id")]
        public virtual tbl_OTP OTP { get; set; }
        public virtual ICollection<Auth> tbl_auths { get; set; }
    }
}