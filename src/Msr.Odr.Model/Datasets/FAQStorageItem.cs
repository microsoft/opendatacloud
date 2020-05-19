using System;
using Microsoft.Azure.Search.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Msr.Odr.Model.Datasets
{
    /// <summary>
    /// Represents a collection of files in a published dataset.
    /// </summary>
    [SerializePropertyNamesAsCamelCase]
    [JsonObject("faq")]
    public class FAQStorageItem
    {
        /// <summary>
        /// The dataset identifier
        /// </summary>
        [JsonProperty("datasetId")]
        public Guid DatasetId;

        /// <summary>
        /// The unique identifier for the dataset
        /// </summary>
        [JsonProperty("id")]
        public Guid Id;

        /// <summary>
        /// Gets or sets the title of the FAQ
        /// </summary>
        /// <value>The title.</value>
        [JsonProperty("title")]
        public string Title;

        /// <summary>
        /// Gets or sets the HTML content of the FAQ
        /// </summary>
        /// <value>The content.</value>
        [JsonProperty("content")]
        public string Content;

        /// <summary>
        /// Value to determine the display order or the FAQ
        /// </summary>
        /// <value>The order.</value>
        [JsonProperty("order")]
        public double Order;

        /// <summary>
        /// The data type for the dataset
        /// </summary>
        [JsonProperty("dataType")]
        [JsonConverter(typeof(StringEnumConverter))]
        public StorageDataType DataType;
    }
}
