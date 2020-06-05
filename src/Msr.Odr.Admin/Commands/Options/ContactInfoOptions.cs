// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;

namespace Msr.Odr.Admin.Commands.Options
{
	/// <summary>
	/// Contact Information command line options
	/// </summary>
	public class ContactInfoOptions
    {
        private IConfiguration Config => Startup.Configuration;

        public string Name => Config["Contact:Name"];
        public string Email => Config["Contact:Email"];
	}
}
