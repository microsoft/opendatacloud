using McMaster.Extensions.CommandLineUtils;
using Msr.Odr.Admin.Commands.Batch;

namespace Msr.Odr.Admin.Commands
{
    public class BatchCommand
    {
		public static void Configure(CommandLineApplication command)
		{
			command.Description = "Commands relating to Azure Batch";
			command.SetDefaultHelp();

			command.Command("init", BatchInitCommand.Configure);

			command.OnExecuteShowHelp();
		}
	}
}
