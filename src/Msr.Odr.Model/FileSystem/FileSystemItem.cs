// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Msr.Odr.Model.Datasets;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Msr.Odr.Model.FileSystem
{
    /// <summary>
    /// File information record
    /// </summary>
    public class FileSystemItem
    {
        /// <summary>
        /// The dataset identifier
        /// </summary>
        [JsonProperty("datasetId")]
        public Guid DatasetId;

        /// <summary>
        /// The identifier
        /// </summary>
        [JsonProperty("id")]
        public Guid Id;

        /// <summary>
        /// The name
        /// </summary>
        [JsonProperty("name")]
        public string Name;

        /// <summary>
        /// The full name
        /// </summary>
        [JsonProperty("fullname")]
        public string FullName;

        /// <summary>
        /// The full name
        /// </summary>
        [JsonProperty("fileType")]
        public string FileType;

        /// <summary>
        /// The parent
        /// </summary>
        [JsonProperty("parent")]
        public string Parent;

        /// <summary>
        /// The entry type
        /// </summary>
        [JsonProperty("entryType")]
        [JsonConverter(typeof(StringEnumConverter))]
        public FileSystemEntryType EntryType;

        /// <summary>
        /// The data type
        /// </summary>
        [JsonProperty("dataType")]
        [JsonConverter(typeof(StringEnumConverter))]
        public StorageDataType DataType;

        /// <summary>
        /// The sort key
        /// </summary>
        [JsonProperty("sortKey")]
        public string SortKey;

        /// <summary>
        /// Indicates if the file can be previewed
        /// </summary>
        [JsonProperty("canPreview")]
        public bool? CanPreview;

        /// <summary>
        /// Size of the file
        /// </summary>
        [JsonProperty("length")]
        public long? Length;

        /// <summary>
        /// Gets or sets the modified date.
        /// </summary>
        /// <value>The modified.</value>
        [JsonProperty("modified")]
        public DateTimeOffset Modified { get; set; }
    }
}
