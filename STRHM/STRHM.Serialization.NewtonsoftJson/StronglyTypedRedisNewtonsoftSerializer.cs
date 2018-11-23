using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using STRHM.Interfaces;

namespace STRHM.Serialization.NewtonsoftJson
{
    public class StronglyTypedRedisNewtonsoftSerializer : IStronglyTypedRedisSerializer
    {
        private readonly IsoDateTimeConverter _dateTimeConverter;

        public StronglyTypedRedisNewtonsoftSerializer(IsoDateTimeConverter dateTimeConverter)
        {
            _dateTimeConverter = dateTimeConverter;
        }

        public StronglyTypedRedisNewtonsoftSerializer()
        {
            //DateTime.UtcNow.ToString("s")
            _dateTimeConverter = new IsoDateTimeConverter { };
        }


        public string Serialize(object obj)
        {
            return JsonConvert.SerializeObject(obj, _dateTimeConverter);
        }

        public T Deserialize<T>(string value)
        {
            return JsonConvert.DeserializeObject<T>(value, _dateTimeConverter);
        }
    }
}
