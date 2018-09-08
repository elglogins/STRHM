namespace STRHM.Configuration
{
    public class RedisHashSetOptions
    {
        public RedisHashSetOptions()
        {
            DateTimeSerializationFormat = ConfigurationConstants.DateTimeSerializationFormat;
        }

        public int Database { get; set; }
        public string KeyNamespace { get; set; }
        public string DateTimeSerializationFormat { get; set; }
    }
}
