using settl.identityserver.Domain.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace settl.identityserver.Domain.Entities
{
    [Table("tbl_auth")]
    public partial class Auth : Entity
    {
        public Auth()
        {
            merchant = false;
            consumer = false;
            admin = false;
            date_of_birth = null;
            country = "Nigeria";
            gender = "None";
            last_seen = DateTime.MinValue;
            tbl_admins = new HashSet<tbl_admin>();
            tbl_consumers = new HashSet<tbl_consumer>();
            tbl_log_in_activities = new HashSet<tbl_log_in_activity>();
        }

        public string email { get; set; }
        public string phone { get; set; }
        public string secret { get; set; }
        public bool approved { get; set; }
        public DateTime? last_seen { get; set; }
        public bool enabled { get; set; }
        public int user_on_boarding_id { get; set; }
        public bool online { get; set; }
        public string geolocation { get; set; }
        public string gender { get; set; }
        public string username { get; set; }
        public string residential_address { get; set; }
        public string selfie { get; set; }
        public string referral_code { get; set; }
        public bool consumer { get; set; } = false;
        public bool admin { get; set; } = false;
        public bool merchant { get; set; } = false;
        public string country { get; set; }
        public string state { get; set; }
        public string address { get; set; }
        public string lg { get; set; }
        public DateTime? date_of_birth { get; set; }
        public bool referred { get; set; }
        public string nearest_landmark { get; set; }
        public string phone_name { get; set; }
        public string phone_model_no { get; set; }
        public string imei_no { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }

        public string Department { get; set; }

        public string JwtToken { get; set; }

        public string RefreshToken { get; set; }
        public bool IsBlacklisted { get; set; }

        [Column("rider")]
        public bool Rider { get; set; }

        [Column("deliveryAgent")]
        public bool DeliveryAgent { get; set; }

        public bool IsFirstTimeLogin { get; set; }

        [ForeignKey("user_on_boarding_id")]
        public virtual tbl_user_on_boarding user_on_boarding { get; set; }

        public virtual ICollection<tbl_admin> tbl_admins { get; set; }
        public virtual ICollection<tbl_consumer> tbl_consumers { get; set; }
        public virtual ICollection<tbl_log_in_activity> tbl_log_in_activities { get; set; }
    }
}