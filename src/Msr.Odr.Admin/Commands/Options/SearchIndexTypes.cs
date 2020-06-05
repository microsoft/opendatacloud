// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Msr.Odr.Admin.Commands.Options
{
    [Flags]
    public enum SearchIndexTypes
    {
        None = 0x00,
        Datasets = 0x01,
        Files = 0x02,
        Nominations = 0x04,
        //Issues = 0x08,
        //Licenses = 0x10,
    }
}
