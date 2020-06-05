// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Msr.Odr.Admin.Commands.Options;
using Msr.Odr.Admin.Data;

namespace Msr.Odr.Admin.Commands.Dataset
{
    public class DataInitCommand
    {
        public static string AllDataTypes => string.Join(",", new[]
        {
            DataInitTypes.Domains.ToString(),
            DataInitTypes.Licenses.ToString(),
            DataInitTypes.FAQs.ToString(),
            DataInitTypes.Email.ToString(),
            DataInitTypes.ARM.ToString(),
            DataInitTypes.DatasetOwners.ToString(),
        });

        public static void Configure(CommandLineApplication command)
        {
            command.Description = "Creates initial data for application.";
            command.SetDefaultHelp();

            var typesOption = command.Option(
                "--types | -t <types>", $"The comma-separated list of types to initialize ({AllDataTypes}) or '*' for all.",
                CommandOptionType.SingleValue);

            command.OnExecute(async () =>
            {
                var cosmos = new CosmosOptions();
                var storage = new StorageOptions();

                DataInitTypes selectedTypes = DataInitTypes.None;
                string typesText = typesOption.Value();
                if(typesText == "*")
                {
                    selectedTypes =
                        DataInitTypes.Domains |
                        DataInitTypes.Licenses |
                        DataInitTypes.FAQs |
                        DataInitTypes.Email |
                        DataInitTypes.ARM |
                        DataInitTypes.DatasetOwners;
                }
                else if(!string.IsNullOrWhiteSpace(typesText))
                {
                    selectedTypes = typesText
                        .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => Enum.Parse(typeof(DataInitTypes), s.Trim(), true))
                        .Aggregate(DataInitTypes.None, (selected, v) => selected | (DataInitTypes)v);
                }
                if (selectedTypes == DataInitTypes.None)
                {
                    throw new ArgumentException("No data types specified.");
                }

                if (command.HasAllRequiredParameters(new[]
                {
                    cosmos.Endpoint,
                    cosmos.Database,
                    storage.Account
                }))
                {
                    await new Admin.Data.DataInitTask(cosmos, storage, selectedTypes).ExecuteAsync();
                }

                return 0;
            });
        }
    }
}
