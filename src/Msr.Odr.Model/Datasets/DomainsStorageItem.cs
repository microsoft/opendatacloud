using System;
using System.Collections.Generic;
using Microsoft.Azure.Search.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Msr.Odr.Model.Datasets
{
    /// <summary>
    /// Represents a collection of files in a published dataset.
    /// </summary>
    [SerializePropertyNamesAsCamelCase]
    [JsonObject("domains")]
    public class DomainsStorageItem
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
        /// The name of the dataset
        /// </summary>
        [JsonProperty("domains")]
        public ICollection<Domain> Domains;

        /// <summary>
        /// The data type for the dataset
        /// </summary>
        [JsonProperty("dataType")]
        [JsonConverter(typeof(StringEnumConverter))]
        public StorageDataType DataType;
    }
}
