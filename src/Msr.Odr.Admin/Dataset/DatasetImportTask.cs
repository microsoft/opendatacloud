// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.Batch;
using Microsoft.Azure.Batch.Auth;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Msr.Odr.Admin.Commands.Options;
using Msr.Odr.Model.Configuration;
using Msr.Odr.Model.Datasets;
using Msr.Odr.Model.UserData;
using Msr.Odr.Services.Batch;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Msr.Odr.Admin.Dataset
{
    public class DatasetImportTask
    {
        public CosmosOptions CosmosOptions { get; }
        public StorageOptions StorageOptions { get; }
        public ContactInfoOptions ContactOptions { get; }
        public string DatasetUrl { get; }
        public string StorageName { get; }

        public DatasetImportTask(
            CosmosOptions cosmosOptions,
            StorageOptions storageOptions,
            ContactInfoOptions contactOptions,
            string datasetUrl,
            string storageName)
        {
            CosmosOptions = cosmosOptions;
            StorageOptions = storageOptions;
            ContactOptions = contactOptions;
            DatasetUrl = datasetUrl;
            StorageName = storageName;
        }

        public async Task<int> ExecuteAsync()
        {
            Console.WriteLine($"Importing {DatasetUrl}");

            using(var httpClient = new HttpClient())
            {
                var result = await httpClient.GetAsync(DatasetUrl);
                result.EnsureSuccessStatusCode();
                var dataset = JsonConvert.DeserializeObject<DatasetStorageItem>(await result.Content.ReadAsStringAsync());

                Console.WriteLine($"Found Doc: {dataset.Id} - {dataset.Name}");

                var storageCredentials = new StorageCredentials(StorageOptions.Account, StorageOptions.Key);
                var storageAccount = new CloudStorageAccount(storageCredentials, true);
                var blobClient = storageAccount.CreateCloudBlobClient();

                var containerRef = blobClient.GetContainerReference(StorageName);

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

                    var license = await client.ReadDocumentAsync<LicenseStorageItem>(
                        UriFactory.CreateDocumentUri(CosmosOptions.Database, CosmosOptions.DatasetsCollection, dataset.LicenseId.ToString()),
                        new RequestOptions
                        {
                            PartitionKey = new PartitionKey(WellKnownIds.LicenseDatasetId.ToString())
                        });

                    // Attachment details
                    var blob = new
                    {
                        StorageType = "blob",
                        StorageOptions.Account,
                        Container = StorageName,
                        MediaLink = containerRef.Uri.ToString(),
                    };

                    // Convert Dataset to Nomination
                    var nomination = DatasetConvert.DatasetToNomination(dataset, ContactOptions, license);
                    Console.WriteLine(JsonConvert.SerializeObject(dataset, Formatting.Indented));
                    Console.WriteLine(JsonConvert.SerializeObject(nomination, Formatting.Indented));
                    Console.WriteLine(JsonConvert.SerializeObject(blob, Formatting.Indented));

                    // Add the nomination document
                    var uri = UriFactory.CreateDocumentCollectionUri(
                        CosmosOptions.Database,
                        CosmosOptions.UserDataCollection);
                    var options = new RequestOptions
                    {
                        PartitionKey = new PartitionKey(nomination.DatasetId.ToString())
                    };
                    var newDoc = await client.UpsertDocumentAsync(uri, nomination, options);

                    // Add the attachment to the nomination (with blob storage details)
                    var datasetRecordLink = new Attachment
                    {
                        Id = "Content",
                        ContentType = "x-azure-blockstorage",
                        MediaLink = blob.MediaLink,
                    };
                    datasetRecordLink.SetPropertyValue("storageType", "blob");
                    datasetRecordLink.SetPropertyValue("container", blob.Container);
                    datasetRecordLink.SetPropertyValue("account", blob.Account);
                    await client.UpsertAttachmentAsync(newDoc.Resource.SelfLink, datasetRecordLink, options);

                    Console.WriteLine($"Added nomination document.");
                }
            }

            return 0;
        }
    }
}
