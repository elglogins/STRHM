using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using STRHM.Interfaces;

namespace STRHM.Serialization.NewtonsoftJson
{
    public class StronglyTypedRedisNewtonsoftSerializer : IStronglyTypedRedisSerializer
    {
        public string Serialize(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        public string Serialize(object obj, string dateTimeFormat)
        {
            return JsonConvert.SerializeObject(obj, new IsoDateTimeConverter { DateTimeFormat = dateTimeFormat} );
        }

        public T Deserialize<T>(string value)
        {
            return JsonConvert.DeserializeObject<T>(value);
        }

        public T Deserialize<T>(string value, string dateTimeFormat)
        {
            return JsonConvert.DeserializeObject<T>(value, new IsoDateTimeConverter { DateTimeFormat = dateTimeFormat });
        }
    }
}
