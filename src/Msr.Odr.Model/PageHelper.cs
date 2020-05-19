using System;

namespace Msr.Odr.Model
{
	/// <summary>
	/// Temporary class for helping to manage paging operations.
	/// </summary>
	public static class PageHelper
    {
		/// <summary>
		/// The page size
		/// </summary>
		public const int PageSize = 5;

        /// <summary>
        /// Calculates the number of pages based on a count of results
        /// </summary>
        /// <param name="count">The results count.</param>
        /// <returns>The total number of pages.</returns>
        public static int CalculateNumberOfPages(long? count)
        {
			if (count == 0 || count == null)
            {
                return 0;
            }

            var pages = (int)Math.Ceiling((count ?? 0L) / (double)PageHelper.PageSize) ;
            return pages;
        }
    }
}
