// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using McMaster.Extensions.CommandLineUtils;
using Msr.Odr.Admin.Commands.Batch;
using Msr.Odr.Admin.Commands.Dataset;

namespace Msr.Odr.Admin.Commands
{
    public class DataCommand
    {
		public static void Configure(CommandLineApplication command)
		{
			command.Description = "Commands relating to data initialization";
			command.SetDefaultHelp();

			command.Command("init", DataInitCommand.Configure);

			command.OnExecuteShowHelp();
		}
	}
}
