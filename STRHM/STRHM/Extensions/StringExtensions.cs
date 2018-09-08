using System;

namespace STRHM.Extensions
{
    public static class StringExtensions
    {
        public static bool IsJson(this string value)
        {
            if (String.IsNullOrEmpty(value))
                return false;

            // cleanup value content
            var cleanedValue = value.Replace("\"", "").Trim();

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
