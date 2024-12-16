namespace settl.identityserver.Domain.Models
{
    public class Response<T>
    {
        public string Code { get; set; }

        public string Message { get; set; }

        public T Data { get; set; }

        public object Errors { get; set; }
    }
}