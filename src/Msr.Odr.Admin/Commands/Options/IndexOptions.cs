// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;

namespace Msr.Odr.Admin.Commands.Options
{
	/// <summary>
	/// Azure Search indexer line options
	/// </summary>
	public class IndexOptions
	{
        private IConfiguration Config => Startup.Configuration;

        public static string AllIndexNames => string.Join(",", new[]
	    {
	        SearchIndexTypes.Datasets.ToString(),
	        SearchIndexTypes.Files.ToString(),
	        SearchIndexTypes.Nominations.ToString(),
	    });

		public IndexOptions(string indexes)
		{
			this.FileIndex = Config["Index:FileIndex"] ?? "files-ix";
			this.FileDataSource = Config["Index:FileDatasource"] ?? "files-ds";
			this.FileIndexer = Config["Index:FileIndexer"] ?? "files-indexer";

			this.DatasetIndex = Config["Index:DatasetIndex"] ?? "dataset-ix";
			this.DatasetDataSource = Config["Index:DatasetDatasource"] ?? "dataset-ds";
			this.DatasetIndexer = Config["Index:DatasetIndexer"] ?? "dataset-indexer";

			this.NominationIndex = Config["Index:NominationIndex"] ?? "nomination-ix";
			this.NominationDataSource = Config["Index:NominationDatasource"] ?? "nomination-ds";
			this.NominationIndexer = Config["Index:NominationIndexer"] ?? "nomination-indexer";

		    this.SelectedIndexTypes = (indexes ?? string.Empty)
		        .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
		        .Select(s => Enum.Parse(typeof(SearchIndexTypes), s.Trim(), true))
		        .Aggregate(SearchIndexTypes.None, (selected, v) => selected | (SearchIndexTypes)v);
        }

        public string FileIndex { get; private set; }
		public string FileDataSource { get; private set; }
		public string FileIndexer { get; private set; }

		public string DatasetIndex { get; private set; }
		public string DatasetDataSource { get; private set; }
		public string DatasetIndexer { get; private set; }

        public string NominationIndex { get; private set; }
        public string NominationDataSource { get; private set; }
        public string NominationIndexer { get; private set; }

        public SearchIndexTypes SelectedIndexTypes { get; private set; }
	}
}
