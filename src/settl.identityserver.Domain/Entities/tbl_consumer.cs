using System;
using System.Collections.Generic;

#nullable disable

namespace settl.identityserver.Domain.Entities
{
    public partial class tbl_consumer
    {
        public int id { get; set; }
        public int auth_id { get; set; }
        public string bvn { get; set; }
        public bool? active { get; set; }
        public bool deleted { get; set; }
        public string state { get; set; }
        public string country { get; set; }
        public bool? approved { get; set; }
        public int tiers_id { get; set; }
        public DateTime created_on { get; set; }
        public DateTime? updated_on { get; set; }

        public virtual Auth auth { get; set; }
    }
}