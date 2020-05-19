using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.Azure.Batch;
using Microsoft.Azure.Batch.Auth;
using Microsoft.Extensions.Options;
using Msr.Odr.Model;
using Msr.Odr.Services.Configuration;
using Microsoft.Azure.Batch.Conventions.Files;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;

namespace Msr.Odr.Services.Batch
{
    public class ApplicationJobs
    {
        private const int BatchOperationsWindowDays = 60;

        private BatchConfiguration Batch { get; }

        private readonly Lazy<BatchClient> _lazyBatchClient;

        private BatchClient BatchClient => _lazyBatchClient.Value;

        public ApplicationJobs(IOptions<BatchConfiguration> batchConfig)
        {
            Batch = batchConfig.Value;
            _lazyBatchClient = new Lazy<BatchClient>(() =>
            {
                var batchCreds = new BatchSharedKeyCredentials(Batch.Url, Batch.Account, Batch.Key);
                return BatchClient.Open(batchCreds);
            }, true);
        }

        public async Task StartDatasetImportJob(Guid nominationId, string nominationName)
        {
            var taskName = GetUniqueTaskName(BatchConstants.ImportDatasetAppName);
            var commandLine = GetCommandLine(
                BatchConstants.ImportDatasetAppName,
                BatchConstants.ImportDatasetExeName,
                nominationId.ToString());
            var task = new CloudTask(taskName, commandLine)
            {
                DisplayName = $"Importing: {nominationName} ({nominationId})"
            };
            await StartJob(task);
        }

        public async Task StartDatasetCompressionJob(Guid datasetId, string datasetName)
        {
            var taskName = GetUniqueTaskName(BatchConstants.CompressDatasetAppName);
            var commandLine = GetCommandLine(
                BatchConstants.CompressDatasetAppName,
                BatchConstants.CompressDatasetExeName,
                datasetId.ToString());
            var task = new CloudTask(taskName, commandLine)
            {
                DisplayName = $"Compressing: {datasetName} ({datasetId})"
            };
            await StartJob(task);
        }

        public async Task<BatchOperations> GetLatestTasks(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var taskCounts = await BatchClient.JobOperations.GetJobTaskCountsAsync(BatchConstants.DatasetJobId, null, cancellationToken);

            var fromDate = DateTime.UtcNow.Date.AddDays(-BatchOperationsWindowDays);
            var detailLevel = new ODATADetailLevel
            {
                FilterClause = $"creationTime gt DateTime'{fromDate:s}'",
                SelectClause = "id,displayName,state,creationTime",
            };
            var tasks = await BatchClient.JobOperations
                .ListTasks(BatchConstants.DatasetJobId, detailLevel)
                .ToListAsync(cancellationToken);

            return new BatchOperations
            {
                Status = new BatchOperationsSystemStatus
                {
                    Active = taskCounts.Active,
                    Completed = taskCounts.Completed,
                    Failed = taskCounts.Failed,
                    Running = taskCounts.Running,
                    Succeeded = taskCounts.Succeeded,
                    ValidationStatus = taskCounts.ValidationStatus.ToString(),
                },
                PageNumber = 1,
                PageSize = 0,
                Operations = tasks
                    .OrderByDescending(task => task.CreationTime)
                    .ThenBy(task => task.DisplayName)
                    .Select(task => new BatchOperation
                    {
                        Id = task.Id,
                        DisplayName = task.DisplayName,
                        State = task.State == null ? string.Empty : task.State.Value.ToString(),
                        CreationTime = task.CreationTime ?? DateTime.UtcNow,
                    })
                    .ToList()
            };
        }

        public async Task<BatchOperationOutput> GetTaskOutput(string taskId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var task = await BatchClient.JobOperations.GetTaskAsync(BatchConstants.DatasetJobId, taskId, null, null, cancellationToken);

            var storageCreds = new StorageCredentials(Batch.StorageName, Batch.StorageKey);
            var linkedStorageAccount = new CloudStorageAccount(storageCreds, true);
            var outputReference = task.OutputStorage(linkedStorageAccount)
                .ListOutputs(TaskOutputKind.TaskLog)
                .FirstOrDefault();

            string outputText = "[No Output]";
            if (outputReference != null)
            {
                using (var readStream = await outputReference.OpenReadAsync(cancellationToken))
                using (var streamReader = new StreamReader(readStream, System.Text.Encoding.UTF8))
                {
                    outputText = streamReader.ReadToEnd();
                }
            }

            return new BatchOperationOutput
            {
                Text = outputText
            };
        }

        private async Task StartJob(CloudTask task)
        {
            await BatchClient.JobOperations.AddTaskAsync(BatchConstants.DatasetJobId, task);
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
