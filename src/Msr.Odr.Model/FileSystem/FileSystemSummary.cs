// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Msr.Odr.Model.Datasets;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Msr.Odr.Model.FileSystem
{
    /// <summary>
    /// Dataset file information summary record
    /// </summary>
    [JsonObject("fileSummary")]
    public class FileSystemSummary
    {
        [JsonProperty("datasetId")]
        public Guid DatasetId { get; set; }

        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("size")]
        public long Size { get; set; } = 0;

        [JsonProperty("fileCount")]
        public int FileCount { get; set; } = 0;

        [JsonProperty("fileTypes")]
        public ICollection<string> FileTypes { get; set; }

        [JsonProperty("dataType")]
        [JsonConverter(typeof(StringEnumConverter))]
        public StorageDataType DataType { get; set; }
    }
}
