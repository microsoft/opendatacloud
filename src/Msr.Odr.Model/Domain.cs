// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Msr.Odr.Model
{
    /// <summary>
    /// A domain that classifies a dataset.
    /// </summary>
    public class Domain
	{
        /// <summary>
        /// Gets or sets the unique identifier for the domain.
        /// </summary>
        /// <value>The identifier.</value>
        public string Id
		{
			get;
			set;
		}

        /// <summary>
        /// Gets or sets the name (short description) of the domain.
        /// </summary>
        /// <value>The name.</value>
        public string Name
		{
			get;
			set;
		}
    }
}