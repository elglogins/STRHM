using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq.Expressions;
using Newtonsoft.Json;
using STRHM.Extensions;

namespace STRHM.Collections
{
    public class StronglyTypedDictionary<T> : Dictionary<string,string>
        where T: class
    {
        public void Add(Expression<Func<T, object>> key, string value)
        {
            Add(key.GetPropertyName(), value);
        }

        public bool Remove(Expression<Func<T, object>> key)
        {
            return Remove(key.GetPropertyName());
        }

        public TK Get<TK>(Expression<Func<T, object>> key)
        {
            var value = this[key];

            if (value == null)
                return default(TK);

            if (value.IsJson())
                return JsonConvert.DeserializeObject<TK>(value);

            TypeConverter converter = TypeDescriptor.GetConverter(typeof(TK));
            return (TK)converter.ConvertFromString(null, CultureInfo.InvariantCulture, value);
        }

        public string this[Expression<Func<T, object>> key]
        {
            get
            {
                if (ContainsKey(key.GetPropertyName()))
                    return base[key.GetPropertyName()];

                return null;
            }

            set
            {
                base[key.GetPropertyName()] = value;
            }
        }
    }
}
