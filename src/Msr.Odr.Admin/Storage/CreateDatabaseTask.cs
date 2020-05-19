using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Msr.Odr.Admin.Commands.Options;

namespace Msr.Odr.Admin.Storage
{
	/// <summary>
	/// Task to configure the initial database
	/// </summary>
	public class CreateDatabaseTask : StorageServiceTask
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="CreateDatabaseTask" /> class.
		/// </summary>
		/// <param name="cosmos">The Cosmos DB configuration.</param>
		public CreateDatabaseTask(CosmosOptions cosmos)
			: base (cosmos)
		{
		}
		
		/// <summary>
		/// Executes the task asynchronously
		/// </summary>
		/// <returns>The status code</returns>
		public override async Task<int> ExecuteAsync()
		{
			var client = await this.CreateClientAsync().ConfigureAwait(false);
			var dbRsp = await client.CreateDatabaseIfNotExistsAsync(new Database { Id = this.Database }).ConfigureAwait(false);
            var db = dbRsp.Resource;

            await CreateDatasetsCollection(client, db);
            await CreateUserDataCollection(client, db);

			return 0;
		}

        private async Task CreateDatasetsCollection(DocumentClient client, Database db)
        {
            DocumentCollection collectionDefinition = new DocumentCollection();
            collectionDefinition.Id = this.CosmosOptions.DatasetsCollection;
            collectionDefinition.PartitionKey.Paths.Add("/datasetId");
            collectionDefinition.IndexingPolicy.IndexingMode = IndexingMode.Lazy;

            var collection = await client.CreateDocumentCollectionIfNotExistsAsync(
                UriFactory.CreateDatabaseUri(db.Id),
                collectionDefinition,
                new RequestOptions { OfferThroughput = 400 });
        }

        private async Task CreateUserDataCollection(DocumentClient client, Database db)
        {
            DocumentCollection collectionDefinition = new DocumentCollection();
            collectionDefinition.Id = this.CosmosOptions.UserDataCollection;
            collectionDefinition.PartitionKey.Paths.Add("/datasetId");
            collectionDefinition.IndexingPolicy.IndexingMode = IndexingMode.Lazy;

            var collection = await client.CreateDocumentCollectionIfNotExistsAsync(
                UriFactory.CreateDatabaseUri(db.Id),
                collectionDefinition,
                new RequestOptions { OfferThroughput = 400 });
        }
    }
}
