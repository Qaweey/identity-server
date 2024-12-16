using settl.identityserver.Domain.Shared.Enums;
using System.Collections.Generic;

namespace settl.identityserver.Application.Contracts.DTO
{
    public class SendEmailDTO
    {
        public string Name { get; set; }

        public string Email { get; set; }

        public string EmailCode { get; set; }

        public string FromEmail { get; set; }

        public string FromName { get; set; }
    }

    public class EmailEntityDto
    {
        public string EmailCode { get; set; }
        public string FromEmail { get; set; }
        public string FromName { get; set; }
        public string EmailType { get; set; }
        public string Templates { get; set; }
        public string Description { get; set; }
    }

    public class EmailRequest
    {
        public EmailRequest(string name = "", string email = "")
        {
            Name = name;
            Email = email;
        }

        public string Name { get; set; }
        public string Email { get; set; }
        public string EmailCode { get; set; }
        public string OtpCode { get; set; }

        public string MicroserviceName { get; set; } = Constants.IDENTITYSERVER_URL.ToUpper();
    }

    public class AWSEmailDTO
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string EmailCode { get; set; }
        public string FromEmail { get; set; }
        public string FromName { get; set; }
        public string Subject { get; set; }
        public string Templates { get; set; }

        public string Type { get; set; }

        public string Description { get; set; }

        public string MicroserviceName { get; set; } = Constants.IDENTITYSERVER_URL.ToUpper();
    }

    public class AWSBulkEmailDTO
    {
        public IList<EmailRequest> Recipients { get; set; }
        public string FromEmail { get; set; }
        public string FromName { get; set; }
        public string Subject { get; set; }
        public string Templates { get; set; }

        public string Type { get; set; }

        public string Description { get; set; }

        public string MicroserviceName { get; set; } = Constants.IDENTITYSERVER_URL.ToUpper();
    }

    public class BulkEmailRequest
    {
        public string EmailCode { get; set; }

        public string MicroserviceName { get; set; } = Constants.IDENTITYSERVER_URL.ToUpper();

        public EmailRequest[] UserData { get; set; }
    }

    public class EmailRequestTemplate
    {
        public string name { get; set; }
        public string email { get; set; }
        public string emailCode { get; set; }
        public string fromEmail { get; set; }
        public string fromName { get; set; }
        public string subject { get; set; }
        public string templates { get; set; }
        public string type { get; set; }
        public string description { get; set; }
        public string microserviceName { get; set; } = Constants.IDENTITYSERVER_URL.ToUpper();
    }
}