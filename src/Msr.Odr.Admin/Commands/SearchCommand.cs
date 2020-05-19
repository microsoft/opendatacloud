using System;
using System.Collections.Generic;
using System.Text;
using McMaster.Extensions.CommandLineUtils;
using Msr.Odr.Admin.Commands.Search;

namespace Msr.Odr.Admin.Commands
{
	public class SearchCommand
    {
		public static void Configure(CommandLineApplication command)
		{
			command.Description = "Commands relating to Azure Search";
			command.SetDefaultHelp();

			command.Command("init", SearchInitCommand.Configure);
			command.Command("reindex", SearchReindexCommand.Configure);

			command.OnExecuteShowHelp();
		}
	}
}
