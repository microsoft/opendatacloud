// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
    public class DatasetViewTask
    {
        public CosmosOptions CosmosOptions { get; }
        public StorageOptions StorageOptions { get; }
        public string DatasetId { get; }

        public DatasetViewTask(
            CosmosOptions cosmosOptions,
            StorageOptions storageOptions,
            string datasetId)
        {
            CosmosOptions = cosmosOptions;
            StorageOptions = storageOptions;
            DatasetId = datasetId;
        }

        public async Task<int> ExecuteAsync()
        {
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

                Console.WriteLine("========DATASET========");
                var datasetUri = UriFactory.CreateDocumentCollectionUri(
                    CosmosOptions.Database,
                    CosmosOptions.DatasetsCollection);
                var dataset = (await client.CreateDocumentQuery(
                    datasetUri,
                    new FeedOptions
                    {
                        PartitionKey = new PartitionKey(DatasetId)
                    })
                    .Where(d => d.Id == DatasetId)
                    .AsDocumentQuery()
                    .GetQueryResultsAsync())
                    .SingleOrDefault();
                if(dataset == null)
                {
                    Console.WriteLine($"Dataset document {DatasetId} not found.");
                }
                else
                {
                    Console.WriteLine(dataset.ToString());
                    Console.WriteLine("========ATTACHMENT========");
                    try
                    {
                        var documentLink = UriFactory.CreateAttachmentUri(
                            CosmosOptions.Database,
                            CosmosOptions.DatasetsCollection,
                            DatasetId,
                            "Content");
                        var response = await client.ReadAttachmentAsync(documentLink, new RequestOptions
                        {
                            PartitionKey = new PartitionKey(DatasetId)
                        });
                        Console.WriteLine(response?.Resource?.ToString());
                    }
                    catch(DocumentClientException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
                    {
                        Console.WriteLine("None.");
                    }
                }

                Console.WriteLine("========DATASET-EDIT========");
                datasetUri = UriFactory.CreateDocumentCollectionUri(
                    CosmosOptions.Database,
                    CosmosOptions.UserDataCollection);
                dataset = (await client.CreateDocumentQuery(
                    datasetUri,
                    new FeedOptions
                    {
                        PartitionKey = new PartitionKey(WellKnownIds.DatasetEditDatasetId.ToString())
                    })
                    .Where(d => d.Id == DatasetId)
                    .AsDocumentQuery()
                    .GetQueryResultsAsync())
                    .SingleOrDefault();
                if(dataset == null)
                {
                    Console.WriteLine($"No Dataset Edit found for {DatasetId}.");
                }
                else
                {
                    Console.WriteLine(dataset.ToString());
                    Console.WriteLine("========ATTACHMENT========");
                    try
                    {
                        var documentLink = UriFactory.CreateAttachmentUri(
                            CosmosOptions.Database,
                            CosmosOptions.UserDataCollection,
                            DatasetId,
                            "Content");
                        var response = await client.ReadAttachmentAsync(documentLink, new RequestOptions
                        {
                            PartitionKey = new PartitionKey(WellKnownIds.DatasetEditDatasetId.ToString())
                        });
                        Console.WriteLine(response?.Resource?.ToString());
                    }
                    catch(DocumentClientException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
                    {
                        Console.WriteLine("None.");
                    }
                }

                Console.WriteLine("========NOMINATION========");
                datasetUri = UriFactory.CreateDocumentCollectionUri(
                    CosmosOptions.Database,
                    CosmosOptions.UserDataCollection);
                dataset = (await client.CreateDocumentQuery(
                    datasetUri,
                    new FeedOptions
                    {
                        PartitionKey = new PartitionKey(WellKnownIds.DatasetNominationDatasetId.ToString())
                    })
                    .Where(d => d.Id == DatasetId)
                    .AsDocumentQuery()
                    .GetQueryResultsAsync())
                    .SingleOrDefault();
                if(dataset == null)
                {
                    Console.WriteLine($"No Nomination found for {DatasetId}.");
                }
                else
                {
                    Console.WriteLine(dataset.ToString());
                    Console.WriteLine("========ATTACHMENT========");
                    try
                    {
                        var documentLink = UriFactory.CreateAttachmentUri(
                            CosmosOptions.Database,
                            CosmosOptions.UserDataCollection,
                            DatasetId,
                            "Content");
                        var response = await client.ReadAttachmentAsync(documentLink, new RequestOptions
                        {
                            PartitionKey = new PartitionKey(WellKnownIds.DatasetNominationDatasetId.ToString())
                        });
                        Console.WriteLine(response?.Resource?.ToString());
                    }
                    catch(DocumentClientException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
                    {
                        Console.WriteLine("None.");
                    }
                }
            }
            Console.WriteLine("================");

            return 0;
        }
    }
}
