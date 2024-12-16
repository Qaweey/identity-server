using Newtonsoft.Json;

namespace settl.identityserver.Domain.Shared.Helpers
{
    public class JsonHelper
    {
        public static string SerializeObject(object objectInstance) => JsonConvert.SerializeObject(objectInstance);

        public static object DeserializeObject(string json) => JsonConvert.DeserializeObject(json);

        public static T DeserializeObject<T>(string json) => JsonConvert.DeserializeObject<T>(json);
    }
}