using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace settl.identityserver.Application.Contracts.DTO.Consumer
{
    public class VerifyTransPin
    {
        public string Phoneno { get; set; }
        public string TransactionPin { get; set; }
    }
}