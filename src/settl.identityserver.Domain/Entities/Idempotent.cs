using settl.identityserver.Domain.Shared;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace settl.identityserver.Domain.Entities
{
    [Table("tblIdempotents")]
    public class Idempotent : Entity
    {
        public string RequestID { get; set; }

        [StringLength(100)]
        public string Description { get; set; }

        public string Result { get; set; }

        [Column(TypeName = "date")]
        public DateTime DateStamp { get; set; }

        [Column("TimeStamp", TypeName = "time")]
        public TimeSpan TimeStamp { get; set; }
    }
}
