using System;

namespace Msr.Odr.Model
{
    /// <summary>
    /// Specifies the sort order to use for  search results
    /// </summary>
    public enum SortOrder
    {
        /// <summary>
        /// The default sort order (name)
        /// </summary>
        Default = 0,

        /// <summary>
        /// Sorts by the name
        /// </summary>
        Name = 1,

        /// <summary>
        /// Sorts by the last modified time
        /// </summary>
        LastModified = 2,

        /// <summary>
        /// Sorts by Featured datasets first (and then name)
        /// </summary>
        Featured = 3
    }
}