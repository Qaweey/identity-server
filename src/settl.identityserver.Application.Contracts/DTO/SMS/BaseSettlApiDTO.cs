namespace settl.identityserver.Application.Contracts.DTO.SMS
{
    public class BaseSettlApiDTO
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public string Data { get; set; }
        public object Errors { get; set; }
    }
}