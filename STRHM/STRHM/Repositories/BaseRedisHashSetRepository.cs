using System;
using System.Collections.Generic;
using System.Configuration;
using StackExchange.Redis;

namespace STRHM.Repositories
{
    public abstract class BaseRedisHashSetRepository<T>
    {
        private static ConfigurationOptions _configurationOptions;

        public BaseRedisHashSetRepository()
        {
            var conString = ConfigurationManager.ConnectionStrings["RedisConnectionString"].ConnectionString;
            _configurationOptions = new ConfigurationOptions();
            _configurationOptions.EndPoints.Add(conString);
        }

        #region Connection

        protected static ConnectionMultiplexer GetConnection
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

        #endregion
    }
}
