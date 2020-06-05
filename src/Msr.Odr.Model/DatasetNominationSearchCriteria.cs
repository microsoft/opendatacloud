// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Msr.Odr.Model
{
    /// <summary>
    /// Search criteria for dataset nominations
    /// </summary>
    public class DatasetNominationSearchCriteria
    {
        public DatasetNominationSearchCriteria()
        {
            PageSize = 5;
        }

        /// <summary>
        /// The zero-based page number of nominations.
        /// </summary>
        /// <value>The identifier.</value>
        public int PageNumber { get; set; }

        /// <summary>
        /// The number of items per page.
        /// </summary>
        public int PageSize { get; set; }
    }
}
