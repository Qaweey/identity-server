using System;
using System.Collections.Generic;

#nullable disable

namespace settl.identityserver.Domain.Entities
{
    public partial class tbl_log_in_activity
    {
        public int id { get; set; }
        public int auth_id { get; set; }
        public string location { get; set; }
        public DateTime created_on { get; set; }
        public DateTime? updated_on { get; set; }

        public virtual Auth auth { get; set; }
    }
}