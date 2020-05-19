using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Msr.Odr.Model.Converters;
using Msr.Odr.Model.UserData;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Msr.Odr.Model
{
    /// <summary>
    /// Represents a nomination for a dataset.
    /// </summary>
    public class DatasetNomination : IOtherLicenseDetails
    {
        public DatasetNomination()
        {
            Tags = new List<string>();
        }

        /// <summary>
        /// The datataset id
        /// </summary>
        /// <value>The identifier.</value>
        public Guid DatasetId { get; set; }

        /// <summary>
        /// The document id
        /// </summary>
        /// <value>The identifier.</value>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the dataset.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the dataset.
        /// </summary>
        /// <value>The description.</value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the URL to the dataset.
        /// </summary>
        /// <value>The Url.</value>
        public string DatasetUrl { get; set; }

        /// <summary>
        /// Gets or sets the URL to the project page
        /// </summary>
        /// <value>The Url.</value>
        public string ProjectUrl { get; set; }

        /// <summary>
        /// Gets or sets the tags.
        /// </summary>
        /// <value>The tags.</value>
        public ICollection<string> Tags { get; set; }

        /// <summary>
        /// Gets or sets the date the dataset was published.
        /// </summary>
        /// <value>The created.</value>
        [DataType(DataType.Date)]
        public DateTime? Published { get; set; }

        /// <summary>
        /// The creation date for the nomination
        /// </summary>
        public DateTime? Created { get; set; }

        /// <summary>
        /// The creation date for the nomination
        /// </summary>
        public DateTime? Modified { get; set; }

        /// <summary>
        /// Gets or sets the version of the dataset.
        /// </summary>
        /// <value>The version.</value>
        public string Version { get; set; }

        /// <summary>
        /// Gets or sets the string for domain name for the dataset
        /// </summary>
        /// <value>The domain name.</value>
        public string Domain { get; set; }

        /// <summary>
        /// Gets or sets the string id for domain name for the dataset
        /// </summary>
        /// <value>The domain id.</value>
        public string DomainId { get; set; }

        /// <summary>
        /// Gets or sets the license id
        /// </summary>
        /// <value>The license id.</value>
        public Guid? LicenseId { get; set; }

        /// <summary>
        /// Gets or sets the license name
        /// </summary>
        /// <value>The license name.</value>
        public string LicenseName { get; set; }

        /// <summary>
        /// Gets or sets the contact name
        /// </summary>
        /// <value>The name.</value>
        public string ContactName { get; set; }

        /// <summary>
        /// Gets or sets the contact information
        /// </summary>
        /// <value>The name.</value>
        public string ContactInfo { get; set; }

        /// <summary>
        /// Gets or sets the id of the creator of the nomination.
        /// </summary>
        public string CreatedByUserId { get; set; }

        /// <summary>
        /// Gets or sets the name of the creator of the nomination.
        /// </summary>
        /// <value>The creator's name.</value>
        public string CreatedByUserName { get; set; }

        /// <summary>
        /// Gets or sets the email of the creator of the nomination.
        /// </summary>
        /// <value>The creator's email.</value>
        public string CreatedByUserEmail { get; set; }

        /// <summary> 
        /// Gets or sets the name of the modifier of the nomination.
        /// </summary>
        /// <value>The modifier's name.</value>
        public string ModifiedByUserName { get; set; }

        /// <summary>
        /// Gets or sets the name of the modifier of the nomination.
        /// </summary>
        /// <value>The modifier's email.</value>
        public string ModifiedByUserEmail { get; set; }

        /// <summary>
        /// Gets or sets whether downloads are allowed 
        /// </summary>
        public bool IsDownloadAllowed { get; set; }

        /// <summary>
        ///     gets or sets the digital object identifier
        /// </summary>
        public string DigitalObjectIdentifier { get; set; }

        /// <summary>
        ///     gets or sets the License Content when LicenseType is HtmlText
        /// </summary>
        public string OtherLicenseContentHtml { get; set; }

        /// <summary>
        ///     gets or sets the License File when LicenseType is InputFile
        /// </summary>
        public IFormFile OtherLicenseFile { get; set; }

        /// <summary>
        ///     gets or sets the License File Content (as base64 encoded) when LicenseType is InputFile
        /// </summary>
        public string OtherLicenseFileContent { get; set; }

        /// <summary>
        ///     gets or sets the License File Content Type when LicenseType is InputFile
        /// </summary>
        public string OtherLicenseFileContentType { get; set; }

        /// <summary>
        ///     gets or sets the License File Name when LicenseType is InputFile
        /// </summary>
        public string OtherLicenseFileName { get; set; }

        /// <summary>
        ///     gets or sets the Other License Additional Info URL when LicenseType is InputFile or HtmlText
        /// </summary>
        public string OtherLicenseAdditionalInfoUrl { get; set; }

        /// <summary>
        ///     gets or sets the License Name when LicenseType is InputFile or HtmlText
        /// </summary>
        public string OtherLicenseName { get; set; }

        /// <summary>
        /// The approval status for the nominated dataset
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public NominationStatus NominationStatus { get; set; } = NominationStatus.PendingApproval;

        [JsonConverter(typeof(NominationLicenseTypeConverter))]
        public NominationLicenseType NominationLicenseType { get; set; } = NominationLicenseType.Unknown;
        
        public Uri LicenseContentUri =>
            LicenseId.HasValue ? new Uri($"/licenses/{LicenseId.Value}/content", UriKind.Relative) : null;
    }
}
