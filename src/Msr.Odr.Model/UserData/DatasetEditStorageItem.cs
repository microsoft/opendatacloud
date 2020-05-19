using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Search.Models;
using Msr.Odr.Model.Configuration;
using Msr.Odr.Model.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Msr.Odr.Model.UserData
{
    /// <summary>
    /// Represents a dataset owner's edit of a dataset.
    /// </summary>
    [SerializePropertyNamesAsCamelCase]
    [JsonObject("datasetEdit")]
    public class DatasetEditStorageItem : DatasetNominationStorageItem
    {
        /// <summary>
        /// The data type for the dataset edit item.
        /// </summary>
        [JsonProperty("dataType")]
        [JsonConverter(typeof(StringEnumConverter))]
        public override UserDataTypes DataType => UserDataTypes.DatasetEdit;

        /// <summary>
        /// The current status of the dataset edit.
        /// </summary>
        [JsonProperty("editStatus")]
        [JsonConverter(typeof(StringEnumConverter))]
        public DatasetEditStatus EditStatus { get; set; }

        /// <summary>
        /// The name of the account the content edit is stored in.
        /// </summary>
        [JsonProperty("contentEditAccount", NullValueHandling = NullValueHandling.Ignore)]
        public string ContentEditAccount { get; set; }

        /// <summary>
        /// The name of the container the content edit is stored in.
        /// </summary>
        [JsonProperty("contentEditContainer", NullValueHandling = NullValueHandling.Ignore)]
        public string ContentEditContainer { get; set; }

        /// <summary>
        /// The name of the account the original content is stored in.
        /// </summary>
        [JsonProperty("originalStorageAccount", NullValueHandling = NullValueHandling.Ignore)]
        public string OriginalStorageAccount { get; set; }

        /// <summary>
        /// The name of the container the original content is stored in.
        /// </summary>
        [JsonProperty("originalStorageContainer", NullValueHandling = NullValueHandling.Ignore)]
        public string OriginalStorageContainer { get; set; }
    }
}
