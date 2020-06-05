// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Linq;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Msr.Odr.Admin.Commands.Options;

namespace Msr.Odr.Admin.Commands.Dataset
{
	public class DatasetDeleteCommand
    {
		public static void Configure(CommandLineApplication command)
		{
			command.Description = "Deletes a specific dataset";
			command.SetDefaultHelp();

		    command.Option("--datasetId | -dsid <datasetId>", "The dataset identifier", CommandOptionType.SingleValue);

            command.OnExecute(async () =>
			{
                var search = new SearchOptions();
                var cosmos = new CosmosOptions();
			    var datasetId = command.Options.FirstOrDefault(t => t.ValueName == "datasetId")?.Value();

                if (command.HasAllRequiredParameters(new[]
                {
                    search.Name,
                    cosmos.Endpoint,
                    cosmos.Database,
                    datasetId,
                }))
                {
                    await new Admin.Dataset.DatasetDeleteTask(search, cosmos, datasetId).ExecuteAsync();
                }

			    return 0;
			});
		}
	}
}
