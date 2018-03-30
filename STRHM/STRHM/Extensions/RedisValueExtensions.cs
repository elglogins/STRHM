using System;
using StackExchange.Redis;

namespace STRHM.Extensions
{
    public static class RedisValueExtensions
    {
        public static bool IsJson(this RedisValue value)
        {
            if (!value.HasValue || value.IsNullOrEmpty)
                return false;

            return value.ToString().IsJson();
        }
    }
}
