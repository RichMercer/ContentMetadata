using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace ContentMetadata
{
    [Serializable]
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

    [Serializable]
    public class RestContentMetadata
    {
        public RestContentMetadata() { }

        public RestContentMetadata(ContentMetadata metadata)
        {
            ContentId = metadata.ContentId;
            ContentTypeId = metadata.ContentTypeId;
            Key = metadata.Key;
            Value = metadata.Value;
        }

        public Guid ContentId { get; set; }

        public Guid ContentTypeId { get; set; }

        public string Key { get; set; }

        public string Value { get; set; }
    }
}
