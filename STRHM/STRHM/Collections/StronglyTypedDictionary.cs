using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq.Expressions;
using Newtonsoft.Json;
using STRHM.Extensions;

namespace STRHM.Collections
{
    public class StronglyTypedDictionary<T> : Dictionary<string,object>
        where T: class
    {
        public void Add(Expression<Func<T, object>> key, object value)
        {
            this[key] = value;
        }

        public bool Remove(Expression<Func<T, object>> key)
        {
            return Remove(key.GetPropertyName());
        }

        public TK Get<TK>(Expression<Func<T, object>> key)
        {
            var value = this[key];

            if (value == null || String.IsNullOrEmpty(value.ToString()))
                return default(TK);

            if (value.ToString().IsJson())
                return JsonConvert.DeserializeObject<TK>(value.ToString());

            // https://msdn.microsoft.com/en-us/library/system.componentmodel.typedescriptor(v=vs.110).aspx
            TypeConverter converter = TypeDescriptor.GetConverter(typeof(TK));
            return (TK)converter.ConvertFromString(null, CultureInfo.InvariantCulture, value.ToString());
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
                    base[key.GetPropertyName()] = value == null ? String.Empty : JsonConvert.SerializeObject(value);
                else
                    base[key.GetPropertyName()] = value;
            }
        }
    }
}
