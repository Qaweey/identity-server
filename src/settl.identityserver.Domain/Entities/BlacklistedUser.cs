using settl.identityserver.Domain.Shared;
using System.ComponentModel.DataAnnotations.Schema;

namespace settl.identityserver.Domain.Entities
{
    [Table("tbl_IdentityServer_BlacklistedUsers")]
    public class BlacklistedUser : Entity
    {
        public string Phone { get; set; }
    }
}