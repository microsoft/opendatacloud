// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Runtime.Serialization;

namespace Msr.Odr.Model.Datasets
{
    /// <summary>
    /// The datatype for the stored document record
    /// </summary>
    public enum StorageDataType
    {
        /// <summary>
        /// Unknown or unspecified
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// The dataset
        /// </summary>
		[EnumMember(Value = "dataset")]
        Dataset = 1,

        /// <summary>
        /// The file system
        /// </summary>
        [EnumMember(Value = "filesystem")]
        FileSystem = 2,

        /// <summary>
        /// A license definition record.
        /// </summary>
        [EnumMember(Value = "license")]
        License = 4,

        /// <summary>
        /// List of dataset domains.
        /// </summary>
        [EnumMember(Value = "domains")]
        Domains = 5,

        /// <summary>
        /// Frequently Asked Question (FAQ)
        /// </summary>
        [EnumMember(Value = "faq")]
        FAQ = 6,

        /// <summary>
        /// The summary of the dataset contents
        /// </summary>
        [EnumMember(Value = "fileSummary")]
        FileSummary = 7,

        /// <summary>
        /// The summary of the dataset contents
        /// </summary>
        [EnumMember(Value = "storage")]
        Storage = 8,
    }
}
