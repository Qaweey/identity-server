using settl.identityserver.Domain.Shared;

namespace settl.identityserver.Domain.Entities
{
    public class UserOnboarding : Entity
    {
        public string Phone { get; set; }

        public string Stage { get; set; }
    }
}
