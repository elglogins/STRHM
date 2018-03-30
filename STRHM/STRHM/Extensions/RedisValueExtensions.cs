using StackExchange.Redis;

namespace STRHM.Extensions
{
    public static class RedisValueExtensions
    {
        public static bool IsJson(this RedisValue value)
        {
            if (!value.HasValue || value.IsNullOrEmpty)
                return false;

            // cleanup value content
            var cleanedValue = value.ToString().Trim();

            // if starts and ends with matching Parentes, then we assume
            // that it is a json object value
            if (cleanedValue.StartsWith("{") && cleanedValue.EndsWith("}"))
                return true;

            // if starts and ends with matching square brackets, then we assume
            // that it is a json array value
            if (cleanedValue.StartsWith("[") && cleanedValue.EndsWith("]"))
                return true;

            return false;
        }
    }
}
