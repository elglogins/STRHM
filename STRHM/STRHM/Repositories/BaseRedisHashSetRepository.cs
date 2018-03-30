using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
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
            if (String.IsNullOrEmpty(keyNamespace))
                throw new ArgumentNullException(nameof(keyNamespace));

            var conString = ConfigurationManager.ConnectionStrings["RedisConnectionString"].ConnectionString;
            _configurationOptions = new ConfigurationOptions();
            _configurationOptions.EndPoints.Add(conString);

            Database = database;
            KeyNamespace = keyNamespace;
        }

        #region Exposed methods

        public void Save(string key, T model)
        {
            if (String.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            if (model == null)
                throw new ArgumentNullException(nameof(model));

            var database = GetConnection.GetDatabase(Database);
            database.HashSet(KeyNamespace + key, Map(model));
        }

        #endregion

        #region Privates

        /// <summary>
        /// Maps object into HashEntry values array
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private HashEntry[] Map(T obj)
        {
            var values = new List<HashEntry>();
            foreach (var property in typeof(T).GetProperties())
            {
                var objectProperty = obj.GetType().GetProperty(property.Name);

                if (objectProperty == null)
                    throw new Exception($"Couldn't get object property named {property.Name}");

                var value = (objectProperty.GetValue(obj, null) ?? String.Empty).ToString();
                values.Add(new HashEntry(property.Name, value));
            }
            return values.ToArray();
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
