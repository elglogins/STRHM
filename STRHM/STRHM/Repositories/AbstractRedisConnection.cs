using StackExchange.Redis;
using System;
using STRHM.Interfaces;

namespace STRHM.Repositories
{
    public class RedisConnection : IRedisConnection
    {
        private static ConfigurationOptions _configurationOptions;

        public RedisConnection(ConfigurationOptions options)
        {
            _configurationOptions = options;
        }

        private readonly Lazy<ConnectionMultiplexer> _lazyConnection
            = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(_configurationOptions));

        public ConnectionMultiplexer GetConnection => _lazyConnection.Value;
    }
}
