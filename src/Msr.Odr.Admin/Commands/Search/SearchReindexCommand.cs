using System;
using System.Linq;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Msr.Odr.Admin.Commands.Options;
using Msr.Odr.Admin.Search;

namespace Msr.Odr.Admin.Commands.Search
{
	public class SearchReindexCommand
	{
		public static void Configure(CommandLineApplication command)
		{
			command.Description = "Runs the Azure Search indexers to update the index";
			command.SetDefaultHelp();

            var indexesOption = command.Option(
		        "--indexes | -i <indexes>", $"The comma-separated list of indexes ({IndexOptions.AllIndexNames})",
		        CommandOptionType.SingleValue);

            command.OnExecute(async () =>
			{
			    var search = new SearchOptions();
			    var index = new IndexOptions(indexesOption.Value());

			    if (command.HasAllRequiredParameters(new[]
			    {
			        search.Key,
			        search.Name,
			        index.DatasetIndexer,
			        index.FileIndexer
			    }))
			    {
			        Console.WriteLine($"Reindexing {index.SelectedIndexTypes}");
                    await new ReindexSearchTask(search, index).ExecuteAsync();
			    }

			    return 0;
			});
		}
	}
}
