using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Batch;
using Microsoft.Azure.Batch.Auth;
using Microsoft.Azure.Batch.Conventions.Files;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.Rest;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Msr.Odr.Admin.Commands.Options;

namespace Msr.Odr.Admin.Nomination
{
    public class NominationDeleteTask
    {
        public SearchOptions SearchOptions { get; }
        public CosmosOptions CosmosOptions { get; }
        public string NominationId { get; }

        public NominationDeleteTask(
            SearchOptions searchOptions,
            CosmosOptions cosmosOptions,
            string nominationId)
        {
            SearchOptions = searchOptions;
            CosmosOptions = cosmosOptions;
            NominationId = nominationId;
        }

        public async Task<int> ExecuteAsync()
        {
            var datasetNominationDatasetId = Model.Configuration.WellKnownIds.DatasetNominationDatasetId;

            Console.WriteLine($"Deleting dataset nomination {NominationId}");

            using (var searchClient =
                new SearchIndexClient(
                    SearchOptions.Name,
                    "nomination-ix",
                    new SearchCredentials(SearchOptions.Key)))
            {
                var doc = await searchClient.Documents.GetAsync<DocumentInfo>(NominationId, new[] { "id", "name" });
                if (doc != null)
                {
                    Console.WriteLine($"Found Idx: {doc.Id} - {doc.Name}");
                    var batch = IndexBatch.Delete("id", new [] { NominationId });
                    await searchClient.Documents.IndexAsync(batch);
                }
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
                    CosmosOptions.UserDataCollection,
                    NominationId);
                var options = new RequestOptions
                {
                    PartitionKey = new PartitionKey(datasetNominationDatasetId.ToString())
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
