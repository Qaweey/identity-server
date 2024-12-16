using settl.identityserver.Domain.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace settl.identityserver.Domain.Entities
{
      [Table("tbl_IdentityServer_SelfieVerifications")]
        public class SelfieVerification :Entity
        {
           
            public string upload_url { get; set; }
            public string ref_id { get; set; }
            public string smile_job_id { get; set; }
            public string camera_config { get; set; }
            public string code { get; set; }
        }
    
}
