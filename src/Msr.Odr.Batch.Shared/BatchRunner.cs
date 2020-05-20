using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Batch.Conventions.Files;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Msr.Odr.Services.Configuration;

namespace Msr.Odr.Batch.Shared
{
    public static class BatchRunner
    {
        static BatchRunner()
        {
            StoreBatchOutput = true;
            ShowCurrentEnvironment = false;
            QueueJobs = true;
        }

        public static bool StoreBatchOutput { get; set; }

        public static bool ShowCurrentEnvironment { get; set; }

        public static bool QueueJobs { get; set; }

        private const string OutputFileName = "stdout.txt";

        private static Startup _startup = null;

        private static BatchLogger Log => BatchLogger.Instance;

        private static IServiceProvider ServiceProvider => _startup.ServiceProvider;

        public static async Task<int> Run<T>(Func<T, Task<int>> execTask)
            where T : class
        {
            try
            {
                return await ConfigureServices(async () =>
                {
                    var task = ActivatorUtilities.CreateInstance<T>(ServiceProvider);
                    return await execTask(task);
                });
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Error intializing batch task:");
                Log.InitializationError(ex);
                return 1;
            }
        }

        public static async Task<int> ConfigureServices(Func<Task<int>> batchOperation)
        {
            _startup = new Startup();
            _startup.ConfigureServices();
            return await StoreOutput(batchOperation);
        }

        public static async Task<int> StoreOutput(Func<Task<int>> batchOperation)
        {
            if (!StoreBatchOutput)
            {
                return await ExecTask(batchOperation);
            }

            var batch = ServiceProvider.GetService<IOptions<BatchConfiguration>>().Value;
            var stdoutFlushDelay = TimeSpan.FromSeconds(5);
            var stdoutFreqDelay = TimeSpan.FromSeconds(30);
            var logFilePath = Path.Combine(batch.TaskPath, OutputFileName);

            var credentials = new StorageCredentials(batch.StorageName, batch.StorageKey);
            var storageAccount = new CloudStorageAccount(credentials, true);
            var taskStorage = new TaskOutputStorage(storageAccount, batch.JobId, batch.TaskId);

            // The stdout.txt blob in Storage every stdoutFreqDelay (30 seconds) while the task code runs.
            using (await taskStorage.SaveTrackedAsync(TaskOutputKind.TaskLog, logFilePath, OutputFileName, stdoutFreqDelay))
            {
                int result = await ExecTask(batchOperation);
                await Task.Delay(stdoutFlushDelay);
                return result;
            }
        }

        public static async Task<int> ExecTask(Func<Task<int>> batchOperation)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            void StopTimer(bool isFinished)
            {
                stopWatch.Stop();
                string prefix = isFinished ? "Finished" : "Stopped";
                Log
                    .Add($"{prefix} on {DateTimeOffset.UtcNow:F}")
                    .Add($"Elapsed: {stopWatch.Elapsed:g}");
            }

            Log
                .Add("Running Batch Task")
                .Add(System.Reflection.Assembly.GetEntryAssembly().FullName)
                .Add($"Started on {DateTimeOffset.UtcNow:F}");

            if (ShowCurrentEnvironment)
            {
                BatchDiagnostics.DumpEnvironment();
            }

            try
            {
                int result = await batchOperation();
                StopTimer(true);
                return result;
            }
            catch (Exception ex)
            {
                StopTimer(false);
                Log.Add("Error running batch task:").BatchError(ex);
                return 1;
            }
        }
    }
}
