using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Newtonsoft.Json;
using StackExchange.Redis;
using STRHM.Attributes;
using STRHM.Collections;
using STRHM.Extensions;

namespace STRHM.Repositories
{
    public abstract class BaseRedisHashSetRepository<T> : AbstractRedisConnection
        where T : class
    {
        protected readonly int Database;
        protected readonly string KeyNamespace;

        public BaseRedisHashSetRepository(string connectionString, string keyNamespace, int database) : base(connectionString)
        {
            if (String.IsNullOrEmpty(keyNamespace))
                throw new ArgumentNullException(nameof(keyNamespace));

            Database = database;
            KeyNamespace = keyNamespace;
        }

        #region Exposed methods

        public StronglyTypedDictionary<T> HashGet(string key, CommandFlags flags = CommandFlags.None, params Expression<Func<T, object>>[] properties)
        {
            var database = RedisConnectionMultiplexer.GetDatabase(Database);
            var propertiesAsRedisValues = TransformExpressionIntoRedisValues(properties);
            return Map(database.HashGet(KeyNamespace + key, propertiesAsRedisValues, flags), properties);
        }

        public void HashSet(string key, StronglyTypedDictionary<T> updates, CommandFlags flags = CommandFlags.None)
        {
            IDatabase database = RedisConnectionMultiplexer.GetDatabase(Database);
            database.HashSet(KeyNamespace + key, TransformDictionaryIntoHashEntries(updates), flags);
        }

        public void Save(string key, T model, CommandFlags flags = CommandFlags.None)
        {
            if (String.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            if (model == null)
                throw new ArgumentNullException(nameof(model));

            var database = RedisConnectionMultiplexer.GetDatabase(Database);
            database.HashSet(KeyNamespace + key, Map(model), flags);
        }

        public T Get(string key, CommandFlags flags = CommandFlags.None)
        {
            var database = RedisConnectionMultiplexer.GetDatabase(Database);
            return Map(database.HashGet(KeyNamespace + key, ObjectPropertyNames, flags));
        }

        public void RemoveExpiration(string key, CommandFlags flags = CommandFlags.None)
        {
            var database = RedisConnectionMultiplexer.GetDatabase(Database);
            database.KeyExpire(KeyNamespace + key, (TimeSpan?)null, flags);
        }

        public void SetExpiration(string key, TimeSpan expiration, CommandFlags flags = CommandFlags.None)
        {
            var database = RedisConnectionMultiplexer.GetDatabase(Database);
            database.KeyExpire(KeyNamespace + key, expiration, flags);
        }

        #endregion
        
        #region Privates 

        private HashEntry[] TransformDictionaryIntoHashEntries(StronglyTypedDictionary<T> updates)
        {
            // ensure that value is not null, otherway exception is throw, use emtpy string instead
            return updates.Where(c=>c.Value != null).Select(s => new HashEntry(s.Key, s.Value.ToString() )).ToArray();
        }

        private RedisValue[] TransformExpressionIntoRedisValues(params Expression<Func<T, object>>[] properties)
        {
            return properties.Select(c=> (RedisValue) c.GetPropertyName()).ToArray();
        }

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

        /// <summary>
        /// Maps Redis result values to strongly typed dictionary of T type
        /// </summary>
        /// <param name="values"></param>
        /// <param name="properties"></param>
        /// <returns></returns>
        private StronglyTypedDictionary<T> Map(RedisValue[] values, params Expression<Func<T, object>>[] properties)
        {
            // ensure same amount of Redis result values is present as requested using expression
            if (values.Length != properties.Length)
                throw new ArgumentException("Object properties not matching");

            var dictionary = new StronglyTypedDictionary<T>();
            for (int i = 0; i < properties.Length; i++)
            {
                if (values[i].HasValue && properties[i].IsPropertySerializable())
                    dictionary.Add(properties[i], JsonConvert.DeserializeObject<dynamic>(values[i]));
                else
                    dictionary.Add(properties[i], values[i]);
            }
            
            return dictionary;
        }

        #endregion
    }
}
