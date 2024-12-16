using settl.identityserver.Domain.Shared.Helpers;
using System;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace settl.identityserver.Domain.Entities
{
    [Table("tbl_admin")]
    public partial class tbl_admin
    {
        public tbl_admin()
        {
            created_on = DateHelper.GetCurrentLocalTime();
        }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }

        [Column("auth_id")]
        public int auth_id { get; set; }
        public int admin_roles_id { get; set; }
        public DateTime created_on { get; set; }
        public DateTime? updated_on { get; set; }

        [ForeignKey("auth_id")]
        public virtual Auth auth { get; set; }
    }
}