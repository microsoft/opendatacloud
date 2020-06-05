// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Msr.Odr.Model.Validators
{
    public static class ValidatorHelpers
    {
        public static bool BeValidUrl(string url, bool allowEmpty = true)
        {
            if (allowEmpty && string.IsNullOrEmpty(url)) { return true; }

            return Uri.IsWellFormedUriString(url, UriKind.Absolute);
        }
    }
}
