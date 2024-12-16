using settl.identityserver.Domain.Shared;
using System.ComponentModel.DataAnnotations.Schema;

namespace settl.identityserver.Domain.Entities
{
    [Table("tbl_security_questions")]
    public class SecurityQuestion : Entity
    {
        public string Question { get; set; }
    }
}