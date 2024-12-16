using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace settl.identityserver.Application.Contracts.DTO.Users
{
    public class RegisterNewDeviceDto
    {
        public string PhoneNo { get; set; }
        public string OTP { get; set; }

        public string IMEINO { get; set; }

        public string PhoneName { get; set; }

        public string PhoneModelNo { get; set; }
        public string IPAddress { get; set; }
    }

    public class EmailNewDeviceDto
    {
    }
}