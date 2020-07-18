using Newtonsoft.Json;

namespace Authentication
{
    public class JsonHelper
    {
        public static string Serialize(object value, JsonSerializerSettings settings = null)
        {
            return JsonConvert.SerializeObject(value, settings);
        }

        public static T Deserialize<T>(string value, JsonSerializerSettings settings = null)
        {
            return JsonConvert.DeserializeObject<T>(value, settings);
        }
    }
}