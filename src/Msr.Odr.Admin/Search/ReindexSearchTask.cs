// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Msr.Odr.Admin.Commands;
using Msr.Odr.Admin.Commands.Options;

namespace Msr.Odr.Admin.Search
{
	public class ReindexSearchTask : SearchTask
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ReindexSearchTask" /> class.
		/// </summary>
		/// <param name="search">The Azure search configuration options.</param>
		/// <param name="cosmos">The Cosmos configuration options.</param>
		public ReindexSearchTask(SearchOptions search, IndexOptions index)
			: base(search, index)
		{
		}

		/// <summary>
		/// Executes the task asynchronously
		/// </summary>
		/// <returns>A status code</returns>
		public override async Task<int> ExecuteAsync()
		{
			var client = await this.CreateClientAsync().ConfigureAwait(false);

		    var list = new[]
		    {
		        (SearchType: SearchIndexTypes.Files, IndexName: IndexOptions.FileIndexer),
		        (SearchType: SearchIndexTypes.Datasets, IndexName: IndexOptions.DatasetIndexer),
		        (SearchType: SearchIndexTypes.Nominations, IndexName: IndexOptions.NominationIndexer),
            };

            await list
                .Where(v => (v.SearchType & IndexOptions.SelectedIndexTypes) == v.SearchType)
                .ForEachAsync(async (v) =>
                    {
                        Console.WriteLine($"Indexing {v.SearchType} (Name: \"{v.IndexName}\")");
                        await client.Indexers.RunAsync(v.IndexName);
                        await MonitorIndexingAsync(v.IndexName);
                    });

			return 0;
		}
	}
}
