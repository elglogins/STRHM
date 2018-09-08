using StackExchange.Redis;

namespace STRHM.Configuration
{
    public class RedisHashSetOptions
    {
        public RedisHashSetOptions()
        {
            DateTimeSerializationFormat = ConfigurationConstants.DateTimeSerializationFormat;
            PrefferedReadFlags = CommandFlags.None;
            PrefferedWriteFlags = CommandFlags.None;
        }

        public int Database { get; set; }
        public string KeyNamespace { get; set; }
        public string DateTimeSerializationFormat { get; set; }
        public CommandFlags PrefferedReadFlags { get; set; }
        public CommandFlags PrefferedWriteFlags { get; set; }
    }
}
