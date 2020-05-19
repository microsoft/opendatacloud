using System.Linq;
using McMaster.Extensions.CommandLineUtils;
using Msr.Odr.Admin.Commands.Options;

namespace Msr.Odr.Admin.Commands.Search
{
	public class SearchInitCommand
    {
		public static void Configure(CommandLineApplication command)
		{
			command.Description = "Initializes the Azure Search configuration";
			command.SetDefaultHelp();

		    var indexesOption = command.Option(
		        "--indexes | -i <indexes>", $"The comma-separated list of indexes ({IndexOptions.AllIndexNames})",
		        CommandOptionType.SingleValue);

			command.OnExecute(async () =>
			{
			    var search = new SearchOptions();
			    var index = new IndexOptions(indexesOption.Value());
			    var cosmos = new CosmosOptions();

			    if (command.HasAllRequiredParameters(new[]
			    {
			        search.Key,
			        search.Name,
			        cosmos.DatasetsCollection,
			        cosmos.Database,
			        cosmos.Endpoint,
			        cosmos.RawKey,
			        index.DatasetIndexer,
			        index.DatasetDataSource,
			        index.DatasetIndex,
			        index.FileIndexer,
			        index.FileIndex,
			        index.FileDataSource
			    }))
			    {
			        await new Admin.Search.CreateSearchTask(search, index, cosmos).ExecuteAsync();
                }

			    return 0;
            });
		}
	}
}
