using System.Collections.Generic;
using Telligent.Evolution.Components;

namespace MetaContent.Extensions
{
    public static class MetaExtensions
    {
        public static IDictionary<string, string> GetMetaData(this IContent content)
        {
            // TODO: Get items here and cache.
            return new Dictionary<string, string>();
        }

        public static string GetString(this IContent content, string key)
        {
            return content.GetMetaData()[key];
        }

        public static int GetInt(this IContent content, string key, int defaultValue)
        {
            int outValue;
            if (!string.IsNullOrEmpty(content.GetMetaData()[key]) && int.TryParse(content.GetMetaData()[key], out outValue))
                return outValue;

            return defaultValue;
        }

        public static bool GetBool(this IContent content, string key, bool defaultValue)
        {
            bool outValue;
            if (!string.IsNullOrEmpty(content.GetMetaData()[key]) && bool.TryParse(content.GetMetaData()[key], out outValue))
                return outValue;

            return defaultValue;
        }

        public static void SetItem(this IContent content, string key, string value)
        {
            // TODO: Write the new value to the DB.
        }
    }
}
