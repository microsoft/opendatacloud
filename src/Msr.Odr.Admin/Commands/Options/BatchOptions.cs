// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;

namespace Msr.Odr.Admin.Commands.Options
{
	/// <summary>
	/// Azure Batch command line options
	/// </summary>
	public class BatchOptions
    {
        private IConfiguration Config => Startup.Configuration;

        public string Url => Config["Batch:Url"];
        public string Account => Config["Batch:Account"];
        public string Key => Config["Batch:Key"];
        public string StorageName => Config["Batch:StorageName"];
        public string StorageKey => Config["Batch:StorageKey"];
	}
}
