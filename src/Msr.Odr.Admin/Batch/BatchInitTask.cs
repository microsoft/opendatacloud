using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Batch;
using Microsoft.Azure.Batch.Auth;
using Microsoft.Azure.Batch.Conventions.Files;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Msr.Odr.Admin.Commands.Options;
using Newtonsoft.Json;

namespace Msr.Odr.Admin.Batch
{
    public class BatchInitTask
    {
        public BatchOptions BatchOptions { get; }
        public SearchOptions SearchOptions { get; }
        public CosmosOptions CosmosOptions { get; }
        public StorageOptions StorageOptions { get; }
        public VaultOptions VaultOptions { get; }

        public BatchInitTask(
            BatchOptions batchOptions,
            SearchOptions searchOptions,
            CosmosOptions cosmosOptions,
            StorageOptions storageOptions,
            VaultOptions vaultOptions)
        {
            BatchOptions = batchOptions;
            SearchOptions = searchOptions;
            CosmosOptions = cosmosOptions;
            StorageOptions = storageOptions;
            VaultOptions = vaultOptions;
        }

        public async Task<int> ExecuteAsync()
        {
            Console.WriteLine("Initializing Azure Batch configuration.");

            var batchCreds = new BatchSharedKeyCredentials(
                BatchOptions.Url,
                BatchOptions.Account,
                BatchOptions.Key);
            using (var batchClient = BatchClient.Open(batchCreds))
            {
                (string key, string value)[] environmentValues = {

                    ("Batch:Account", BatchOptions.Account),
                    ("Batch:Url", BatchOptions.Url),
                    ("Batch:Key", BatchOptions.Key),
                    ("Batch:StorageName", BatchOptions.StorageName),
                    ("Batch:StorageKey", BatchOptions.StorageKey),

                    ("Search:Account", SearchOptions.Name),
                    ("Search:Key", SearchOptions.Key),
                    ("Search:DatasetIndex", "dataset-ix"),
                    ("Search:FileIndex", "files-ix"),

                    ("Documents:Account", CosmosOptions.Endpoint),
                    ("Documents:Key", CosmosOptions.RawKey),
                    ("Documents:Database", CosmosOptions.Database),
                    ("Documents:DatasetCollection", CosmosOptions.DatasetsCollection),
                    ("Documents:UserDataCollection", CosmosOptions.UserDataCollection),

                    ($"Storage:Accounts:{StorageOptions.Account}", StorageOptions.Key),
                };

                var job = batchClient.JobOperations.CreateJob();
                job.Id = "DatasetJob";
                job.DisplayName = "Various Dataset Operations Job";
                job.PoolInformation = new PoolInformation
                {
                    PoolId = "DatasetPool"
                };
                job.CommonEnvironmentSettings = environmentValues
                    .Select(t => new EnvironmentSetting(t.key, t.value))
                    .ToList();
                await job.CommitAsync().ConfigureAwait(false);

                var storageCreds = new StorageCredentials(BatchOptions.StorageName, BatchOptions.StorageKey);
                var linkedStorageAccount = new CloudStorageAccount(storageCreds, true);
                await job.PrepareOutputStorageAsync(linkedStorageAccount);
            }

            return 0;
        }
    }
}
