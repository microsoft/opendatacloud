// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;

namespace Msr.Odr.Admin.Commands.Options
{
	/// <summary>
	/// Storage account command line options
	/// </summary>
	public class StorageOptions
	{
        private IConfiguration Config => Startup.Configuration;

        public string Account => Config["Storage:DefaultAccount"];
		public string Key => Config[$"Storage:Accounts:{Account}"];
	}
}
