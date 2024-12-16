namespace settl.identityserver.Domain.Entities
{
    public class EmailTemplate
    {
        public string FromName { get; set; }
        public string FromEmail { get; set; }
        public string EmailCode { get; set; }
        public string Description { get; set; }
        public string EmailType { get; set; }
        public string Status { get; set; }
        public string Templates { get; set; }
        public string Subject { get; set; }
        public string CreatedBy { get; set; }
    }
}