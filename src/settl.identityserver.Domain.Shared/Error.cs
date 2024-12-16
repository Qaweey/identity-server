using settl.identityserver.Domain.Shared.Helpers;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace settl.identityserver.Domain.Shared
{
    public class Error
    {
        [JsonPropertyName("code")]
        public string Code { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("errors")]
        public List<string> Errors { get; set; }

        public override string ToString() => JsonHelper.SerializeObject(this);
    }
}