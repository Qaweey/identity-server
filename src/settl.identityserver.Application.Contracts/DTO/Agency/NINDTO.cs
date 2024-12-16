namespace settl.identityserver.Application.Contracts.DTO.Agency
{
    public class NINDTO
    {
        public string sec_key { get; set; }
        public string timestamp { get; set; }
        public PartnerParam partner_params { get; set; }
        public string country { get; set; }
        public string id_type { get; set; }
        public string id_number { get; set; }
        public string partner_id { get; set; }
    }

    public class PartnerParam
    {
        public string job_id { get; set; }
        public string user_id { get; set; }
        public int job_type { get; set; }
    }
}
