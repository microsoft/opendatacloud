// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Msr.Odr.Model.Configuration
{
    public static class WellKnownIds
    {
        /// <summary>
        /// The dataset id that is used as the partition key for configuration information
        /// in the Datasets collection.
        /// </summary>
        public static readonly Guid ConfigurationDatasetId = new Guid("fd56f7c8-89a5-4997-82bc-95e955468e14");

        /// <summary>
        /// The id of the document containing the list of domains.
        /// </summary>
        public static readonly Guid DomainsDocId = new Guid("264f5075-8ed2-4aac-b389-a42678904b12");

        /// <summary>
        /// The dataset id that is used as the partition key for dataset nominations.
        /// </summary>
        public static readonly Guid DatasetNominationDatasetId = new Guid("a535c9fd-bb0f-4550-8859-fbabb6e257e1");

        /// <summary>
        /// The dataset id that is used as the partition key for dataset edits by dataset owners.
        /// </summary>
        public static readonly Guid DatasetEditDatasetId = new Guid("6017d462-8def-4a3a-9b60-23d8d974bb92");

        /// <summary>
        /// The dataset id that is used as the partition key for general issues.
        /// </summary>
        public static readonly Guid GeneralIssueDatasetId = new Guid("10122dc2-967f-4743-b84a-5a7b2863da7b");

        /// <summary>
        /// The dataset id for licenses.
        /// </summary>
        public static readonly Guid LicenseDatasetId = new Guid("00000000-cafe-5000-0080-000000000001");

        /// <summary>
        /// The id of the document containing the list of domains.
        /// </summary>
        public static readonly Guid DatasetOwnersDocId = new Guid("dd71bbb6-aaa7-4f78-a0bf-1d0f6759e674");
    }
}
