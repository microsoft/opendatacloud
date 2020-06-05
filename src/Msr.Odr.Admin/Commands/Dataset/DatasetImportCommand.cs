// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Linq;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Msr.Odr.Admin.Commands.Options;

namespace Msr.Odr.Admin.Commands.Dataset
{
	public class DatasetImportCommand
    {
		public static void Configure(CommandLineApplication command)
		{
			command.Description = "Imports a dataset from another site as a nomination.";
			command.SetDefaultHelp();

		    var datasetUrlOpt = command.Option(
                "--datasetUrl | -u <datasetUrl>",
                "The API url of the dataset",
                CommandOptionType.SingleValue);
            var storageNameOpt = command.Option(
                "--storageName | -n <storageName>", 
                "The name of the storage container where the dataset contents reside.", 
                CommandOptionType.SingleValue);

            command.OnExecute(async () =>
			{
			    var cosmos = new CosmosOptions();
                var storage = new StorageOptions();
                var contact = new ContactInfoOptions();
			    var datasetUrl = datasetUrlOpt.Value();
			    var storageName = storageNameOpt.Value();

                if (command.HasAllRequiredParameters(new[]
                {
                    cosmos.Endpoint,
                    cosmos.Database,
                    storage.Account,
                    storage.Key,
                    contact.Name,
                    contact.Email,
                    datasetUrl,
                    storageName,
                }))
                {
                    await new Admin.Dataset.DatasetImportTask(cosmos, storage, contact, datasetUrl, storageName).ExecuteAsync();
                }

			    return 0;
			});
		}
	}
}
