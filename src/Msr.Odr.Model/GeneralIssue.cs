// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.ComponentModel.DataAnnotations;
using Msr.Odr.Model.Configuration;

namespace Msr.Odr.Model
{
	/// <summary>
	/// Represents a document containing a feedback/general issue report.
	/// </summary>
	public class GeneralIssue
	{
	    /// <summary>
	    /// The document id
	    /// </summary>
	    /// <value>The identifier.</value>
	    public Guid Id { get; set; }

		/// <summary>
        /// Gets or sets the title of the issue
        /// </summary>
        /// <value>The name.</value>
        [StringLength(Constraints.LongName, MinimumLength = 1)]
		public string Name { get; set; }

	    /// <summary>
	    /// The fixed dataset Id used as the partition key for general issues.
	    /// </summary>
	    /// <value>The id.</value>
	    public Guid DatasetId { get; set; } = WellKnownIds.GeneralIssueDatasetId;

        /// <summary>
        /// Gets or sets the description of the issue.
        /// </summary>
        /// <value>The description.</value>
        [StringLength(Constraints.MedDescription)]
        [Required]
		public string Description { get; set; }

        /// <summary>
        /// Gets or sets the name of the contact creating the issue.
        /// </summary>
        /// <value>The name.</value>
        [StringLength(Constraints.ContactNameLength, MinimumLength = 1)]
        public string ContactName { get; set; }

        /// <summary>
        /// Gets or sets the contact information
        /// </summary>
        /// <value>The name.</value>
        [StringLength(Constraints.ContactInfoLength, MinimumLength = 1)]
        public string ContactInfo { get; set; }
    }
}
