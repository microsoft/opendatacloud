// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Runtime.Serialization;

namespace Msr.Odr.Model.UserData
{
    /// <summary>
    /// The status of the dataset edit process
    /// </summary>
    public enum DatasetEditStatus
    {
        /// <summary>
        /// The original dataset has not been modified.
        /// </summary>
        [EnumMember(Value = "unmodified")]
        Unmodified = 0,

        /// <summary>
        /// The details (metadata) of the dataset has been modified, but not yet saved.
        /// </summary>
        [EnumMember(Value = "details-modified")]
        DetailsModified = 1,

        /// <summary>
        /// The contents of the dataset are being updated (also implies "DetailsModified").
        /// </summary>
        [EnumMember(Value = "contents-modified")]
        ContentsModified = 2,

        /// <summary>
        /// The contents of the updated dataset are being imported.
        /// </summary>
        [EnumMember(Value = "importing")]
        Importing = 3,
    }
}