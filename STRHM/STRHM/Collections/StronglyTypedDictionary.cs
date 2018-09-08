using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq.Expressions;
using STRHM.Extensions;
using STRHM.Serialization;

namespace STRHM.Collections
{
    public class StronglyTypedDictionary<T> : Dictionary<string,object>
        where T: class
    {
        protected readonly IStronglyTypedRedisSerializer Serializer;

        public StronglyTypedDictionary(IStronglyTypedRedisSerializer serializer)
        {
            Serializer = serializer;
        }

        public StronglyTypedDictionary()
        {

        }

        public void Add(Expression<Func<T, object>> key, object value)
        {
            this[key] = value;
        }

        public bool Remove(Expression<Func<T, object>> key)
        {
            return Remove(key.GetPropertyName());
        }

        public bool HasValue(Expression<Func<T, object>> key)
        {
            var value = this[key];
            return value != null && !String.IsNullOrEmpty(value.ToString());
        }

        public TK Get<TK>(Expression<Func<T, object>> key, CultureInfo cultureInfo = null)
        {
            var value = this[key];

            if (value == null || String.IsNullOrEmpty(value.ToString()))
                return default(TK);

            if (value.ToString().IsJson())
                return Serializer.Deserialize<TK>(value.ToString());

            // https://msdn.microsoft.com/en-us/library/system.componentmodel.typedescriptor(v=vs.110).aspx
            TypeConverter converter = TypeDescriptor.GetConverter(typeof(TK));

            try
            {
                return (TK)converter.ConvertFromString(null, cultureInfo ?? CultureInfo.CurrentCulture, value.ToString());
            }
            catch (Exception)
            {
                return default(TK);
            }
        }

        public TK Get<TK>(Expression<Func<T, object>> key)
        {
            return Get<TK>(key, CultureInfo.CurrentCulture);
        }

        public object this[Expression<Func<T, object>> key]
        {
            get
            {
                if (ContainsKey(key.GetPropertyName()))
                    return base[key.GetPropertyName()];

                return null;
            }

            set
            {
                var isSerializable = key.IsPropertySerializable();
                if (isSerializable)
                    base[key.GetPropertyName()] = value == null ? String.Empty : Serializer.Serialize(value);
                else
                    base[key.GetPropertyName()] = value;
            }
        }
    }
}
