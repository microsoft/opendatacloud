using System;
using System.Collections.Generic;
using Microsoft.Azure.Search.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Msr.Odr.Model.Datasets
{
    [JsonObject("datasetStorage")]
    public class DatasetStorageDetailsStorageItem
    {
        [JsonProperty("datasetId")]
        public Guid DatasetId { get; set; }

        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("storageType")]
        public string StorageType { get; set; } = "blob";

        [JsonProperty("container")]
        public string Container { get; set; }

        [JsonProperty("account")]
        public string Account { get; set; }

        [JsonProperty("primaryUri")]
        public Uri PrimaryUri { get; set; }

        [JsonProperty("dataType")]
        [JsonConverter(typeof(StringEnumConverter))]
        public StorageDataType DataType
        {
            get;
            private set;
        } = StorageDataType.Storage;
    };
}
