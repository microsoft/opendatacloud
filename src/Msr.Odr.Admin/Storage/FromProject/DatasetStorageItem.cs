// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

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
    [JsonObject("dataset")]
    public class DatasetStorageItem
    {
        private string _sourceUri = null;
        private string _projectUri = null;

        /// <summary>
        /// The dataset identifier
        /// </summary>
        [JsonProperty("datasetId")]
        //[IsRetrievable(true)]
        public Guid DatasetId;

        /// <summary>
        /// The unique identifier for the dataset
        /// </summary>
        [JsonProperty("id")]
        public Guid Id;

        /// <summary>
        /// The name of the dataset
        /// </summary>
        [JsonProperty("name")]
        public string Name;

        /// <summary>
        /// The description for the dataset
        /// </summary>
        [JsonProperty("description")]
        public string Description;

        /// <summary>
        /// The dataset owner name
        /// </summary>
        [JsonProperty("ownerName")]
        public string OwnerName;

        /// <summary>
        /// The owner identifier
        /// </summary>
        [JsonProperty("ownerId")]
        public Guid OwnerId;

        /// <summary>
        /// The source URI for the dataset
        /// </summary>
        [JsonProperty("sourceUri")]
        public string SourceUri
        {
            get { return _sourceUri; }
            set { _sourceUri = value; }
        }
        [JsonProperty("sourceUrl")]
        public string SourceUrl
        {
            get { return _sourceUri; }
            set { _sourceUri = value; }
        }

		/// <summary>
        /// The project URI for the dataset
        /// </summary>
        [JsonProperty("projectUri")]
        public string ProjectUri
        {
            get { return _projectUri; }
            set { _projectUri = value; }
        }
        [JsonProperty("projectUrl")]
        public string ProjectUrl
        {
            get { return _projectUri; }
            set { _projectUri = value; }
        }

        /// <summary>
        /// The version number of the dataset
        /// </summary>
        [JsonProperty("version")]
        public string Version;

        /// <summary>
        /// The published date for the record
        /// </summary>
        [JsonProperty("published")]
        public DateTime? Published;

        /// <summary>
        /// The created date for the record
        /// </summary>
        [JsonProperty("created")]
        public DateTime? Created;

        /// <summary>
        /// The modified date for the record
        /// </summary>
        [JsonProperty("modified")]
        public DateTime? Modified;

        /// <summary>
        /// The license name
        /// </summary>
        [JsonProperty("license")]
        public string License;

        /// <summary>
        /// The license identifier
        /// </summary>
        [JsonProperty("licenseId")]
        public Guid LicenseId;


        /// <summary>
        /// The domain name
        /// </summary>
        [JsonProperty("domain")]
        public string Domain;

        /// <summary>
        /// The domain identifier
        /// </summary>
        [JsonProperty("domainId")]
        public string DomainId;

        /// <summary>
        /// The data type for the dataset
        /// </summary>
        [JsonProperty("dataType")]
        [JsonConverter(typeof(StringEnumConverter))]
        public StorageDataType DataType;

        /// <summary>
        /// The tags
        /// </summary>
        [JsonProperty("tags")]
        public IEnumerable<string> Tags;

        /// <summary>
        /// The file count
        /// </summary>
        [JsonProperty("fileCount")]
        public int FileCount = 0;

        /// <summary>
        /// The file types
        /// </summary>
        [JsonProperty("fileTypes")]
        public IEnumerable<string> FileTypes;

        /// <summary>
        /// The size of the dataset
        /// </summary>
        [JsonProperty("size")]
        public long Size;

        /// <summary>
        /// The size of the zip file
        /// </summary>
        [JsonProperty("zipFileSize")]
        public long ZipFileSize;

        /// <summary>
        /// The size of the gzip file
        /// </summary>
        [JsonProperty("gzipFileSize")]
        public long GzipFileSize;
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
        public bool? IsCompressedAvailable;


        /// <summary>
        ///  True if the dataset can be directly downloaded
        /// </summary>
        [JsonProperty("isDownloadAllowed")]
        public bool? IsDownloadAllowed;

        /// <summary>
        ///  True if the dataset is featured
        /// </summary>
        [JsonProperty("isFeatured")]
        public bool? IsFeatured;

        /// <summary>
        ///     gets or sets the digital object identifier
        /// </summary>
        [JsonProperty("digitalObjectIdentifier")]
        public string DigitalObjectIdentifier { get; set; }
    }
}
