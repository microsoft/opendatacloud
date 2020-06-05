// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Linq;
using McMaster.Extensions.CommandLineUtils;
using Msr.Odr.Admin.Commands.Options;

namespace Msr.Odr.Admin.Commands.Cosmos
{
	public class CosmosInitCommand
	{
		public static void Configure(CommandLineApplication command)
		{
			command.Description = "Creates the initial database and collection in an account";
		    command.SetDefaultHelp();

			command.OnExecute(async () =>
			{
			    var cosmos = new CosmosOptions();
			    if (command.HasAllRequiredParameters(new[]
			    {
			        cosmos.DatasetsCollection,
			        cosmos.Database,
			        cosmos.Endpoint,
			        cosmos.RawKey
			    }))
			    {
			        await new Storage.CreateDatabaseTask(cosmos).ExecuteAsync();
                }

			    return 0;
            });
		}
	}
}
