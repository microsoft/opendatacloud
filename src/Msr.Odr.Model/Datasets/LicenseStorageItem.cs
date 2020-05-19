using System;
using Msr.Odr.Model.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Msr.Odr.Model.Datasets
{
    /// <summary>
    /// A stored license definition
    /// </summary>
    [JsonObject("license")]
    public class LicenseStorageItem
    {
        /// <summary>
        /// The dataset identifier always associated with a license.
        /// </summary>
        public static Guid LicenseDatasetId => WellKnownIds.LicenseDatasetId;

        /// <summary>
        /// The dataset identifier
        /// </summary>
        [JsonProperty("datasetId")]
        public Guid DatasetId => LicenseDatasetId;

        /// <summary>
        /// The license identifier
        /// </summary>
        [JsonProperty("id")]
        public Guid Id;

        /// <summary>
        /// The license name
        /// </summary>
        [JsonProperty("name")]
        public string Name;

        /// <summary>
        /// The flag indicating if this is a Standard License
        /// </summary>
        [JsonProperty("isStandard")]
        public bool IsStandard;

        /// <summary>
        /// The flag indicating the license is file based
        /// </summary>
        [JsonProperty("isFileBased")]
        public bool IsFileBased;

        /// <summary>
        /// The file type of the file when IsFileBased == true
        /// </summary>
        [JsonProperty("fileContentType")]
        public string FileContentType;

        /// <summary>
        /// The file name of the file when IsFileBased == true
        /// </summary>
        [JsonProperty("fileName")]
        public string FileName;

        /// <summary>
        /// License File Content (as base64 encoded) string when IsFileBased == true
        /// </summary>
        [JsonProperty("fileContent")]
        public string FileContent
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the type of the data.
        /// </summary>
        /// <value>The type of the data.</value>
        [JsonProperty("dataType")]
        [JsonConverter(typeof(StringEnumConverter))]
        public StorageDataType DataType
        {
            get;
            private set;
        } = StorageDataType.License;
    };
}
