using Newtonsoft.Json;

namespace settl.identityserver.Domain.Shared.Helpers
{
    public class SerializeUtility
    {
        public static string SerializeJSON(object objectInstance)
        {
            return JsonConvert.SerializeObject(objectInstance);
        }
    }
}
