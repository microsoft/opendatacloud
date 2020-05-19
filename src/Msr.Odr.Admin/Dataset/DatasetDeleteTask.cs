using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Msr.Odr.Admin.Commands.Options;

namespace Msr.Odr.Admin.Dataset
{
    public class DatasetDeleteTask
    {
        public SearchOptions SearchOptions { get; }
        public CosmosOptions CosmosOptions { get; }
        public string DatasetId { get; }

        public DatasetDeleteTask(
            SearchOptions searchOptions,
            CosmosOptions cosmosOptions,
            string datasetId)
        {
            SearchOptions = searchOptions;
            CosmosOptions = cosmosOptions;
            DatasetId = datasetId;
        }

        public async Task<int> ExecuteAsync()
        {
            Console.WriteLine($"Deleting dataset {DatasetId}");

            using (var searchClient =
                new SearchIndexClient(
                    SearchOptions.Name,
                    "dataset-ix",
                    new SearchCredentials(SearchOptions.Key)))
            {
                var doc = await searchClient.Documents.GetAsync<DocumentInfo>(DatasetId, new[] { "id", "name" });
                if (doc != null)
                {
                    Console.WriteLine($"Found Idx: {doc.Id} - {doc.Name}");
                    var batch = IndexBatch.Delete("id", new[] { DatasetId });
                    await searchClient.Documents.IndexAsync(batch);
                }
            }

            using (var searchClient =
                new SearchIndexClient(
                    SearchOptions.Name,
                    "files-ix",
                    new SearchCredentials(SearchOptions.Key)))
            {
                var count = 0;
                var parameters = new SearchParameters
                {
                    Filter = $"datasetId eq '{DatasetId}'",
                    Select = new[] { "id" },
                };
                var response = await searchClient.Documents.SearchAsync<DocumentInfo>(string.Empty, parameters);
                while (true)
                {
                    if (response.Results.Any())
                    {
                        var batch = IndexBatch.Delete("id", response.Results.Select(result => result.Document.Id));
                        await searchClient.Documents.IndexAsync(batch);
                        count += response.Results.Count();
                    }

                    if (response.ContinuationToken == null)
                    {
                        break;
                    }
                    else
                    {
                        response = await searchClient.Documents.ContinueSearchAsync<DocumentInfo>(response.ContinuationToken);
                    }
                }

                Console.WriteLine($"Found {count:n0} file index documents.");
            }

            using (var client =
                new DocumentClient(
                    new Uri($"https://{CosmosOptions.Endpoint}.documents.azure.com/"),
                    CosmosOptions.Key,
                    new ConnectionPolicy
                    {
                        ConnectionMode = ConnectionMode.Direct,
                        ConnectionProtocol = Protocol.Tcp
                    },
                    ConsistencyLevel.Session))
            {
                await client.OpenAsync();

                var uri = UriFactory.CreateDocumentUri(
                    CosmosOptions.Database,
                    CosmosOptions.DatasetsCollection,
                    DatasetId);
                var options = new RequestOptions
                {
                    PartitionKey = new PartitionKey(DatasetId)
                };

                var doc = await client.ReadDocumentAsync(uri, options);
                var id = doc.Resource.GetPropertyValue<string>("id");
                var name = doc.Resource.GetPropertyValue<string>("name");
                Console.WriteLine($"Found Doc: {id} - {name}");

                await client.DeleteDocumentAsync(uri, options);
            }

            return 0;
        }

        private class DocumentInfo
        {
            public string Id { get; set; }
            public string Name { get; set; }
        }
    }
}
