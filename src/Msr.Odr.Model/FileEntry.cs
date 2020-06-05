// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Msr.Odr.Model
{
    /// <summary>
    /// Represents a file or folder within a dataset
    /// </summary>
    public class FileEntry
    {
        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        /// <value>The identifier.</value>
		[Key]
        public Guid Id
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the name of the file.
        /// </summary>
        /// <value>The name.</value>
        [StringLength(Constraints.MaxFileNameLength, MinimumLength = 1)]
		[Required]
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the type of file entry: file or folder.
        /// </summary>
        /// <value>The type of the entry.</value>
        public FileEntryType EntryType
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the parent folder name.
        /// </summary>
        /// <value>The parent folder.</value>
        public string ParentFolder
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the content preview URL.
        /// </summary>
        /// <value>The preview URL.</value>
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Uri PreviewUrl
        {
            get;
            set;
        }

        public long? Length { get; set; }
    }
}
