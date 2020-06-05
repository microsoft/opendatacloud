// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.Search.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Msr.Odr.Model.Datasets
{
    /// <summary>
    /// Represents a collection of files in a published dataset.
    /// </summary>
    [SerializePropertyNamesAsCamelCase]
    [JsonObject("dataset")]
    public class DatasetStorageItem
    {
		public DatasetStorageItem()
        {
            Tags = new List<string>();
			FileTypes = new List<string>();
        }

        /// <summary>
        /// The dataset identifier
        /// </summary>
        [JsonProperty("datasetId")]
        public Guid DatasetId => Id;

        /// <summary>
        /// The unique identifier for the dataset
        /// </summary>
        [JsonProperty("id")]
        public Guid Id { get; set; }

        /// <summary>
        /// The name of the dataset
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// The description for the dataset
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; }

        /// <summary>
        /// The source URI for the dataset
        /// </summary>
        [JsonProperty("sourceUri")]
        public string SourceUri { get; set; }

        /// <summary>
        /// The project URI for the dataset
        /// </summary>
        [JsonProperty("projectUri")]
        public string ProjectUri { get; set; }

        /// <summary>
        /// The version number of the dataset
        /// </summary>
        [JsonProperty("version")]
        public string Version { get; set; }

        /// <summary>
        /// The published date for the record
        /// </summary>
        [JsonProperty("published")]
        public DateTime? Published { get; set; }

        /// <summary>
        /// The created date for the record
        /// </summary>
        [JsonProperty("created")]
        public DateTime? Created { get; set; }

        /// <summary>
        /// The modified date for the record
        /// </summary>
        [JsonProperty("modified")]
        public DateTime? Modified { get; set; }

        /// <summary>
        /// The license name
        /// </summary>
        [JsonProperty("license")]
        public string License { get; set; }

        /// <summary>
        /// The license identifier
        /// </summary>
        [JsonProperty("licenseId")]
        public Guid LicenseId { get; set; }

        /// <summary>
        /// The domain name
        /// </summary>
        [JsonProperty("domain")]
        public string Domain { get; set; }

        /// <summary>
        /// The domain identifier
        /// </summary>
        [JsonProperty("domainId")]
        public string DomainId { get; set; }

        /// <summary>
        /// The data type for the dataset
        /// </summary>
        [JsonProperty("dataType")]
        [JsonConverter(typeof(StringEnumConverter))]
        public StorageDataType DataType => StorageDataType.Dataset;

        /// <summary>
        /// The tags
        /// </summary>
        [JsonProperty("tags")]
        public ICollection<string> Tags { get; set; }

        /// <summary>
        /// The file count
        /// </summary>
        [JsonProperty("fileCount", NullValueHandling = NullValueHandling.Ignore)]
        public int FileCount { get; set; }

        /// <summary>
        /// The file types
        /// </summary>
        [JsonProperty("fileTypes")]
        public ICollection<string> FileTypes { get; set; }

        /// <summary>
        /// The size of the dataset
        /// </summary>
        [JsonProperty("size", NullValueHandling = NullValueHandling.Ignore)]
        public long Size { get; set; }

        /// <summary>
        /// The size of the zip file
        /// </summary>
        [JsonProperty("zipFileSize", NullValueHandling = NullValueHandling.Ignore)]
        public long ZipFileSize { get; set; }

        /// <summary>
        /// The size of the gzip file
        /// </summary>
        [JsonProperty("gzipFileSize", NullValueHandling = NullValueHandling.Ignore)]
        public long GzipFileSize { get; set; }

        /// <summary>
        /// Gets or sets the name of the creator of the dataset.
        /// </summary>
        /// <value>The creator's name.</value>
        [JsonProperty("createdByUserName")]
        public string CreatedByUserName { get; set; }

        /// <summary>
        /// Gets or sets the email of the creator of the dataset.
        /// </summary>
        /// <value>The creator's email.</value>
        [JsonProperty("createdByUserEmail")]
        public string CreatedByUserEmail { get; set; }

        /// <summary> 
        /// Gets or sets the name of the modifier of the dataset.
        /// </summary>
        /// <value>The modifier's name.</value>
        [JsonProperty("modifiedByUserName")]
        public string ModifiedByUserName { get; set; }

        /// <summary>
        /// Gets or sets the name of the modifier of the dataset.
        /// </summary>
        /// <value>The modifier's email.</value>
        [JsonProperty("modifiedByUserEmail")]
        public string ModifiedByUserEmail { get; set; }

        /// <summary>
        ///  True if there are compressed versions available
        /// </summary>
        [JsonProperty("isCompressedAvailable")]
        public bool? IsCompressedAvailable { get; set; }

        /// <summary>
        ///  True if the dataset can be directly downloaded
        /// </summary>
        [JsonProperty("isDownloadAllowed")]
        public bool? IsDownloadAllowed { get; set; }

        /// <summary>
        ///  True if the dataset is featured
        /// </summary>
        [JsonProperty("isFeatured")]
        public bool? IsFeatured { get; set; }

        /// <summary>
        /// The digital object identifier for the dataset.
        /// </summary>
        [JsonProperty("digitalObjectIdentifier")]
        public string DigitalObjectIdentifier { get; set; }

        /// <summary>
        /// The set of "owners" of the dataset.
        /// </summary>
        [JsonProperty("datasetOwners")]
        public ICollection<DatasetOwner> DatasetOwners { get; set; }
    }
}
