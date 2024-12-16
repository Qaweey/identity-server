using settl.identityserver.Domain.Shared;
using System.ComponentModel.DataAnnotations.Schema;

namespace settl.identityserver.Domain.Entities
{
    [Table("tbl_security_answers")]
    public class SecurityAnswer : Entity
    {
        [Column("auth_id")]
        public int AuthId { get; set; }

        [Column("first_question")]
        public string First_question { get; set; }

        [Column("first_answer")]
        public string First_answer { get; set; }

        [Column("second_question")]
        public string Second_question { get; set; }

        [Column("second_answer")]
        public string Second_answer { get; set; }

        [Column("third_question")]
        public string Third_question { get; set; }

        [Column("third_answer")]
        public string Third_answer { get; set; }
    }
}