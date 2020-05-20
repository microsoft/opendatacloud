using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Msr.Odr.Batch.Shared;
using Msr.Odr.Services;
using Msr.Odr.Services.Batch;
using Msr.Odr.Services.Configuration;

namespace Msr.Odr.Batch.ImportDatasetApp
{
    public partial class ImportTask
    {
        const long MaxFileSizeForCompression = 2L * 1024 * 1024 * 1024; // 2GB

        public ApplicationJobs ApplicationJobs { get; }
        public StorageConfiguration StorageConfig { get; }
        public DatasetStorageService DatasetStorage { get; }
        public DatasetEditStorageService DatasetEditStorage { get; }
        public UserDataStorageService UserDataStorage { get; }
        public DatasetSearchService DatasetSearch { get; }
        public FileSearchService FileSearch { get; }
        private ImportTaskOptions Options { get; set; }

        private Guid DatasetId => Options.NominationId;

        private BatchLogger Log => BatchLogger.Instance;

        public ImportTask(
            ApplicationJobs applicationJobs,
            IOptions<StorageConfiguration> storageConfig,
            DatasetStorageService datasetStorage,
            DatasetEditStorageService datasetEditStorage,
            UserDataStorageService userDataStorage,
            DatasetSearchService datasetSearch,
            FileSearchService fileSearch)
        {
            ApplicationJobs = applicationJobs;
            StorageConfig = storageConfig.Value;
            DatasetStorage = datasetStorage;
            DatasetEditStorage = datasetEditStorage;
            UserDataStorage = userDataStorage;
            DatasetSearch = datasetSearch;
            FileSearch = fileSearch;
        }

        public async Task<int> Run(ImportTaskOptions options)
        {
            var cancellationToken = CancellationToken.None;
            Options = options;

            try
            {
                Log
                    .Add("Running dataset import task ...")
                    .Add($"Nomination Id: {Options.NominationId}");

                (var nomination, var storage) = await LoadNomination(cancellationToken);

                await DeleteDatasetDocuments(cancellationToken);

                var fileDetails = await CreateDatasetFileDocuments(storage, cancellationToken);

                await CreateDatasetDocument(nomination, storage, fileDetails, cancellationToken);

                await UpdateNominationStatusToCompleted(cancellationToken);

                if (BatchRunner.QueueJobs)
                {
                    if (fileDetails.fileSize < MaxFileSizeForCompression)
                    {
                        Log.Add("Starting up compression batch task.");
                        await ApplicationJobs.StartDatasetCompressionJob(Options.NominationId, nomination.Name);
                    }
                }

                var cleanedUp = await DatasetEditStorage.CleanUpDatasetEditAfterImport(Options.NominationId);
                if (cleanedUp)
                {
                    Log.Add("Cleaned up dataset edit resources.");
                }

                return 0;
            }
            catch(Exception)
            {
                await UpdateNominationStatusToError();
                throw;
            }
        }
    }
}
