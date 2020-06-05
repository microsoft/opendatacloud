// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;

namespace Msr.Odr.Admin.Commands.Options
{
	/// <summary>
	/// Azure Search command line options
	/// </summary>
	public class SearchOptions
    {
        private IConfiguration Config => Startup.Configuration;

        public string Name => Config["Search:Account"];
		public string Key => Config["Search:Key"];
	}
}
