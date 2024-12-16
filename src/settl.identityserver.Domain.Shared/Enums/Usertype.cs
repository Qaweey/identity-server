using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace settl.identityserver.Domain.Shared.Enums
{
    public class Usertype
    {
        public enum UserType
        {
            CONSUMER = 1,
            SUPER_AGENT = 2,
            SUB_AGENT = 3,
            INDI_AGENT,
            MERCHANT = 5,
            ADMIN = 6
        }
    }
}