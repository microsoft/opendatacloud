using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Msr.Odr.Model.Datasets;

namespace Msr.Odr.Model
{
	/// <summary>
	/// Represents a collection of files in a published dataset.
	/// </summary>
	public class Dataset
	{
        public Dataset()
	    {
	        Tags = new List<string>();
            FileTypes = new List<string>();
            DatasetOwners = new List<DatasetOwner>();
	    }

        /// <summary>
        /// Gets or sets the identifier for the dataset.
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
        /// Gets or sets the URL to the project page.
        /// </summary>
        /// <value>The Url.</value>
        public string ProjectUrl { get; set; }

		/// <summary>
		/// Gets or sets the file types that exist within the dataset.
		/// </summary>
		/// <value>The file types.</value>
		public ICollection<string> FileTypes { get; set; }

        /// <summary>
        /// Gets or sets the tags.
        /// </summary>
        /// <value>The tags.</value>
        [MaxLength(10)]
		public ICollection<string> Tags { get; set; }

        /// <summary>
        /// Gets or sets the creation timestamp.
        /// </summary>
        /// <value>The created.</value>
        public DateTime? Created { get; set; }

        /// <summary>
        /// Gets or sets the number of files contained in the dataset
        /// </summary>
        /// <value>The file count.</value>
		public int FileCount { get; set; }

        /// <summary>
        /// Gets or sets the last modified timestamp.
        /// </summary>
        /// <value>The modified.</value>
        public DateTime? Modified { get; set; }

        /// <summary>
        /// Gets or sets the name for domain
        /// </summary>
        /// <value>The domain name.</value>
        public string Domain { get; set; }

        /// <summary>
        /// Gets or sets the id for the domain
        /// </summary>
        /// <value>The domain name.</value>
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
	    /// The version of the dataset.
	    /// </summary>
	    public string Version { get; set; }

	    /// <summary>
	    /// The version of the dataset.
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
        /// Gets or sets the name of the creator of the dataset.
        /// </summary>
        /// <value>The creator's name.</value>
        public string CreatedByUserName { get; set; }

        /// <summary>
        /// Gets or sets the email of the creator of the dataset.
        /// </summary>
        /// <value>The creator's email.</value>
        public string CreatedByUserEmail { get; set; }

        /// <summary> 
        /// Gets or sets the name of the modifier of the dataset.
        /// </summary>
        /// <value>The modifier's name.</value>
        public string ModifiedByUserName { get; set; }

        /// <summary>
        /// Gets or sets the name of the modifier of the dataset.
        /// </summary>
        /// <value>The modifier's email.</value>
        public string ModifiedByUserEmail { get; set; }
        
        /// <summary>
        ///  True if the dataset can be directly downloaded
        /// </summary>
        public bool? IsDownloadAllowed { get; set; }

	    /// <summary>
	    ///  True if there are compressed versions available
	    /// </summary>
        public bool? IsCompressedAvailable { get; set; }

        /// <summary>
        ///  True if the dataset is featured
        /// </summary>
        public bool? IsFeatured { get; set; }

        /// <summary>
        ///     gets or sets the digital object identifier
        /// </summary>
        public string DigitalObjectIdentifier { get; set; }

        /// <summary>
        /// Owners of dataset, if applicable.
        /// </summary>
        public ICollection<DatasetOwner> DatasetOwners { get; set; }
	}
}
