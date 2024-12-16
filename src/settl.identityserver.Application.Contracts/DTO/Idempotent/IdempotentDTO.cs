using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace settl.identityserver.Application.Contracts.DTO.Idempotent
{
    public class IdempotentDTO
    {
        public string RequestID { get; set; }

        [StringLength(10)]
        public string Code { get; set; }

        [StringLength(100)]
        public string Description { get; set; }

        public string Result { get; set; }

        [Column(TypeName = "date")]
        public DateTime DateStamp { get; set; }

        [Column("TimeStamp", TypeName = "time")]
        public TimeSpan TimeStamp { get; set; }
    }
}
