using StackExchange.Redis;
using STRHM.Attributes;
using STRHM.Collections;
using STRHM.Configuration;
using STRHM.Extensions;
using STRHM.Interfaces;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace STRHM.Repositories
{
    public abstract class BaseRedisHashSetRepository<T>
        where T : class
    {
        protected readonly IStronglyTypedRedisSerializer Serializer;
        protected readonly RedisHashSetOptions ConfigurationOptions;

        protected BaseRedisHashSetRepository(
            IStronglyTypedRedisSerializer serializer,
            RedisHashSetOptions configurationOptions
            )
        {
            Serializer = serializer;
            ConfigurationOptions = configurationOptions;
        }

        protected abstract IDatabase GetDatabase(int databaseId);

        #region Exposed methods

        public async Task<StronglyTypedDictionary<T>> HashGetAsync(string key, params Expression<Func<T, object>>[] properties)
        {
            var propertiesAsRedisValues = TransformExpressionIntoRedisValues(properties);
            var values = await GetDatabase(ConfigurationOptions.Database).HashGetAsync(ConfigurationOptions.KeyNamespace + key, propertiesAsRedisValues, ConfigurationOptions.PrefferedReadFlags);
            return Map(values, properties);
        }

        public async Task HashSetAsync(string key, StronglyTypedDictionary<T> updates)
        {
            await GetDatabase(ConfigurationOptions.Database).HashSetAsync(ConfigurationOptions.KeyNamespace + key, TransformDictionaryIntoHashEntries(updates), ConfigurationOptions.PrefferedWriteFlags);
        }

        public async Task SaveAsync(string key, T model)
        {
            if (String.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            if (model == null)
                throw new ArgumentNullException(nameof(model));

            await GetDatabase(ConfigurationOptions.Database).HashSetAsync(ConfigurationOptions.KeyNamespace + key, Map(model), ConfigurationOptions.PrefferedWriteFlags);
        }

        public async Task<T> GetAsync(string key)
        {
            var values = await GetDatabase(ConfigurationOptions.Database).HashGetAsync(ConfigurationOptions.KeyNamespace + key, ObjectPropertyNames, ConfigurationOptions.PrefferedReadFlags);
            return Map(values);
        }

        public async Task<bool> DeleteAsync(string key)
        {
            return await GetDatabase(ConfigurationOptions.Database).KeyDeleteAsync(ConfigurationOptions.KeyNamespace + key, ConfigurationOptions.PrefferedWriteFlags);
        }

        public async Task RemoveExpirationAsync(string key)
        {
            await GetDatabase(ConfigurationOptions.Database).KeyExpireAsync(ConfigurationOptions.KeyNamespace + key, (TimeSpan?)null, ConfigurationOptions.PrefferedWriteFlags);
        }

        public async Task SetExpirationAsync(string key, TimeSpan expiration)
        {
            await GetDatabase(ConfigurationOptions.Database).KeyExpireAsync(ConfigurationOptions.KeyNamespace + key, expiration, ConfigurationOptions.PrefferedWriteFlags);
        }

        #endregion

        #region Privates 

        private HashEntry[] TransformDictionaryIntoHashEntries(StronglyTypedDictionary<T> updates)
        {
            // ensure that value is not null, otherway exception is throw, use emtpy string instead
            return updates.Where(c => c.Value != null).Select(s => new HashEntry(s.Key, s.Value.ToString())).ToArray();
        }

        private RedisValue[] TransformExpressionIntoRedisValues(params Expression<Func<T, object>>[] properties)
        {
            return properties.Select(c => (RedisValue)c.GetPropertyName()).ToArray();
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
                        value = Serializer.Serialize(objectProperty.GetValue(obj, null) ?? String.Empty);
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

                if (!redisValue.HasValue)
                    expandoDict[property.Name] = null;

                if (redisValue.IsJson())
                    expandoDict[property.Name] = Serializer.Deserialize<dynamic>(redisValue.ToString());
                else
                    expandoDict[property.Name] = redisValue.ToString();
            }

            string serializedObject = Serializer.Serialize(obj);
            return Serializer.Deserialize<T>(serializedObject);
        }

        private StronglyTypedDictionary<T> Map(RedisValue[] values, params Expression<Func<T, object>>[] properties)
        {
            // ensure same amount of Redis result values is present as requested using expression
            if (values.Length != properties.Length)
                throw new ArgumentException("Object properties not matching");

            var dictionary = new StronglyTypedDictionary<T>(Serializer);
            for (int i = 0; i < properties.Length; i++)
            {
                if (values[i].HasValue && properties[i].IsPropertySerializable())
                    dictionary.Add(properties[i], Serializer.Deserialize<dynamic>(values[i]));
                else
                    dictionary.Add(properties[i], values[i]);
            }

            return dictionary;
        }

        #endregion
    }
}
