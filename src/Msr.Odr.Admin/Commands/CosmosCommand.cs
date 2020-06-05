// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using McMaster.Extensions.CommandLineUtils;
using Msr.Odr.Admin.Commands.Cosmos;

namespace Msr.Odr.Admin.Commands
{
    public class CosmosCommand
    {
		public static void Configure(CommandLineApplication command)
		{
			command.Description = "Commands relating to Cosmos DB storage";
		    command.SetDefaultHelp();

            command.Command("init", CosmosInitCommand.Configure);

		    command.OnExecuteShowHelp();
		}
    }
}
