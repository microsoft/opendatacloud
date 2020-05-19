using System.Collections.Generic;

namespace Msr.Odr.Model
{
	/// <summary>
	/// Represents a paged data result
	/// </summary>
	/// <typeparam name="T">The data type returned in the search</typeparam>
	public class PagedResult<T>
    {
        /// <summary>
        /// Gets or sets the total number of results available.
        /// </summary>
        /// <value>The page count.</value>
        public long RecordCount
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the total number of pages of results available.
        /// </summary>
        /// <value>The page count.</value>
        public int PageCount
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the data returned by the search.
		/// </summary>
		/// <value>The value.</value>
		public IEnumerable<T> Value
		{
			get;
			set;
		}
    }
}
