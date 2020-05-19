using System.Collections.Generic;
using System.Linq;

namespace Msr.Odr.Model.Configuration
{
    /// <summary>
    /// Information about a particular static asset item used by an ARM template.
    /// </summary>
    public class StaticAssetsItem
    {
        /// <summary>
        /// Id within Cosmos of static asset document.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Mime type of static asset.
        /// </summary>
        public string MimeType { get; set; }
    }

    public class StaticAssetsMap : Dictionary<string, StaticAssetsItem>
    {
        public StaticAssetsMap()
        {
        }

        public StaticAssetsMap(IEnumerable<KeyValuePair<string, StaticAssetsItem>> items) :
            base(items.ToDictionary(x => x.Key, x => x.Value))
        {
        }
    }
}
