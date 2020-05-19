using System.Linq;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Msr.Odr.Admin.Commands.Options;

namespace Msr.Odr.Admin.Commands.Dataset
{
	public class DatasetNominateCommand
    {
		public static void Configure(CommandLineApplication command)
		{
			command.Description = "Adds a dataset as an uploaded nomination (for refreshing catalog)";
			command.SetDefaultHelp();

		    var datasetIdOpt = command.Option("--datasetId | -dsid <datasetId>", "The dataset identifier", CommandOptionType.SingleValue);
            var queueOpt = command.Option("--queue | -q", "Queues the job to catalog the dataset.", CommandOptionType.NoValue);

            command.OnExecute(async () =>
			{
			    var cosmos = new CosmosOptions();
                var contact = new ContactInfoOptions();
                var batch = new BatchOptions();
			    var datasetId = datasetIdOpt.Value();

                if (command.HasAllRequiredParameters(new[]
                {
                    cosmos.Endpoint,
                    cosmos.Database,
                    contact.Name,
                    contact.Email,
                    batch.Url,
                    batch.Account,
                    batch.Key,
                    datasetId,
                }))
                {
                    await new Admin.Dataset.DatasetNominateTask(cosmos, contact, batch, datasetId, queueOpt.HasValue()).ExecuteAsync();
                }

			    return 0;
			});
		}
	}
}
