using Microsoft.Azure.Search.Models;
using Newtonsoft.Json;

namespace Msr.Odr.Model.Datasets
{
    /// <summary>
    /// Details about an "owner" of a dataset.
    /// </summary>
    [SerializePropertyNamesAsCamelCase]
    [JsonObject("datasetOwner")]
    public class DatasetOwner
    {
        /// <summary>
        /// The dataset owner name
        /// </summary>
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        /// <summary>
        /// The dataset owner email address
        /// </summary>
        [JsonProperty("email")]
        public string Email { get; set; }
    }
}
