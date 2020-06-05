// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Runtime.Serialization;

namespace Msr.Odr.Model.FileSystem
{
    /// <summary>
    /// The record type for a file system entry
    /// </summary>
    public enum FileSystemEntryType
    {
        /// <summary>
        /// Unknown or unspecified
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// A file record
        /// </summary>
        [EnumMember(Value = "file")]
        File = 1,

        /// <summary>
        /// A folder record
        /// </summary>
        [EnumMember(Value = "folder")]
        Folder = 2
    }
}