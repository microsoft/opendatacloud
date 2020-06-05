// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Azure.Search.Models;
using Msr.Odr.Model.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Msr.Odr.Model.Datasets
{
    /// <summary>
    /// Represents configuration for dataset owners
    /// </summary>
    [SerializePropertyNamesAsCamelCase]
    [JsonObject("datasetOwners")]
    public class DatasetOwnersStorageItem
    {
        /// <summary>
        /// The dataset identifier
        /// </summary>
        [JsonProperty("datasetId")]
        public Guid DatasetId => WellKnownIds.ConfigurationDatasetId;

        /// <summary>
        /// The unique identifier for the dataset
        /// </summary>
        [JsonProperty("id")]
        public Guid Id => WellKnownIds.DatasetOwnersDocId;

        /// <summary>
        /// A list of regular expressions that identify email addresses that are elibible to
        /// be dataset owners.
        /// </summary>
        [JsonProperty("eligible")]
        public ICollection<string> Eligible { get; set; }

        /// <summary>
        /// The data type for the dataset
        /// </summary>
        [JsonProperty("dataType")]
        [JsonConverter(typeof(StringEnumConverter))]
        public StorageDataType DataType => StorageDataType.DatasetOwners;
    }
}
