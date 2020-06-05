// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Runtime.Serialization;

namespace Msr.Odr.Model.UserData
{
    public enum NominationLicenseType
    {
        [EnumMember(Value = "Unknown")]
        Unknown = 0,

        [EnumMember(Value = "Standard")]
        Standard = 1,

        [EnumMember(Value = "HtmlText")]
        HtmlText = 2,

        [EnumMember(Value = "InputFile")]
        InputFile = 3
    }
}