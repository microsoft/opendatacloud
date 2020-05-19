using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Batch;
using Microsoft.Azure.Batch.Auth;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Msr.Odr.Admin.Commands.Options;
using Msr.Odr.Model.Configuration;
using Msr.Odr.Model.Datasets;
using Msr.Odr.Model.UserData;
using Msr.Odr.Services.Batch;
using Newtonsoft.Json;

namespace Msr.Odr.Admin.Dataset
{
    public class DatasetNominateTask
    {
        public CosmosOptions CosmosOptions { get; }
        public ContactInfoOptions ContactOptions { get; }
        public BatchOptions BatchOptions { get; }
        public string DatasetId { get; }
        public bool QueueJob { get; }

        public DatasetNominateTask(
            CosmosOptions cosmosOptions,
            ContactInfoOptions contactOptions,
            BatchOptions batchOptions,
            string datasetId,
            bool queueJob)
        {
            CosmosOptions = cosmosOptions;
            ContactOptions = contactOptions;
            BatchOptions = batchOptions;
            DatasetId = datasetId;
            QueueJob = queueJob;
        }

        public async Task<int> ExecuteAsync()
        {
            Console.WriteLine($"Re-nominating dataset {DatasetId}");

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

                // Read Dataset Document

                var uri = UriFactory.CreateDocumentUri(
                    CosmosOptions.Database,
                    CosmosOptions.DatasetsCollection,
                    DatasetId);
                var options = new RequestOptions
                {
                    PartitionKey = new PartitionKey(DatasetId)
                };

                var doc = await client.ReadDocumentAsync<DatasetStorageItem>(uri, options);
                var dataset = doc.Document;
                Console.WriteLine($"Found Doc: {dataset.Id} - {dataset.Name}");

                var license = await client.ReadDocumentAsync<LicenseStorageItem>(
                    UriFactory.CreateDocumentUri(CosmosOptions.Database, CosmosOptions.DatasetsCollection, dataset.LicenseId.ToString()),
                    new RequestOptions
                    {
                        PartitionKey = new PartitionKey(WellKnownIds.LicenseDatasetId.ToString())
                    });

                // Read Dataset Document attachment (with blob storage details)

                var documentLink = UriFactory.CreateAttachmentUri(
                    CosmosOptions.Database,
                    CosmosOptions.DatasetsCollection,
                    DatasetId,
                    "Content");
                var response = await client.ReadAttachmentAsync(documentLink, options);
                var resource = response?.Resource;
                if (resource == null)
                {
                    throw new InvalidOperationException("Could not find storage information for dataset.");
                }

                var storageType = resource.GetPropertyValue<string>("storageType") ?? string.Empty;
                if (storageType != "blob")
                {
                    throw new InvalidOperationException($"Unknown storage type, \"{storageType}\", for dataset.");
                }
                var blob = new
                {
                    StorageType = storageType,
                    Account = resource.GetPropertyValue<string>("account"),
                    Container = resource.GetPropertyValue<string>("container"),
                    MediaLink = resource.MediaLink,
                };

                // Convert Dataset to Nomination with either PendingApproval or Importing status.
                var nomination = DatasetConvert.DatasetToNomination(dataset, ContactOptions, license);
                nomination.NominationStatus = QueueJob
                    ? NominationStatus.Importing
                    : NominationStatus.PendingApproval;
                Console.WriteLine(JsonConvert.SerializeObject(dataset, Formatting.Indented));
                Console.WriteLine(JsonConvert.SerializeObject(nomination, Formatting.Indented));
                Console.WriteLine(JsonConvert.SerializeObject(blob, Formatting.Indented));

                // Add the nomination document

                uri = UriFactory.CreateDocumentCollectionUri(
                    CosmosOptions.Database,
                    CosmosOptions.UserDataCollection);
                options = new RequestOptions
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

                if(QueueJob)
                {
                    // Add import job to queue
                    var batchCreds = new BatchSharedKeyCredentials(BatchOptions.Url, BatchOptions.Account, BatchOptions.Key);
                    using (var batchClient = BatchClient.Open(batchCreds))
                    {
                        var taskName = GetUniqueTaskName(BatchConstants.ImportDatasetAppName);
                        var commandLine = GetCommandLine(
                            BatchConstants.ImportDatasetAppName,
                            BatchConstants.ImportDatasetExeName,
                            nomination.Id.ToString());
                        var task = new CloudTask(taskName, commandLine)
                        {
                            DisplayName = $"Importing: {nomination.Name} ({nomination.Id})"
                        };

                        await batchClient.JobOperations.AddTaskAsync(BatchConstants.DatasetJobId, task);
                        Console.WriteLine($"Queued Job to import dataset.");
                    }
                }
            }

            return 0;
        }

        private string GetUniqueTaskName(string name)
        {
            return $"{name.ToLower()}-{Guid.NewGuid():N}";
        }

        private string GetCommandLine(string applicationId, string exeName, params string[] args)
        {
            string argList = string.Join(" ", args);
            string envVarName = $"AZ_BATCH_APP_PACKAGE_{applicationId.ToLower()}";
            return $"cmd /c \"%{envVarName}%\\{exeName} {argList}\"";
        }
    }
}
