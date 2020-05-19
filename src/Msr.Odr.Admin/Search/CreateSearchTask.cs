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
    public class CreateSearchTask : SearchTask
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CreateSearchTask" /> class.
        /// </summary>
        /// <param name="search">The Azure search configuration options.</param>
        /// <param name="cosmos">The Cosmos configuration options.</param>
        public CreateSearchTask(SearchOptions search, IndexOptions index, CosmosOptions cosmos)
            : base(search, index)
        {
            this.CosmosOptions = cosmos;
        }

        /// <summary>
        /// Gets the Cosmos options
        /// </summary>
        protected CosmosOptions CosmosOptions
        {
            get;
        }

        /// <summary>
        /// Executes the task asynchronously
        /// </summary>
        /// <returns>A status code</returns>
        public override async Task<int> ExecuteAsync()
        {
            var list = new[]
            {
                (
                    Collection: CosmosOptions.DatasetsCollection,
                    SearchType: SearchIndexTypes.Files,
                    Index: IndexOptions.FileIndex,
                    DataSource: IndexOptions.FileDataSource,
                    Indexer: IndexOptions.FileIndexer,
                    FieldMap: (ICollection<FieldMapping>)null,
                    CreateIndex: (Func<Task<Index>>)CreateFileIndexAsync,
                    DataSourceQuery: $"SELECT * FROM {CosmosOptions.DatasetsCollection} d WHERE d._ts >= @HighWaterMark AND d.dataType = 'filesystem' ORDER BY d._ts"
                ),
                (
                    Collection: CosmosOptions.DatasetsCollection,
                    SearchType: SearchIndexTypes.Datasets,
                    Index: IndexOptions.DatasetIndex,
                    DataSource: IndexOptions.DatasetDataSource,
                    Indexer: IndexOptions.DatasetIndexer,
                    FieldMap: (ICollection<FieldMapping>)null,
                    CreateIndex: (Func<Task<Index>>)CreateDatasetIndexAsync,
                    DataSourceQuery: $"SELECT * FROM {CosmosOptions.DatasetsCollection} d WHERE d._ts > @HighWaterMark AND d.dataType='dataset' ORDER BY d._ts"
                ),
                (
                    Collection: CosmosOptions.UserDataCollection,
                    SearchType: SearchIndexTypes.Nominations,
                    Index: IndexOptions.NominationIndex,
                    DataSource: IndexOptions.NominationDataSource,
                    Indexer: IndexOptions.NominationIndexer,
                    FieldMap: new []
                    {
                        new FieldMapping { SourceFieldName = "_ts", TargetFieldName = "timeStamp" }
                    },
                    CreateIndex: (Func<Task<Index>>)CreateNominationIndexAsync,
                    DataSourceQuery: $"SELECT * FROM {CosmosOptions.UserDataCollection} d WHERE d._ts > @HighWaterMark AND d.dataType='dataset-nomination' ORDER BY d._ts"
                ),
            };

            await list
                .Where(v => (v.SearchType & IndexOptions.SelectedIndexTypes) == v.SearchType)
                .ForEachAsync(async (v) =>
                {
                    Console.WriteLine($"Creating index {v.SearchType}");
                    await DeleteExistingIndexing(v.Index, v.DataSource, v.Indexer);
                    await v.CreateIndex();
                    await CreateDatasourceAsync(v.Collection, v.DataSource, v.DataSourceQuery);
                    await CreateIndexerAsync(v.Indexer, v.Index, v.DataSource, v.FieldMap);
                    await MonitorIndexingAsync(v.Indexer);
                });

            return 0;
        }

        /// <summary>
        /// Asynchronously creates a CosmosDB datasource
        /// </summary>
        /// <param name="collection">The CosmosDB collection.</param>
        /// <param name="datasource">The datasource.</param>
        /// <param name="query">The query.</param>
        /// <returns>The created datasource.</returns>
        private async Task<DataSource> CreateDatasourceAsync(string collection, string datasource, string query)
        {
            var client = await this.CreateClientAsync().ConfigureAwait(false);
            var connStr = $"AccountName={this.CosmosOptions.Endpoint};AccountKey={this.CosmosOptions.RawKey};Database={this.CosmosOptions.Database}";
            var definition = new DataSource
            {
                Name = datasource,
                Type = "documentdb",
                Credentials = new DataSourceCredentials
                {
                    ConnectionString = connStr
                },
                Container = new DataContainer
                {
                    Name = collection,
                    Query = query,
                },
                DataChangeDetectionPolicy = new HighWaterMarkChangeDetectionPolicy
                {
                    HighWaterMarkColumnName = "_ts",
                },
                DataDeletionDetectionPolicy = new SoftDeleteColumnDeletionDetectionPolicy
                {
                    SoftDeleteColumnName = "isDeleted",
                    SoftDeleteMarkerValue = "true",
                }
            };
            var result = await client.DataSources.CreateAsync(definition).ConfigureAwait(false);
            return result;
        }

        /// <summary>
        /// Deletes the existing indexing resources.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="dataSource">The data source.</param>
        /// <param name="indexer">The indexer.</param>
        /// <returns>The task instance.</returns>
        private async Task DeleteExistingIndexing(string index, string dataSource, string indexer)
        {
            var client = await this.CreateClientAsync().ConfigureAwait(false);
            await client.Indexers.DeleteWithHttpMessagesAsync(indexer);
            await client.DataSources.DeleteWithHttpMessagesAsync(dataSource);
            await client.Indexes.DeleteWithHttpMessagesAsync(index);
        }

        private async Task<Index> CreateFileIndexAsync()
        {
            ISearchServiceClient client = await this.CreateClientAsync().ConfigureAwait(false);
            var tokenizers = new Tokenizer[] {
                    new MicrosoftLanguageStemmingTokenizer(
                        "customStemmingTokenizer",
                        100,
                        true,
                        MicrosoftStemmingTokenizerLanguage.English)};
            var filters = new TokenFilter[]{
                    new AsciiFoldingTokenFilter("customFoldingFilter", true),
                    new WordDelimiterTokenFilter(
                        "customWordFilter",
                        generateWordParts: true,
                        generateNumberParts: false,
                        catenateWords: false,
                        catenateNumbers: false,
                        catenateAll: false,
                        splitOnCaseChange:true,
                        preserveOriginal: true,
                        splitOnNumerics:true),
                    new StemmerTokenFilter("customStemmerFilter", StemmerTokenFilterLanguage.English)};

            var analyzers = new Analyzer[]{
                    new CustomAnalyzer(
                        "fileAnalyzer",
                        TokenizerName.Create("customStemmingTokenizer"),
                        new List<TokenFilterName>(
                            new [] {
                                TokenFilterName.Create("lowercase"),
                                TokenFilterName.Create("customFoldingFilter"),
                                TokenFilterName.Create("customWordFilter")}))};

            var defaultAnalyzer = AnalyzerName.Create("fileAnalyzer");

            var definition = new Index()
            {
                Name = this.IndexOptions.FileIndex,
                Fields = new[]
                {
                        new Field("datasetId", DataType.String)         { IsKey = false,  IsSearchable = false, IsFilterable = true, IsSortable = false, IsFacetable = false, IsRetrievable = true},
                        new Field("id", DataType.String)         { IsKey = true, IsSearchable = false,  IsFilterable = false,  IsSortable = false,  IsFacetable = false, IsRetrievable = true},
                        new Field("parent", DataType.String)         { IsKey = false, IsSearchable = true,  IsFilterable = true,  IsSortable = true,  IsFacetable = false, IsRetrievable = true},
                        new Field("name", DataType.String)         { IsKey = false, IsSearchable = true,  IsFilterable = true,  IsSortable = true,  IsFacetable = false, IsRetrievable = true, IndexAnalyzer = defaultAnalyzer, SearchAnalyzer = defaultAnalyzer },
                        new Field("length", DataType.Int64)          { IsKey = false, IsSearchable = false, IsFilterable = true,  IsSortable = true,  IsFacetable = true,  IsRetrievable = true},
                        new Field("fullName", DataType.String)         { IsKey = false, IsSearchable = true,  IsFilterable = true,  IsSortable = true,  IsFacetable = false, IsRetrievable = true, Analyzer = AnalyzerName.EnMicrosoft},
                        new Field("sortKey", DataType.String)          { IsKey = false, IsSearchable = false, IsFilterable = false,  IsSortable = true,  IsFacetable = false,  IsRetrievable = false},
                        new Field("modified", DataType.DateTimeOffset)          { IsKey = false, IsSearchable = false, IsFilterable = true,  IsSortable = true,  IsFacetable = true,  IsRetrievable = true},
                        new Field("entryType", DataType.String)          { IsKey = false, IsSearchable = false, IsFilterable = true,  IsSortable = false,  IsFacetable = false,  IsRetrievable = true},
                        new Field("dataType", DataType.String)         { IsKey = false, IsSearchable = false,  IsFilterable = false,  IsSortable = false,  IsFacetable = false, IsRetrievable = false},
                        new Field("rid", DataType.String)         { IsKey = false, IsSearchable = false,  IsFilterable = false, IsSortable = false, IsFacetable = false, IsRetrievable = true},
                        new Field("fileType", DataType.String)         { IsKey = false, IsSearchable = true,  IsFilterable = true, IsSortable = false, IsFacetable = true, IsRetrievable = true},
                        new Field("canPreview", DataType.Boolean)         { IsKey = false, IsSearchable = false,  IsFilterable = false, IsSortable = false, IsFacetable = false, IsRetrievable = true},
                    },
            };

            definition.TokenFilters = filters;
            definition.Tokenizers = tokenizers;
            definition.Analyzers = analyzers;
            var index = await client.Indexes.CreateAsync(definition).ConfigureAwait(false);
            return index;
        }

        private async Task<Index> CreateDatasetIndexAsync()
        {
            ISearchServiceClient client = await this.CreateClientAsync().ConfigureAwait(false);
            var tokenizers = new Tokenizer[] {
                    new MicrosoftLanguageStemmingTokenizer(
                        "customStemmingTokenizer",
                        maxTokenLength:30,
                        isSearchTokenizer: true,
                        language: MicrosoftStemmingTokenizerLanguage.English)};
            var filters = new TokenFilter[]{
                    new AsciiFoldingTokenFilter("customFoldingFilter", preserveOriginal: true),
                    new WordDelimiterTokenFilter(
                        "customWordFilter",
                        generateWordParts: true,
                        generateNumberParts: false,
                        catenateWords: false,
                        catenateNumbers: false,
                        catenateAll: false,
                        splitOnCaseChange:true,
                        preserveOriginal: true,
                        splitOnNumerics:true),
                    new StemmerTokenFilter("customStemmerFilter", StemmerTokenFilterLanguage.English),
                    // This appears to cause issues with long description fields:
                    //new EdgeNGramTokenFilterV2("customNGram", 1, 20)
                };

            var analyzers = new Analyzer[]{
                    new CustomAnalyzer(
                        "datasetAnalyzer",
                        TokenizerName.Create("customStemmingTokenizer"),
                        new List<TokenFilterName>(new [] {
                            TokenFilterName.Create("lowercase"),
                            TokenFilterName.Create("customFoldingFilter"),
                            TokenFilterName.Create("customWordFilter"),
                            //TokenFilterName.Create("customNGram")
                        }))};

            var defaultAnalyzer = AnalyzerName.Create("datasetAnalyzer");

            var definition = new Index()
            {
                Name = this.IndexOptions.DatasetIndex,
                Fields = new[]
                {
                        new Field("id", DataType.String)         { IsKey = true, IsSearchable = false,  IsFilterable = false,  IsSortable = false,  IsFacetable = false, IsRetrievable = true},
                        new Field("name", DataType.String)         { IsKey = false, IsSearchable = true,  IsFilterable = true,  IsSortable = true,  IsFacetable = false, IsRetrievable = true, IndexAnalyzer = defaultAnalyzer, SearchAnalyzer = defaultAnalyzer },
                        new Field("description", DataType.String)  { IsKey = false, IsSearchable = true,  IsFilterable = false,  IsSortable = false,  IsFacetable = false, IsRetrievable = true, IndexAnalyzer = defaultAnalyzer, SearchAnalyzer = defaultAnalyzer },
                        new Field("ownerName", DataType.String)    { IsKey = false, IsSearchable = true,  IsFilterable = true,  IsSortable = true,  IsFacetable = false, IsRetrievable = true, Analyzer = AnalyzerName.StandardAsciiFoldingLucene },
                        new Field("ownerId", DataType.String)         { IsKey = false, IsSearchable = false,  IsFilterable = true,  IsSortable = false,  IsFacetable = true, IsRetrievable = true},
                        new Field("published", DataType.DateTimeOffset)          { IsKey = false, IsSearchable = false, IsFilterable = true,  IsSortable = true,  IsFacetable = false,  IsRetrievable = true},
                        new Field("created", DataType.DateTimeOffset)          { IsKey = false, IsSearchable = false, IsFilterable = true,  IsSortable = true,  IsFacetable = false,  IsRetrievable = true},
                        new Field("modified", DataType.DateTimeOffset)  { IsKey = false, IsSearchable = false, IsFilterable = true,  IsSortable = true,  IsFacetable = false,  IsRetrievable = true},
                        new Field("license", DataType.String){ IsKey = false, IsSearchable = false,  IsFilterable = true,  IsSortable = true,  IsFacetable = false, IsRetrievable = true },
                        new Field("licenseId", DataType.String) { IsKey = false, IsSearchable = false,  IsFilterable = true,  IsSortable = false,  IsFacetable = true, IsRetrievable = true},
                        new Field("tags", DataType.Collection(DataType.String)) { IsKey = false, IsSearchable = true, IsFilterable = true,  IsSortable = false,  IsFacetable = true,  IsRetrievable = true, Analyzer = AnalyzerName.StandardAsciiFoldingLucene},
                        new Field("fileTypes", DataType.Collection(DataType.String)){ IsKey = false, IsSearchable = false, IsFilterable = true,  IsSortable = false,  IsFacetable = true,  IsRetrievable = true},
                        new Field("dataType", DataType.String)         { IsKey = false, IsSearchable = false,  IsFilterable = false,  IsSortable = false,  IsFacetable = false, IsRetrievable = false},
                        new Field("rid", DataType.String)         { IsKey = false, IsSearchable = false,  IsFilterable = false, IsSortable = false, IsFacetable = false, IsRetrievable = true},
                        new Field("fileCount", DataType.Int64)         { IsKey = false, IsSearchable = false,  IsFilterable = true, IsSortable = false, IsFacetable = false, IsRetrievable = true},
                        new Field("size", DataType.Int64)         { IsKey = false, IsSearchable = false,  IsFilterable = true, IsSortable = false, IsFacetable = false, IsRetrievable = true},
                        new Field("domainId", DataType.String)         { IsKey = false, IsSearchable = true,  IsFilterable = true,  IsSortable = false,  IsFacetable = true, IsRetrievable = true },
                        new Field("domain", DataType.String)         { IsKey = false, IsSearchable = false,  IsFilterable = true,  IsSortable = true,  IsFacetable = false, IsRetrievable = true },
                        new Field("isCompressedAvailable", DataType.Boolean)         { IsKey = false, IsSearchable = false,  IsFilterable = true,  IsSortable = true,  IsFacetable = false, IsRetrievable = true },
                        new Field("isDownloadAllowed", DataType.Boolean)         { IsKey = false, IsSearchable = false,  IsFilterable = true,  IsSortable = true,  IsFacetable = false, IsRetrievable = true },
                        new Field("isFeatured", DataType.Boolean)         { IsKey = false, IsSearchable = false,  IsFilterable = true,  IsSortable = true,  IsFacetable = false, IsRetrievable = true }
                    }
            };

            definition.TokenFilters = filters;
            definition.Tokenizers = tokenizers;
            definition.Analyzers = analyzers;

            ScoringProfile scoringProfile = new ScoringProfile();
            scoringProfile.Name = "textBoostScoring";
            scoringProfile.Functions = new List<ScoringFunction>();
            scoringProfile.TextWeights = new TextWeights(new Dictionary<string, double>());
            scoringProfile.TextWeights.Weights.Add("tags", 9);
            scoringProfile.TextWeights.Weights.Add("name", 6.5);
            scoringProfile.TextWeights.Weights.Add("description", 3);
            definition.ScoringProfiles = new List<ScoringProfile>();

            definition.ScoringProfiles.Add(scoringProfile);
            var index = await client.Indexes.CreateAsync(definition).ConfigureAwait(false);
            return index;
        }

        private async Task<Index> CreateNominationIndexAsync()
        {
            ISearchServiceClient client = await this.CreateClientAsync().ConfigureAwait(false);
            var tokenizers = new Tokenizer[] {
                    new MicrosoftLanguageStemmingTokenizer(
                        "customStemmingTokenizer",
                        maxTokenLength:30,
                        isSearchTokenizer: true,
                        language: MicrosoftStemmingTokenizerLanguage.English)};
            var filters = new TokenFilter[]{
                    new AsciiFoldingTokenFilter("customFoldingFilter", preserveOriginal: true),
                    new WordDelimiterTokenFilter(
                        "customWordFilter",
                        generateWordParts: true,
                        generateNumberParts: false,
                        catenateWords: false,
                        catenateNumbers: false,
                        catenateAll: false,
                        splitOnCaseChange:true,
                        preserveOriginal: true,
                        splitOnNumerics:true),
                    new StemmerTokenFilter("customStemmerFilter", StemmerTokenFilterLanguage.English),
                    // This appears to cause issues with long description fields:
                    //new EdgeNGramTokenFilterV2("customNGram", 1, 20)
                };

            var analyzers = new Analyzer[]{
                    new CustomAnalyzer(
                        "nominationAnalyzer",
                        TokenizerName.Create("customStemmingTokenizer"),
                        new List<TokenFilterName>(new [] {
                            TokenFilterName.Create("lowercase"),
                            TokenFilterName.Create("customFoldingFilter"),
                            TokenFilterName.Create("customWordFilter"),
                            //TokenFilterName.Create("customNGram")
                        }))};

            var defaultAnalyzer = AnalyzerName.Create("nominationAnalyzer");

            var definition = new Index()
            {
                Name = this.IndexOptions.NominationIndex,
                Fields = new[]
                {
                        new Field("id", DataType.String)         { IsKey = true, IsSearchable = false,  IsFilterable = false,  IsSortable = false,  IsFacetable = false, IsRetrievable = true},
                        new Field("name", DataType.String)         { IsKey = false, IsSearchable = true,  IsFilterable = true,  IsSortable = true,  IsFacetable = false, IsRetrievable = true, IndexAnalyzer = defaultAnalyzer, SearchAnalyzer = defaultAnalyzer },
                        new Field("description", DataType.String)  { IsKey = false, IsSearchable = true,  IsFilterable = false,  IsSortable = false,  IsFacetable = false, IsRetrievable = true, IndexAnalyzer = defaultAnalyzer, SearchAnalyzer = defaultAnalyzer },
                        new Field("published", DataType.DateTimeOffset)          { IsKey = false, IsSearchable = false, IsFilterable = true,  IsSortable = true,  IsFacetable = false,  IsRetrievable = true},
                        new Field("created", DataType.DateTimeOffset)          { IsKey = false, IsSearchable = false, IsFilterable = true,  IsSortable = true,  IsFacetable = false,  IsRetrievable = true},
                        new Field("modified", DataType.DateTimeOffset)  { IsKey = false, IsSearchable = false, IsFilterable = true,  IsSortable = true,  IsFacetable = false,  IsRetrievable = true},
                        new Field("license", DataType.String){ IsKey = false, IsSearchable = false,  IsFilterable = true,  IsSortable = true,  IsFacetable = false, IsRetrievable = true },
                        new Field("licenseId", DataType.String) { IsKey = false, IsSearchable = false,  IsFilterable = true,  IsSortable = false,  IsFacetable = true, IsRetrievable = true},
                        new Field("tags", DataType.Collection(DataType.String)) { IsKey = false, IsSearchable = true, IsFilterable = true,  IsSortable = false,  IsFacetable = true,  IsRetrievable = true, Analyzer = AnalyzerName.StandardAsciiFoldingLucene},
                        new Field("dataType", DataType.String)         { IsKey = false, IsSearchable = false,  IsFilterable = false,  IsSortable = false,  IsFacetable = false, IsRetrievable = false},
                        new Field("domain", DataType.String)         { IsKey = false, IsSearchable = true,  IsFilterable = true,  IsSortable = false,  IsFacetable = true, IsRetrievable = true },
                        new Field("nominationStatus", DataType.String)         { IsKey = false, IsSearchable = true,  IsFilterable = true,  IsSortable = false,  IsFacetable = true, IsRetrievable = true },
                        new Field("timeStamp", DataType.Int64)         { IsKey = false, IsSearchable = false,  IsFilterable = false,  IsSortable = true,  IsFacetable = false, IsRetrievable = true },
                }
            };

            definition.TokenFilters = filters;
            definition.Tokenizers = tokenizers;
            definition.Analyzers = analyzers;

            ScoringProfile scoringProfile = new ScoringProfile();
            scoringProfile.Name = "textBoostScoring";
            scoringProfile.Functions = new List<ScoringFunction>();
            scoringProfile.TextWeights = new TextWeights(new Dictionary<string, double>());
            scoringProfile.TextWeights.Weights.Add("tags", 9);
            scoringProfile.TextWeights.Weights.Add("name", 6.5);
            scoringProfile.TextWeights.Weights.Add("description", 3);
            definition.ScoringProfiles = new List<ScoringProfile>();

            definition.ScoringProfiles.Add(scoringProfile);
            var index = await client.Indexes.CreateAsync(definition).ConfigureAwait(false);
            return index;
        }

        private async Task<Indexer> CreateIndexerAsync(string indexer, string index, string datasource, ICollection<FieldMapping> fieldMappings = null)
        {
            var client = await this.CreateClientAsync().ConfigureAwait(false);
            string description = $"Populates {index} from {datasource}";

            var definition = new Indexer()
            {
                Name = indexer,
                Description = description,
                DataSourceName = datasource,
                TargetIndexName = index,
                Schedule = new IndexingSchedule(TimeSpan.FromHours(1)),
                FieldMappings = (fieldMappings ?? Enumerable.Empty<FieldMapping>()).ToList(),
                Parameters = new IndexingParameters
                {
                    Configuration = new Dictionary<string, object>
                    {
                        { "assumeOrderByHighWaterMarkColumn", true }
                    }
                }
            };

            var result = await client.Indexers.CreateAsync(definition).ConfigureAwait(false);
            await client.Indexers.RunAsync(indexer);
            return result;
        }
    }
}
