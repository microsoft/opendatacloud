using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Msr.Odr.Batch.Shared;
using Msr.Odr.Services;
using Msr.Odr.Services.Configuration;

namespace Msr.Odr.Batch.CompressDatasetApp
{
    public partial class CompressDatasetTask
    {
        private CompressDatasetTaskOptions Options { get; set; }
        public StorageConfiguration StorageConfig { get; }
        public DatasetStorageService DatasetStorage { get; }
        private Guid DatasetId => Options.DatasetId;

        private BatchLogger Log => BatchLogger.Instance;
        public CompressDatasetTask(
            IOptions<StorageConfiguration> storageConfig,
            DatasetStorageService datasetStorage)
        {
            StorageConfig = storageConfig.Value;
            DatasetStorage = datasetStorage;
        }

        public async Task<int> Run(CompressDatasetTaskOptions options)
        {
            var cancellationToken = CancellationToken.None;
            Options = options;
            Log
                .Add("Running compress dataset task ...")
                .Add($"Dataset Id: {Options.DatasetId}");

            await Compress(cancellationToken);

            return 0;
        }
    }
}
