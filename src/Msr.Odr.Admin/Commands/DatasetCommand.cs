// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using McMaster.Extensions.CommandLineUtils;
using Msr.Odr.Admin.Commands.Batch;
using Msr.Odr.Admin.Commands.Dataset;

namespace Msr.Odr.Admin.Commands
{
    public class DatasetCommand
    {
		public static void Configure(CommandLineApplication command)
		{
			command.Description = "Commands relating to Datasets";
			command.SetDefaultHelp();

			command.Command("delete", DatasetDeleteCommand.Configure);
			command.Command("nominate", DatasetNominateCommand.Configure);
			command.Command("import", DatasetImportCommand.Configure);
			command.Command("verify", DatasetVerifyCommand.Configure);
			command.Command("view", DatasetViewCommand.Configure);

			command.OnExecuteShowHelp();
		}
	}
}
