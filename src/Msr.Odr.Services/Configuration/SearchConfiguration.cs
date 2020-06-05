// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using FluentValidation;

namespace Msr.Odr.Services.Configuration
{
    /// <summary>
    /// Options and settings for Azure Search
    /// </summary>
    public class SearchConfiguration
    {
        /// <summary>
        /// Gets or sets the search account.
        /// </summary>
        public string Account { get; set; }

        /// <summary>
        /// Gets or sets the Azure Search Query key.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the index used for searching the dataset.
        /// </summary>
        public string DatasetIndex { get; set; }

        /// <summary>
        /// Gets or sets the index used for searching the files.
        /// </summary>
        public string FileIndex { get; set; }

        /// <summary>
        /// The index for Dataset Nominations.
        /// </summary>
        public string NominationsIndex => "nomination-ix";

        public static void Validate(SearchConfiguration configuration)
        {
            var validator = new SearchConfigurationValidator();
            validator.ValidateAndThrow(configuration);
        }
    }
}
