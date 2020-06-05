// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using McMaster.Extensions.CommandLineUtils;
using Msr.Odr.Admin.Commands.KeyVault;

namespace Msr.Odr.Admin.Commands
{
    public class KeyVaultCommand
    {
		public static void Configure(CommandLineApplication command)
		{
			command.Description = "Commands relating to Key Vault";
			command.SetDefaultHelp();

            command.Command("verify", KeyVaultVerifyCommand.Configure);

			command.OnExecuteShowHelp();
		}
	}
}
