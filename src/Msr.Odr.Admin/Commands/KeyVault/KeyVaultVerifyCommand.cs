// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Msr.Odr.Admin.Commands.Options;

namespace Msr.Odr.Admin.Commands.KeyVault
{
	public class KeyVaultVerifyCommand
    {
		public static void Configure(CommandLineApplication command)
		{
			command.Description = "Verifies secrets exist in Key Vault.";
			command.SetDefaultHelp();

            command.OnExecute(async () =>
			{
                var vault = new VaultOptions();

                if (command.HasAllRequiredParameters(new[]
                {
                    vault.VaultUrl,
                }))
                {
                    await new Admin.KeyVault.KeyVaultVerifyTask(vault).ExecuteAsync();
                }

			    return 0;
			});
		}
	}
}
