using System;
using System.ComponentModel.DataAnnotations;

namespace Msr.Odr.Model
{
	/// <summary>
	/// A result from a dataset name search.
	/// </summary>
	public class DatasetNameSearchResult
    {

		/// <summary>
		/// Gets or sets the identifier for the dataset.
		/// </summary>
		/// <value>The identifier.</value>
		public Guid Id
		{
			get;
			set;
		}

		/// <summary>
		/// The name of the dataset.
		/// </summary>
		/// <value>The name.</value>
		[Required]
		public string Name
		{
			get;
			set;
		}
	}
}
