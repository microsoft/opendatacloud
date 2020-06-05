// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Azure.Search.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Msr.Odr.Model.UserData
{
    /// <summary>
    /// Represents a storage record for an accepted license, mapping
    /// a dataset, license ID, and user ID.
    /// </summary>
    [SerializePropertyNamesAsCamelCase]
    [JsonObject("acceptedLicense")]
    public class AcceptedLicenseStorage
    {
        /// <summary>
        /// Gets or sets the dataset identifier.
        /// </summary>
        /// <value>The dataset identifier.</value>
        [JsonProperty("datasetId")]
        public Guid DatasetId { get; set; }

        /// <summary>
        /// Gets or sets the primary identifier. This is the same as the user identifier.
        /// </summary>
        /// <value>The primary identifier.</value>
        [JsonProperty("id")]
        public Guid Id
        {
            get
            {
                return this.UserId;
            }

            set
            {
            }
        }

        /// <summary>
        /// Gets or sets the license identifier.
        /// </summary>
        /// <value>The license identifier.</value>
        [JsonProperty("licenseId")]
        public Guid LicenseId { get; set; }

        /// <summary>
        /// Gets or sets the domain identifier.
        /// </summary>
        /// <value>The domain identifier.</value>
        [JsonProperty("domainId")]
        public string DomainId { get; set; }

        /// <summary>
        /// Gets or sets the reason the user wants the dataset.
        /// </summary>
        /// <value>The user's reason.</value>
        [JsonProperty("reason")]
        public string ReasonForUse { get; set; }

        /// <summary>
        /// Gets or sets the type of the data.
        /// </summary>
        /// <value>The type of the data.</value>
        [JsonProperty("dataType")]
        [JsonConverter(typeof(StringEnumConverter))]
        public UserDataTypes DataType
        {
            get;
            private set;
        } = UserDataTypes.AcceptedLicense;

        /// <summary>
        /// Gets or sets the user identifier.
        /// </summary>
        /// <value>The user identifier.</value>
        [JsonProperty("userId")]
        public Guid UserId { get; set; }

        /// <summary>
        /// Gets or sets the name of the user.
        /// </summary>
        /// <value>The user's name.</value>
        [JsonProperty("userName")]
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets the email of the user.
        /// </summary>
        /// <value>The user's email.</value>
        [JsonProperty("userEmail")]
        public string UserEmail { get; set; }
    }
}