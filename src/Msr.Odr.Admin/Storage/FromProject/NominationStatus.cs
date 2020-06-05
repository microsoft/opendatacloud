// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Runtime.Serialization;

namespace Msr.Odr.Model.UserData
{
    public enum NominationStatus
    {
        /// <summary>
        /// Dataset status is Unknown or not yet approved/rejected
        /// </summary>
        [EnumMember(Value = "Pending Approval")]
        PendingApproval = 0,

        /// <summary>
        /// Dataset nomination is approved.
        /// </summary>
        [EnumMember(Value = "Approved")]
        Approved = 1,

        /// <summary>
        /// Dataset nomination is rejected.
        /// </summary>
        [EnumMember(Value = "Rejected")]
        Rejected = 2,

        /// <summary>
        /// Dataset nomination is uploading.
        /// </summary>
        [EnumMember(Value = "Uploading")]
        Uploading = 3,

        /// <summary>
        /// Dataset nomination is importing.
        /// </summary>
        [EnumMember(Value = "Importing")]
        Importing = 4,

        /// <summary>
        /// Dataset nomination is processing is finished.
        /// </summary>
        [EnumMember(Value = "Complete")]
        Complete = 5,

        /// <summary>
        /// Dataset nomination is in error state.
        /// </summary>
        [EnumMember(Value = "Error")]
        Error = 6
    }
}
