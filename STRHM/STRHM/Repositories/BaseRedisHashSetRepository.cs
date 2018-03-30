using System;
using System.Collections.Generic;
using System.Configuration;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using StackExchange.Redis;
using STRHM.Attributes;
using STRHM.Extensions;

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

        public T Get(string key)
        {
            var database = GetConnection.GetDatabase(Database);
            return Map(database.HashGet(KeyNamespace + key, ObjectPropertyNames));
        }

        #endregion

        #region Privates

        private RedisValue[] ObjectPropertyNames
        {
            get
            {
                var data = typeof(T).GetProperties()
                    .Select(c => (RedisValue)c.Name)
                    .ToArray();
                return data;
            }
        }

        /// <summary>
        /// Maps object into HashEntry values array
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private HashEntry[] Map(T obj)
        {
            var values = new List<HashEntry>();
            foreach (var propertyName in ObjectPropertyNames)
            {
                var objectProperty = obj.GetType().GetProperties().First(pi => pi.Name == propertyName);
                if (objectProperty == null)
                    throw new Exception($"Couldn't get object property named {propertyName}");

                string value = String.Empty; // default value

                if (objectProperty.GetCustomAttributes<SerializableRedisPropertyAttribute>(false).Any())
                {
                    var propertyValue = objectProperty.GetValue(obj, null);
                    // avoid persisting "" value as a json object, issues deserializing
                    if (propertyValue != null)
                        value = JsonConvert.SerializeObject(objectProperty.GetValue(obj, null) ?? String.Empty);
                }
                else
                    value = (objectProperty.GetValue(obj, null) ?? String.Empty).ToString();

                values.Add(new HashEntry(propertyName, value));
            }
            return values.ToArray();
        }

        /// <summary>
        /// Maps redis values array into our generic model
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        private T Map(RedisValue[] values)
        {
            // redis properties count must match count of object properties
            // in order to get right property result for corresponding property
            var properties = typeof(T).GetProperties();
            if (properties.Count() != values.Count())
                throw new ArgumentException("Object properties not matching");

            dynamic obj = new ExpandoObject();
            var expandoDict = obj as IDictionary<string, object>;

            for (int i = 0; i < properties.Count(); i++)
            {
                var redisValue = values[i];
                var property = properties.ElementAt(i);

                if (redisValue.IsJson())
                    expandoDict[property.Name] = JsonConvert.DeserializeObject<dynamic>(redisValue);
                else
                    expandoDict[property.Name] = redisValue;
            }

            string serializedObject = JsonConvert.SerializeObject(obj);
            return JsonConvert.DeserializeObject<T>(serializedObject);
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
