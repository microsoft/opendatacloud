// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Azure.Search.Models;
using Msr.Odr.Model.Datasets;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Msr.Odr.Services
{
    /// <summary>
    /// Subset of Dataset fields stored in Azure Search index.
    /// </summary>
    /// <remarks>
    /// Used for directly refreshing the search index when the dataset
    /// details have changed. Related to https://github.com/Azure/azure-sdk-for-net/issues/7763.
    /// </remarks>
    [SerializePropertyNamesAsCamelCase]
    [JsonObject("dataset")]
    public class DatasetSearchItem
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime? Published { get; set; }
        public DateTime? Created { get; set; }
        public DateTime? Modified { get; set; }
        public string License { get; set; }
        public Guid LicenseId { get; set; }
        public string Domain { get; set; }
        public string DomainId { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public StorageDataType DataType => StorageDataType.Dataset;
        public ICollection<string> Tags { get; set; }
        public int FileCount { get; set; }
        public ICollection<string> FileTypes { get; set; }
        public long Size { get; set; }
        public bool? IsCompressedAvailable { get; set; }
        public bool? IsDownloadAllowed { get; set; }
        public bool? IsFeatured { get; set; }
    }
}
