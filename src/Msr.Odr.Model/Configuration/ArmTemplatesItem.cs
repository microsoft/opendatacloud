using System.Collections.Generic;
using System.Linq;


namespace Msr.Odr.Model.Configuration
{
    /// <summary>
    /// Information about a particular ARM template
    /// </summary>
    public class ArmTemplatesItem
    {
        /// <summary>
        /// Id within Cosmos of ARM template document.
        /// </summary>
        public string Id { get; set; }
    }

    public class ArmTemplatesMap : Dictionary<string, ArmTemplatesItem>
    {
        public ArmTemplatesMap()
        {
        }

        public ArmTemplatesMap(IEnumerable<KeyValuePair<string, ArmTemplatesItem>> items) :
            base(items.ToDictionary(x => x.Key, x => x.Value))
        {
        }
    }
}
