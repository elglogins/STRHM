using System;
using System.Collections.Generic;
using System.Configuration;
using StackExchange.Redis;

namespace STRHM.Repositories
{
    public abstract class BaseRedisHashSetRepository<T>
        where T : class
    {
        private static ConfigurationOptions _configurationOptions;
        protected readonly int Database;
        protected readonly string KeyNamespace;

        public BaseRedisHashSetRepository(string keyNamespace, int database)
        {
            var conString = ConfigurationManager.ConnectionStrings["RedisConnectionString"].ConnectionString;
            _configurationOptions = new ConfigurationOptions();
            _configurationOptions.EndPoints.Add(conString);
            Database = database;
            KeyNamespace = keyNamespace;
        }

        #region Methods

        public void Save(string key, T model)
        {
            var database = GetConnection.GetDatabase(Database);
            //TODO
        }

        #endregion

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
