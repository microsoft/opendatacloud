// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Linq;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Msr.Odr.Admin.Commands.Options;

namespace Msr.Odr.Admin.Commands.Dataset
{
    public class DatasetVerifyCommand
    {
        public static void Configure(CommandLineApplication command)
        {
            command.Description = "Verifies dataset details.";
            command.SetDefaultHelp();

            var fixCmd = command.Option("--fix | -f", "Fixes datasets (if possible).", CommandOptionType.NoValue);

            command.OnExecute(async () =>
            {
                var cosmos = new CosmosOptions();
                var storage = new StorageOptions();
                var search = new SearchOptions();
                var indexes = new IndexOptions(null);

                if (command.HasAllRequiredParameters(new[]
                {
                    cosmos.Endpoint,
                    cosmos.Database,
                    storage.Account,
                    search.Key,
                    search.Name
                }))
                {
                    bool fixDatasets = fixCmd.HasValue();
                    await new Admin.Dataset.DatasetVerifyTask(cosmos, storage, search, indexes, fixDatasets).ExecuteAsync();
                }

                return 0;
            });
        }
    }
}
