// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Msr.Odr.Services.Configuration
{
    public class EnvironmentValueNotFoundException : Exception
    {
        public EnvironmentValueNotFoundException(string name) :
            base($"The value for environment variable \"{name}\" was not found.")
        {
        }
    }
}
