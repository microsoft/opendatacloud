// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using McMaster.Extensions.CommandLineUtils;
using Msr.Odr.Admin.Commands.Batch;
using Msr.Odr.Admin.Commands.Nominations;

namespace Msr.Odr.Admin.Commands
{
    public class NominationCommand
    {
		public static void Configure(CommandLineApplication command)
		{
			command.Description = "Commands relating to Dataset Nominations";
			command.SetDefaultHelp();

			command.Command("delete", NominationDeleteCommand.Configure);

			command.OnExecuteShowHelp();
		}
	}
}
