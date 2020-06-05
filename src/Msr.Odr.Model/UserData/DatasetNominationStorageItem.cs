// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

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
    /// Represents a user's nomination of a dataset.
    /// </summary>
    [SerializePropertyNamesAsCamelCase]
    [JsonObject("dataset")]
    public class DatasetNominationStorageItem : IOtherLicenseDetails
    {
        public DatasetNominationStorageItem()
        {
            Tags = new List<string>();
        }

        /// <summary>
        /// The unique identifier for the nomination document.
        /// </summary>
        [JsonProperty("id")]
        public Guid Id { get; set; }

        /// <summary>
        /// The dataset identifier (fixed value for nomination documents).
        /// </summary>
        [JsonProperty("datasetId")]
        public Guid DatasetId { get; set; }

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
        /// The string of the domain the dataset belongs to.
        /// </summary>
        [JsonProperty("domain")]
        public string Domain { get; set; }

        /// <summary>
        /// The string id of the domain the dataset belongs to.
        /// </summary>
        [JsonProperty("domainId")]
        public string DomainId { get; set; }

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
        /// The published date for the dataset
        /// </summary>
        [JsonProperty("published")]
        public DateTime? Published { get; set; }

        /// <summary>
        /// The creation date for the nomination
        /// </summary>
        [JsonProperty("created")]
        public DateTime? Created { get; set; }

        /// <summary>
        /// The modified date for the nomination
        /// </summary>
        [JsonProperty("modified")]
        public DateTime? Modified { get; set; }

        /// <summary>
        /// The name of the license associated with the dataset.
        /// </summary>
        [JsonProperty("license")]
        public string License { get; set; }

        /// <summary>
        /// The license identifier
        /// </summary>
        [JsonProperty("licenseId")]
        public Guid? LicenseId { get; set; }

        /// <summary>
        /// The data type for the dataset nomination
        /// </summary>
        [JsonProperty("dataType")]
        [JsonConverter(typeof(StringEnumConverter))]
        public virtual UserDataTypes DataType => UserDataTypes.DatasetNomination;

        /// <summary>
        /// The tags
        /// </summary>
        [JsonProperty("tags")]
        public ICollection<string> Tags { get; set; }

        /// <summary>
        /// The contact name for the user submitting the nomination.
        /// </summary>
        [JsonProperty("contactName")]
        public string ContactName { get; set; }

        /// <summary>
        /// The contact information for the user submitting the nominstation.
        /// </summary>
        [JsonProperty("contactInfo")]
        public string ContactInfo { get; set; }

        /// <summary>
        /// Gets or sets the user identifier of the creator of the nomination
        /// </summary>
        [JsonProperty("createdByUserId")]
        public string CreatedByUserId { get; set; }

        /// <summary>
        /// Gets or sets the name of the creator of the nomination.
        /// </summary>
        /// <value>The creator's name.</value>
        [JsonProperty("createdByUserName")]
        public string CreatedByUserName { get; set; }

        /// <summary>
        /// Gets or sets the email of the creator of the nomination.
        /// </summary>
        /// <value>The creator's email.</value>
        [JsonProperty("createdByUserEmail")]
        public string CreatedByUserEmail { get; set; }

        /// <summary> 
        /// Gets or sets the name of the modifier of the nomination.
        /// </summary>
        /// <value>The modifier's name.</value>
        [JsonProperty("modifiedByUserName")]
        public string ModifiedByUserName { get; set; }

        /// <summary>
        /// Gets or sets the name of the modifier of the nomination.
        /// </summary>
        /// <value>The modifier's email.</value>
        [JsonProperty("modifiedByUserEmail")]
        public string ModifiedByUserEmail { get; set; }

        /// <summary>
        /// Gets or sets whether direct downloads are allowed.
        /// </summary>
        [JsonProperty("isDownloadAllowed")]
        public bool IsDownloadAllowed { get; set; }

        /// <summary>
        /// The approval status for the nominated dataset
        /// </summary>
        [JsonProperty("nominationStatus")]
        [JsonConverter(typeof(NominationStatusConverter))]
        public NominationStatus NominationStatus { get; set; } = NominationStatus.PendingApproval;

        /// <summary>
        /// The type of license for the nominated dataset
        /// </summary>
        [JsonProperty("licenseType")]
        [JsonConverter(typeof(NominationLicenseTypeConverter))]
        public NominationLicenseType NominationLicenseType { get; set; } = NominationLicenseType.Unknown;

        ///// <summary>
        ///// An alias for the "_ts" time stamp property.
        ///// </summary>
        //[JsonProperty("timeStamp")]
        //public long? TimeStamp { get; set; }

        /// <summary>
        ///     gets or sets the digital object identifier
        /// </summary>
        [JsonProperty("digitalObjectIdentifier")]
        public string DigitalObjectIdentifier { get; set; }

        /// <summary>
        ///     gets or sets the License Content when LicenseType is TextHtml
        /// </summary>
        [JsonProperty("otherLicenseContentHtml")]
        public string OtherLicenseContentHtml { get; set; }

        /// <summary>
        ///     gets or sets the License File Content (as base64 encoded) when LicenseType is InputFile
        /// </summary>
        [JsonProperty("otherLicenseFileContent")]
        public string OtherLicenseFileContent { get; set; }

        /// <summary>
        ///     gets or sets the License File Content Type when LicenseType is InputFile
        /// </summary>
        [JsonProperty("otherLicenseFileContentType")]
        public string OtherLicenseFileContentType { get; set; }

        /// <summary>
        ///     gets or sets the Additional Info URL when LicenseType is TextHtml or InputFile
        /// </summary>
        [JsonProperty("otherLicenseAdditionalInfoUrl")]
        public string OtherLicenseAdditionalInfoUrl { get; set; }

        /// <summary>
        ///     gets or sets the License Name when LicenseType is TextHtml or InputFile
        /// </summary>
        [JsonProperty("otherLicenseName")]
        public string OtherLicenseName { get; set; }

        /// <summary>
        ///     gets or sets the License File Name when LicenseType is InputFile
        /// </summary>
        [JsonProperty("otherLicenseFileName")]
        public string OtherLicenseFileName { get; set; }
    }
}
