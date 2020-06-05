// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Msr.Odr.Model.Converters;
using Msr.Odr.Model.Datasets;
using Msr.Odr.Model.UserData;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Msr.Odr.WebApi.ViewModels
{
    public class DatasetEditViewModel
    {
        /// <summary>
        /// Gets or sets the identifier for the dataset.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the dataset.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the dataset.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the URL to the dataset.
        /// </summary>
        public string DatasetUrl { get; set; }

        /// <summary>
        /// Gets or sets the URL to the project page.
        /// </summary>
        public string ProjectUrl { get; set; }

        /// <summary>
        /// Gets or sets the tags.
        /// </summary>
        public ICollection<string> Tags { get; set; }

        /// <summary>
        /// Gets or sets the name for domain
        /// </summary>
        public string Domain { get; set; }

        /// <summary>
        /// Gets or sets the id for the domain
        /// </summary>
        public string DomainId { get; set; }

        /// <summary>
        /// Gets or sets the license id
        /// </summary>
        public Guid? LicenseId { get; set; }

        /// <summary>
        /// Gets or sets the license name
        /// </summary>
        public string LicenseName { get; set; }

        /// <summary>
        /// The version of the dataset.
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Date the dataset was published.
        /// </summary>
        public DateTime? Published { get; set; }

        /// <summary>
        ///  True if the dataset can be directly downloaded
        /// </summary>
        public bool IsDownloadAllowed { get; set; }

        /// <summary>
        /// Gets or sets the digital object identifier
        /// </summary>
        public string DigitalObjectIdentifier { get; set; }

        /// <summary>
        /// The current status of the dataset edit.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public DatasetEditStatus EditStatus { get; set; }

        /// <summary>
        /// The relative URI for the license content.
        /// </summary>
        public Uri LicenseContentUri => LicenseId.HasValue ? new Uri($"/licenses/{LicenseId.Value}/content", UriKind.Relative) : null;

        /// <summary>
        /// The type of license for the nominated dataset
        /// </summary>
        [JsonProperty("licenseType")]
        [JsonConverter(typeof(NominationLicenseTypeConverter))]
        public NominationLicenseType LicenseType { get; set; } = NominationLicenseType.Unknown;

        /// <summary>
        /// The License Content when LicenseType is TextHtml
        /// </summary>
        [JsonProperty("otherLicenseContentHtml")]
        public string OtherLicenseContentHtml { get; set; }

        /// <summary>
        /// The License File Content (as base64 encoded) when LicenseType is InputFile
        /// </summary>
        [JsonProperty("otherLicenseFileContent")]
        public string OtherLicenseFileContent { get; set; }

        /// <summary>
        /// The License File Content Type when LicenseType is InputFile
        /// </summary>
        [JsonProperty("otherLicenseFileContentType")]
        public string OtherLicenseFileContentType { get; set; }

        /// <summary>
        /// The Additional Info URL when LicenseType is TextHtml or InputFile
        /// </summary>
        [JsonProperty("otherLicenseAdditionalInfoUrl")]
        public string OtherLicenseAdditionalInfoUrl { get; set; }

        /// <summary>
        /// The License Name when LicenseType is TextHtml or InputFile
        /// </summary>
        [JsonProperty("otherLicenseName")]
        public string OtherLicenseName { get; set; }

        /// <summary>
        /// The License File Name when LicenseType is InputFile
        /// </summary>
        [JsonProperty("otherLicenseFileName")]
        public string OtherLicenseFileName { get; set; }

        /// <summary>
        /// The License File when LicenseType is InputFile (used only for HTTP POST)
        /// </summary>
        public IFormFile OtherLicenseFile { get; set; }

        /// <summary>
        /// The name of the dataset owner.
        /// </summary>
        [JsonProperty("contactName")]
        public string ContactName { get; set; }

        /// <summary>
        /// The email address of the dataset owner.
        /// </summary>
        [JsonProperty("contactInfo")]
        public string ContactInfo { get; set; }
    }
}
