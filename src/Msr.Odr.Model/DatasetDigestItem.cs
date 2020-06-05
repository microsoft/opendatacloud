// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Newtonsoft.Json;

namespace Msr.Odr.Model
{
    public class DatasetDigestItem
    {
        /// <summary>
        /// The dataset identifier
        /// </summary>
        [JsonProperty("datasetId")]
        //[IsRetrievable(true)]
        public Guid DatasetId;

        ///// <summary>
        ///// The unique identifier for the dataset
        ///// </summary>
        //[JsonProperty("id")]
        //public Guid Id;

        /// <summary>
        /// The name of the dataset
        /// </summary>
        [JsonProperty("name")]
        public string Name;
    }
}
