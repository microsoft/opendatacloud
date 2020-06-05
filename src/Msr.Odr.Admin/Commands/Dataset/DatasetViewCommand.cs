// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Linq;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Msr.Odr.Admin.Commands.Options;

namespace Msr.Odr.Admin.Commands.Dataset
{
    public class DatasetViewCommand
    {
        public static void Configure(CommandLineApplication command)
        {
            command.Description = "Views dataset details.";
            command.SetDefaultHelp();

            var datasetIdOption = command.Option("--id", "Id of dataset.", CommandOptionType.SingleValue);

            command.OnExecute(async () =>
            {
                var cosmos = new CosmosOptions();
                var storage = new StorageOptions();
                string datasetId = datasetIdOption.Value();

                if (command.HasAllRequiredParameters(new[]
                {
                    cosmos.Endpoint,
                    cosmos.Database,
                    storage.Account,
                    datasetId,
                }))
                {
                    await new Admin.Dataset.DatasetViewTask(cosmos, storage, datasetId).ExecuteAsync();
                }

                return 0;
            });
        }
    }
}
