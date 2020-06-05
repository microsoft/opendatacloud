// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.DataMovement;

namespace DatasetUtil.Components
{
    public class DatasetImport
    {
        private Logger Log => Logger.Instance;
        private ImportConfig Config => ImportConfig.Instance;

        public async Task<int> Run()
        {
            await ImportConfig.Load();

            var files = CountFiles(Config.SourcePath);

            if (!ConfirmUpload())
            {
                Log.Add("Dataset upload was abandoned.");
                return 1;
            }

            await UploadFiles(files);

            return 0;
        }

        private (int Count, long Size) CountFiles(string path)
        {
            Log.Add("Collecting files ...");
            var files = Directory
                .EnumerateFiles(path, "*.*", SearchOption.AllDirectories)
                //.Tap(name =>
                //{
                //    Log.Add($" - {name}");
                //})
                .Where(name => !Utils.IsConfigFile(name))
                .Select(name => new FileInfo(name))
                .Aggregate((Count: 0, Size: 0L), (agg, fileInfo) => (
                    Count: agg.Count + 1,
                    Size: agg.Size + fileInfo.Length
                ));

            int directoryCount = Directory
                .EnumerateDirectories(path, "*.*", SearchOption.AllDirectories)
                //.Tap(name =>
                //{
                //    Log.Add($" - [ {name} ]");
                //})
                .Aggregate(0, (count, name) => count + 1);

            Log.Add($"{files.Count:n0} files found in {directoryCount} directories ({files.Size:n0} total bytes).");
            return files;
        }

        private bool ConfirmUpload()
        {
            Console.WriteLine();
            Console.WriteLine("Transferring the dataset:");
            Console.WriteLine();
            Console.WriteLine($"     Id: {Config.Properties.Id}");
            Console.WriteLine($"   Name: {Config.Properties.DatasetName}");
            Console.WriteLine($"   From: {Config.SourcePath}");
            Console.WriteLine($"     To: {Config.Properties.AccountName}/{Config.Properties.ContainerName} storage");
            Console.WriteLine();
            Console.Write("Proceed? [y/n] ");
            var response = Console.ReadLine();
            return response.IsMatchingText("y");
        }

        private async Task UploadFiles((int Count, long Size) files)
        {
            var container = new CloudBlobContainer(new Uri(Config.Properties.AccessToken));
            var blobDirectory = container.GetDirectoryReference("");
            var context = new DirectoryTransferContext
            {
                ShouldTransferCallback = (src, dst) =>
                {
                    var fileName = (string) src;
                    return !Utils.IsConfigFile(fileName);
                },
                ProgressHandler = new Progress<TransferStatus>((pg) =>
                {
                    var d = (
                        files: pg.NumberOfFilesTransferred + pg.NumberOfFilesSkipped + pg.NumberOfFilesFailed,
                        total: files.Count,
                        bytes: pg.BytesTransferred,
                        skipped: pg.NumberOfFilesSkipped,
                        errors: pg.NumberOfFilesFailed
                    );
                    Log.Status($"  ... {d.files} of {d.total} files, {d.bytes} bytes ({d.skipped} skipped, {d.errors} errors)");
                })
            };

            var cancellationSource = new CancellationTokenSource();
            Log.Add("Copying Files ...");

            var stopWatch = Stopwatch.StartNew();
            var options = new UploadDirectoryOptions
            {
                Recursive = true
            };

            var result = await TransferManager.UploadDirectoryAsync(
                Config.SourcePath,
                blobDirectory,
                options,
                context,
                cancellationSource.Token);
            context.ProgressHandler.Report(result);
            stopWatch.Stop();
            var stats = (
                seconds: stopWatch.Elapsed.TotalSeconds,
                files: result.NumberOfFilesTransferred
            );
            Log.Add().Add($"Copied {stats.files:n0} files in {stats.seconds:n2} seconds.");
        }
    }
}
