// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Msr.Odr.Model.Datasets;

namespace Msr.Odr.WebApi.ViewModels
{
    public class DatasetViewModel
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
        /// Gets or sets the file types that exist within the dataset.
        /// </summary>
        public ICollection<string> FileTypes { get; set; }

        /// <summary>
        /// Gets or sets the tags.
        /// </summary>
        public ICollection<string> Tags { get; set; }

        /// <summary>
        /// Gets or sets the creation timestamp.
        /// </summary>
        public DateTime? Created { get; set; }

        /// <summary>
        /// Gets or sets the last modified timestamp.
        /// </summary>
        public DateTime? Modified { get; set; }

        /// <summary>
        /// Gets or sets the number of files contained in the dataset
        /// </summary>
		public int FileCount { get; set; }

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
        /// Total size of the uncompressed files that make up the dataset
        /// </summary>
	    public long Size { get; set; }

        /// <summary>
        /// The size of the compressed tar.gz file
        /// </summary>
	    public long GzipFileSize { get; set; }

        /// <summary>
        /// The size of the compressed zip file
        /// </summary>
	    public long ZipFileSize { get; set; }

        /// <summary>
        ///  True if the dataset can be directly downloaded
        /// </summary>
        public bool IsDownloadAllowed { get; set; }

        /// <summary>
        ///  True if there are compressed versions available
        /// </summary>
        public bool IsCompressedAvailable { get; set; }

        /// <summary>
        /// True if the dataset is featured
        /// </summary>
        public bool IsFeatured { get; set; }

        /// <summary>
        /// Gets or sets the digital object identifier
        /// </summary>
        public string DigitalObjectIdentifier { get; set; }

        /// <summary>
        /// The relative URI for the license content.
        /// </summary>
        public Uri LicenseContentUri => LicenseId.HasValue ? new Uri($"/licenses/{LicenseId.Value}/view", UriKind.Relative) : null;

        /// <summary>
        /// Is the current user an "owner" of this dataset?
        /// </summary>
        public bool IsCurrentUserOwner { get; set; }
    }
}
