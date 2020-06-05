// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using McMaster.Extensions.CommandLineUtils;

namespace Msr.Odr.Admin.Commands
{
    public class MainCommand
    {
		public static void Configure(CommandLineApplication command)
		{
			command.FullName = "MSR ODR Configuration Utility";
            command.Description = "Provides administrative support for the Microsoft Research OpenData application.\n" +
                $"Using Key Vault: {Startup.KeyVaultUrl}";
            command.SetDefaultHelp();

			command.Command("cosmos", CosmosCommand.Configure);
            command.Command("search", SearchCommand.Configure);
            command.Command("vault", KeyVaultCommand.Configure);
            command.Command("batch", BatchCommand.Configure);
            command.Command("data", DataCommand.Configure);
            command.Command("dataset", DatasetCommand.Configure);
            command.Command("nomination", NominationCommand.Configure);

            command.OnExecuteShowHelp();
		}
	}
}
