using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaContent
{
    public class ContentMeta
    {
        public Guid ContentId { get; set; }

        public Guid ContentTypeId { get; set; }

        public string Key { get; set; }

        public string Value { get; set; }
    }
}
