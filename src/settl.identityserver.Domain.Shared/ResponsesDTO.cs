using System.Collections.Generic;

namespace settl.identityserver.Domain.Shared
{
    public class ResponsesDTO
    {
        public string Code { get; set; }

        public string Message { get; set; }

        public object Data { get; set; }

        public List<string> Errors { get; set; }
    }
}