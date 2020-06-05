// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Msr.Odr.Model
{
	/// <summary>
	/// Criteria for searching for nominations
	/// </summary>
	public class NominationSearch
    {
		/// <summary>
		/// Gets or sets the search terms.
		/// </summary>
		/// <value>The terms.</value>
		[StringLength(maximumLength: Constraints.TermLength, MinimumLength = 0)]
        public string Terms { get; set; }

		/// <summary>
		/// Gets or sets the facets for the search by facet name and allowed values.
		/// Facets include licenses, tags, etc.
		/// </summary>
		/// <value>The facets.</value>
		[MaxLength(3)]
		public IDictionary<string, IEnumerable<string>> Facets
		{
			get;
			set;
		} = new Dictionary<string, IEnumerable<string>>();

		/// <summary>
		/// Gets or sets the sort order.
		/// </summary>
		/// <value>The sort order.</value>
		public SortOrder SortOrder { get; set; }

		/// <summary>
		/// Gets or sets the page in the search results to be returned.
		/// </summary>
		/// <value>The page.</value>
		[Range(0, Int32.MaxValue)]
		public int Page { get; set; }

        /// <summary>
        /// A list of status values to include
        /// </summary>
        public ICollection<string> SelectedStatus { get; set; }
    }
}