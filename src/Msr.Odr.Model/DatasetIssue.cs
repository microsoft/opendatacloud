// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.ComponentModel.DataAnnotations;

namespace Msr.Odr.Model
{
	/// <summary>
	/// Represents a document containing a dataset problem report.
	/// </summary>
	public class DatasetIssue
	{
	    /// <summary>
	    /// The document id
	    /// </summary>
	    /// <value>The identifier.</value>
	    public Guid Id
	    {
	        get;
	        set;
	    }

		/// <summary>
        /// Gets or sets the name of the dataset.
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
        /// Gets or sets the unique id of the dataset.
        /// </summary>
        /// <value>The name.</value>
        public Guid DatasetId
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the description of the issue.
        /// </summary>
        /// <value>The description.</value>
        [StringLength(Constraints.MedDescription)]
        [Required]
		public string Description
		{
			get;
			set;
		}

        /// <summary>
        /// Gets or sets the name of the contact creating the issue.
        /// </summary>
        /// <value>The name.</value>
        [StringLength(Constraints.ContactNameLength, MinimumLength = 1)]
        public string ContactName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the contact information
        /// </summary>
        /// <value>The name.</value>
        [StringLength(Constraints.ContactInfoLength, MinimumLength = 1)]
        public string ContactInfo
        {
            get;
            set;
        }
    }
}
