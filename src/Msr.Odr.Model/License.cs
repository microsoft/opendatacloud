using System;
using System.ComponentModel.DataAnnotations;
using Msr.Odr.Model.Datasets;

namespace Msr.Odr.Model
{
    /// <summary>
    /// The license associated with a dataset
    /// </summary>
    public class License
    {
        public License()
        {

        }

        public License(LicenseStorageItem source)
        {
            Id = source.Id;
            Name = source.Name;
            IsStandard = source.IsStandard;
            ContentUri = source.IsFileBased ?
                        new Uri($"/licenses/{source.Id}/file", UriKind.Relative) :
                        new Uri($"/licenses/{source.Id}/content", UriKind.Relative);
            FileName = source.FileName;
            FileContentType = source.FileContentType;
            IsFileBased = source.IsFileBased;
            FileContent = source.FileContent;
        }

        /// <summary>
        /// Gets or sets the unique identifier for the license.
        /// </summary>
        /// <value>The identifier.</value>
        public Guid Id
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the name of the license.
        /// </summary>
        /// <value>The name.</value>
        [StringLength(Constraints.LongName, MinimumLength = 1)]
        [Required]
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the URI for retrieving the license contents..
        /// </summary>
        /// <value>The content URI.</value>
        [Url]
        public Uri ContentUri
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the flag for this being a Standard License (appears for all nominations)
        /// </summary>
        /// <value>The flag indicating if this is a Standard License.</value>
        public bool IsStandard
        {
            get;
            set;
        }

        /// <summary>
        /// The flag indicating the license is file based
        /// </summary>
        public bool IsFileBased
        {
            get;
            set;
        }

        /// <summary>
        /// The file type of the file when IsFileBased == true
        /// </summary>
        public string FileContentType
        {
            get;
            set;
        }

        /// <summary>
        /// The file name of the file when IsFileBased == true
        /// </summary>
        public string FileName
        {
            get;
            set;
        }

        /// <summary>
        /// License File Content (as base64 encoded) string when IsFileBased == true
        /// </summary>
        public string FileContent
        {
            get;
            set;
        }
    }
}
