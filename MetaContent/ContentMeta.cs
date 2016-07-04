using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaContent
{
    public class ContentMeta
    {
        public ContentMeta(Guid contentId, Guid contentTypeId, string key, string value)
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
    }
}
