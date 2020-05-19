using System;
using FluentValidation;

namespace Msr.Odr.Services.Configuration
{
    /// <summary>
    /// THe CosmosDB document storage options
    /// </summary>
    public class CosmosConfiguration
    {
        /// <summary>
        /// Gets or sets the CosmosDB account.
        /// </summary>
        public string Account { get; set; }

        /// <summary>
        /// Gets or sets the Azure CosmosDB key.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the search database.
        /// </summary>
        public string Database { get; set; }

        /// <summary>
        /// Gets or sets the dataset collection.
        /// </summary>
        public string DatasetCollection { get; set; }

        /// <summary>
        /// Gets or sets the user data collection.
        /// </summary>
        public string UserDataCollection { get; set; }

        /// <summary>
        /// Gets the URI for accessing the document storage account.
        /// </summary>
        public Uri Uri => new Uri($"https://{Account}.documents.azure.com");

        public static void Validate(CosmosConfiguration configuration)
        {
            var validator = new CosmosConfigurationValidator();
            validator.ValidateAndThrow(configuration);
        }
    }
}
