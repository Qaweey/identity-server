using System;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace settl.identityserver.Domain.Entities
{
    [Table("tbl_admin_roles")]
    public partial class tbl_admin_role
    {
        public int id { get; set; }
        public string role { get; set; }
        public DateTime created_on { get; set; }
        public DateTime updated_on { get; set; }
    }
}