// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Msr.Odr.Model.UserData
{
    /// <summary>
    /// Represents an general issue (feedback)
    /// </summary>
    [JsonObject("generalIssue")]
    public class GeneralIssueStorageItem
    {
        /// <summary>
        /// The unique identifier for the issue document
        /// </summary>
        [JsonProperty("id")]
        public Guid Id;

        /// <summary>
        /// The dataset identifier (fixed value for issue documents).
        /// </summary>
        [JsonProperty("datasetId")]
        public Guid DatasetId;

        /// <summary>
        /// The title of the issue
        /// </summary>
        [JsonProperty("name")]
        public string Name;

        /// <summary>
        /// The description for the dataset issue.
        /// </summary>
        [JsonProperty("description")]
        public string Description;

        /// <summary>
        /// The data type for the issue document.
        /// </summary>
        [JsonProperty("dataType")]
        [JsonConverter(typeof(StringEnumConverter))]
        public UserDataTypes DataType
        {
            get;
            private set;
        } = UserDataTypes.GeneralIssue;

        /// <summary>
        /// The contact name of the user submitting the issue.
        /// </summary>
        [JsonProperty("contactName")]
        public string ContactName;

        /// <summary>
        /// The contact information for the user submitting the issue.
        /// </summary>
        [JsonProperty("contactInfo")]
        public string ContactInfo;

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
