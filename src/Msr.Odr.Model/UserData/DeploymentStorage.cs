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
    [JsonObject("deployment")]
    public class DeploymentStorage
    {
        /// <summary>
        /// Gets or sets the identifier for the dataset that will be deployed.
        /// </summary>
        /// <value>The dataset identifier.</value>
        [JsonProperty("datasetId")]
        public Guid DatasetId { get; set; }

        /// <summary>
        /// The unique identifier for the issue document
        /// </summary>
        [JsonProperty("id")]
        public Guid Id;

        /// <summary>
        /// The identifier for the type of deployment
        /// </summary>
        [JsonProperty("deploymentId")]
        public string DeploymentId;

        /// <summary>
        /// The Uri for the Dataset storage (including SAS token).
        /// </summary>
        [JsonProperty("storageUri")]
        public string StorageUri;

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
        } = UserDataTypes.Deployment;

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