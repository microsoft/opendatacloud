// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Msr.Odr.Admin.Commands.Options;

namespace Msr.Odr.Admin.Commands.Batch
{
	public class BatchInitCommand
    {
		public static void Configure(CommandLineApplication command)
		{
			command.Description = "Initializes the Azure Batch configuration";
			command.SetDefaultHelp();

            command.OnExecute(async () =>
			{
                var batch = new BatchOptions();
                var search = new SearchOptions();
                var cosmos = new CosmosOptions();
                var storage = new StorageOptions();
                var vault = new VaultOptions();

                if (command.HasAllRequiredParameters(new[]
                {
                    batch.Account,
                    batch.Url,
                    batch.Key,
                    batch.StorageName,
                    batch.StorageKey,

                    search.Name,
                    search.Key,

                    cosmos.Endpoint,
                    cosmos.RawKey,
                    cosmos.Database,
                    cosmos.DatasetsCollection,
                    cosmos.UserDataCollection,

                    storage.Account,
                    storage.Key,

                    vault.VaultUrl
                }))
                {
                    await new Admin.Batch.BatchInitTask(batch, search, cosmos, storage, vault).ExecuteAsync();
                }

			    return 0;
			});
		}
	}
}
