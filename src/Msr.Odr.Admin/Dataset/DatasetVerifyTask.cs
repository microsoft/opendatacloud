// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Msr.Odr.Admin.Commands.Options;
using Msr.Odr.Model.Configuration;
using Msr.Odr.Model.Datasets;
using Msr.Odr.Model.UserData;
using Newtonsoft.Json;

namespace Msr.Odr.Admin.Dataset
{
    public class DatasetVerifyTask
    {
        public CosmosOptions CosmosOptions { get; }
        public StorageOptions StorageOptions { get; }
        public SearchOptions SearchOptions { get; }
        public IndexOptions IndexOptions { get; }
        public bool FixDatasets { get; }

        public DatasetVerifyTask(
            CosmosOptions cosmosOptions,
            StorageOptions storageOptions,
            SearchOptions searchOptions,
            IndexOptions indexOptions,
            bool fixDatasets)
        {
            CosmosOptions = cosmosOptions;
            StorageOptions = storageOptions;
            SearchOptions = searchOptions;
            IndexOptions = indexOptions;
            FixDatasets = fixDatasets;
        }

        public async Task<int> ExecuteAsync()
        {
            Console.WriteLine($"Opening {CosmosOptions.Endpoint} CosmosDB");

            var storageCredentials = new StorageCredentials(StorageOptions.Account, StorageOptions.Key);
            var storageAccount = new CloudStorageAccount(storageCredentials, true);
            var blobClient = storageAccount.CreateCloudBlobClient();

            var indexClient = new SearchIndexClient(
                SearchOptions.Name,
                IndexOptions.DatasetIndex,
                new SearchCredentials(SearchOptions.Key));

            using (var client =
                new DocumentClient(
                    new Uri($"https://{CosmosOptions.Endpoint}.documents.azure.com/"),
                    CosmosOptions.RawKey,
                    new ConnectionPolicy
                    {
                        ConnectionMode = ConnectionMode.Direct,
                        ConnectionProtocol = Protocol.Tcp
                    },
                    ConsistencyLevel.Session))
            {
                await client.OpenAsync();

                var queryText = "SELECT c.datasetId, c.name FROM c WHERE c.dataType = 'dataset'";
                var queryable = client
                    .CreateDocumentQuery<DatasetEntry>(
                        UriFactory.CreateDocumentCollectionUri(CosmosOptions.Database, CosmosOptions.DatasetsCollection),
                        queryText,
                        new FeedOptions
                        {
                            EnableCrossPartitionQuery = true,
                            MaxItemCount = -1
                        })
                    .AsDocumentQuery();

                var entries = new List<DatasetEntry>();
                while (queryable.HasMoreResults)
                {
                    var response = await queryable.ExecuteNextAsync<DatasetEntry>();
                    entries.AddRange(response);
                }

                int errorCount = 0;

                foreach(var entry in entries)
                {
                    bool hasErrors = false;
                    Console.WriteLine($"{entry.DatasetId} - {entry.Name}");

                    var indexDoc = await indexClient.Documents.GetAsync<DatasetStorageItem>(entry.DatasetId);
                    if(indexDoc == null || indexDoc.Id.ToString() != entry.DatasetId || indexDoc.Name != entry.Name)
                    {
                        Console.Error.WriteLine($"Search index document not found.");
                        hasErrors = true;
                    }

                    var options = new RequestOptions
                    {
                        PartitionKey = new PartitionKey(entry.DatasetId)
                    };
                    var documentLink = UriFactory.CreateAttachmentUri(
                        CosmosOptions.Database,
                        CosmosOptions.DatasetsCollection,
                        entry.DatasetId,
                        "Content");
                    var response = await client.ReadAttachmentAsync(documentLink, options);
                    var resource = response?.Resource;
                    if (resource == null)
                    {
                        Console.Error.WriteLine($"Attachment not found.");
                        ++errorCount;
                        continue;
                    }

                    var storageType = resource.GetPropertyValue<string>("storageType") ?? string.Empty;
                    if (storageType != "blob")
                    {
                        Console.Error.WriteLine($"Unknown storage type, \"{storageType}\", for dataset.");
                        ++errorCount;
                        continue;
                    }

                    var account = resource.GetPropertyValue<string>("account");
                    var container = resource.GetPropertyValue<string>("container");
                    Console.WriteLine($"  Account:   {account}");
                    Console.WriteLine($"  Container: {container}");
                    Console.WriteLine($"  MediaLink: {resource.MediaLink}");

                    var containerNames = new HashSet<string>();

                    if (string.IsNullOrWhiteSpace(account) || account != StorageOptions.Account)
                    {
                        Console.Error.WriteLine($"Invalid account.");
                        hasErrors = true;
                    }

                    if (string.IsNullOrWhiteSpace(container) || containerNames.Contains(container))
                    {
                        Console.Error.WriteLine($"Invalid container.");
                        hasErrors = true;
                    }
                    containerNames.Add(container);

                    var containerRef = blobClient.GetContainerReference(container);
                    bool exists = await containerRef.ExistsAsync();
                    if(!exists)
                    {
                        Console.Error.WriteLine($"Associated container does not exist.");
                        hasErrors = true;
                    }

                    var containerUrl = containerRef.Uri.ToString();
                    if (string.IsNullOrWhiteSpace(resource.MediaLink) || resource.MediaLink != containerUrl)
                    {
                        Console.Error.WriteLine($"Invalid MediaLink.");
                        Console.Error.WriteLine($"Expected {containerUrl}");
                        hasErrors = true;
                    }

                    if(hasErrors)
                    {
                        if (FixDatasets && exists)
                        {
                            var datasetRecordLink = new Attachment
                            {
                                Id = "Content",
                                ContentType = "x-azure-blockstorage",
                                MediaLink = containerUrl,
                            };

                            datasetRecordLink.SetPropertyValue("storageType", "blob");
                            datasetRecordLink.SetPropertyValue("container", container);
                            datasetRecordLink.SetPropertyValue("account", StorageOptions.Account);

                            var uri = UriFactory.CreateDocumentUri(
                                CosmosOptions.Database,
                                CosmosOptions.DatasetsCollection,
                                entry.DatasetId);
                            await client.UpsertAttachmentAsync(uri, datasetRecordLink, options);
                        }
                        else
                        {
                            ++errorCount;
                        }
                    }
                }

                if (errorCount == 0)
                {
                    Console.WriteLine("All datasets verified.");
                }
                else
                {
                    Console.Error.WriteLine($"{errorCount} ERRORS FOUND.");
                }
            }

            return 0;
        }

        public class DatasetEntry
        {
            public string DatasetId { get; set; }
            public string Name { get; set; }
        }
    }
}
