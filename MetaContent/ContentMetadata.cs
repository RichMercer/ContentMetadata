using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentMetadata
{
    public class ContentMetadata
    {
        public ContentMetadata() { }

        public ContentMetadata(Guid contentId, Guid contentTypeId, string key, string value)
        {
            ContentId = contentId;
            ContentTypeId = contentTypeId;
            Key = key;
            Value = value;
        }
        public Guid ContentId { get; }

        public Guid ContentTypeId { get; }

        public string Key { get; }

        public string Value { get; }

        public bool GetBoolValue(bool defaultValue)
        {
            bool outValue;
            if (!string.IsNullOrEmpty(Value) && bool.TryParse(Value, out outValue))
                return outValue;

            return defaultValue;
        }

        public int GetIntValue(int defaultValue)
        {
            int outValue;
            if (!string.IsNullOrEmpty(Value) && int.TryParse(Value, out outValue))
                return outValue;

            return defaultValue;
        }
    }
}
