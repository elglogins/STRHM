using System;
using System.Collections.Generic;
using System.Linq.Expressions;
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
