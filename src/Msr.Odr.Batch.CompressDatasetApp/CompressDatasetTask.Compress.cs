// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Tar;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Msr.Odr.Batch.Shared;
using Msr.Odr.Model.Datasets;
using Msr.Odr.Services;

namespace Msr.Odr.Batch.CompressDatasetApp
{
    public partial class CompressDatasetTask
    {
        private const int MaxContainerNameLength = 60;

        private async Task Compress(CancellationToken cancellationToken)
        {
            var result = await DatasetStorage.GetDatasetStorageDetails(DatasetId, cancellationToken);
            if (result is DatasetBlobStorageDetails storage)
            {
                await ReadDatasetFiles(storage, cancellationToken);
            }
            else
            {
                throw new InvalidOperationException("Storage details for dataset not found.");
            }
        }

        private async Task ReadDatasetFiles(DatasetBlobStorageDetails storage, CancellationToken cancellationToken)
        {
            Log.Add("Reading dataset files.");

            var ctx = new CompressContext
            {
                ContainerName = storage.Container,
                Buffer = new byte[32768],
            };

            var credentials = new StorageCredentials(storage.Account, StorageConfig.Accounts[storage.Account]);
            var storageAcct = new CloudStorageAccount(credentials, true);
            var blobClient = storageAcct.CreateCloudBlobClient();
            var blobContainer = blobClient.GetContainerReference(storage.Container);

            ctx.ArchiveContainer = blobClient.GetContainerReference(ctx.ArchiveContainerName);
            Log.Add($"Writing archives to {ctx.ArchiveContainer.Uri}");
            await ctx.ArchiveContainer.CreateIfNotExistsAsync();

            BlobContinuationToken continuationToken = null;
            const bool useFlatBlobListing = true;
            const BlobListingDetails blobListingDetails = BlobListingDetails.None;
            const int maxBlobsPerRequest = 100;

            int totalCount = 0;
            long totalSize = 0;

            await OpenOutputArchives(ctx, cancellationToken);

            do
            {
                var listingResult = await blobContainer
                    .ListBlobsSegmentedAsync("", useFlatBlobListing, blobListingDetails, maxBlobsPerRequest, continuationToken, null, null, cancellationToken)
                    .ConfigureAwait(false);
                continuationToken = listingResult.ContinuationToken;
                var results = listingResult.Results
                    .Cast<CloudBlockBlob>()
                    .Where(r => r.Name != "_metadata.txt")
                    .Select(blob => new FileDetails
                    {
                        Name = Path.GetFileName(blob.Name),
                        FullName = blob.Name,
                        Length = blob.Properties.Length,
                        Modified = blob.Properties.LastModified ?? DateTimeOffset.UtcNow,
                    })
                    .ToList();

                foreach (var result in results)
                {
                    Log.Add($"- {result.FullName}");
                    ctx.Details = result;
                    var blobReference = blobContainer.GetBlockBlobReference(result.FullName);
                    await AddDatasetFileToArchive(ctx, blobReference, cancellationToken);
                }

                totalCount += results.Count();
                totalSize += results.Sum(t => t.Length);

            } while (continuationToken != null);

            CloseOutputArchives(ctx, cancellationToken);

            (var zipSize, var tgzSize) = await GetArchiveDetails(ctx, cancellationToken);

            await DatasetStorage.UpdateDatasetCompressedDetails(DatasetId, zipSize, tgzSize);

            Console.WriteLine($"Compressed {totalCount:n0} total files, {totalSize:n0} bytes.");
            Console.WriteLine($"zip file: {zipSize:n0} bytes ({Ratio(totalSize, zipSize):n2}%).");
            Console.WriteLine($"tgz file: {tgzSize:n0} bytes ({Ratio(totalSize, tgzSize):n2}%).");
        }

        private double Ratio(long originalSize, long compressedSize)
        {
            return (1.0 - ((double)compressedSize / originalSize)) * 100.0;
        }

        private async Task AddDatasetFileToArchive(CompressContext ctx, CloudBlockBlob blobReference, CancellationToken cancellationToken)
        {
            AddToZipArchive(ctx, ctx.Details);
            AddToTarArchive(ctx, ctx.Details);

            using (var readStream = await blobReference.OpenReadAsync())
            using (var writeStream = new WriteTeeStream(ctx.ZipStream, ctx.TarStream))
            {
                StreamUtils.Copy(readStream, writeStream, ctx.Buffer);
            }

            ctx.ZipStream.CloseEntry();
            ctx.TarStream.CloseEntry();
        }

        private class CompressContext
        {
            public string ContainerName { get; set; }
            public string ArchiveContainerName =>
                ContainerName.Substring(0, Math.Min(ContainerName.Length, MaxContainerNameLength)) + "-x";
            public CloudBlobContainer ArchiveContainer { get; set; }
            public FileDetails Details { get; set; }
            public ZipOutputStream ZipStream { get; set; }
            public TarOutputStream TarStream { get; set; }
            public byte[] Buffer { get; set; }
        }

        private class FileDetails
        {
            public string Name { get; set; }
            public string FullName { get; set; }
            public long Length { get; set; }
            public DateTimeOffset Modified { get; set; }
        }
    }
}
