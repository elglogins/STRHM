using StackExchange.Redis;
using System;

namespace STRHM.Repositories
{
    public abstract class AbstractRedisConnection
    {
        private static ConfigurationOptions _configurationOptions;

        public AbstractRedisConnection(string connectionString)
        {
            _configurationOptions = new ConfigurationOptions();
            _configurationOptions.EndPoints.Add(connectionString);
        }

        protected static ConnectionMultiplexer RedisConnectionMultiplexer
        {
            get
            {
                return LazyConnection.Value;
            }
        }

        protected static readonly Lazy<ConnectionMultiplexer> LazyConnection
            = new Lazy<ConnectionMultiplexer>(() =>
            {
                return ConnectionMultiplexer.Connect(_configurationOptions);
            });
    }
}
