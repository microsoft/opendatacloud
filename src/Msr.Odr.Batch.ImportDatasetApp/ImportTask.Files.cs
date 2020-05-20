using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Msr.Odr.Model;
using Msr.Odr.Model.Datasets;
using Msr.Odr.Model.FileSystem;

namespace Msr.Odr.Batch.ImportDatasetApp
{
    public partial class ImportTask
    {
        private const int MaxConcurrent = 10;

        private async Task<(ICollection<string> fileTypes, int fileCount, long fileSize, string containerUri)> CreateDatasetFileDocuments(
            DatasetImportProperties storage,
            CancellationToken cancellationToken)
        {
            Log.Add("Creating dataset file documents.");

            var credentials = new StorageCredentials(storage.AccountName, StorageConfig.Accounts[storage.AccountName]);
            var storageAcct = new CloudStorageAccount(credentials, true);
            var blobClient = storageAcct.CreateCloudBlobClient();
            var blobContainer = blobClient.GetContainerReference(storage.ContainerName);

            BlobContinuationToken continuationToken = null;
            const bool useFlatBlobListing = true;
            const BlobListingDetails blobListingDetails = BlobListingDetails.None;
            const int maxBlobsPerRequest = 100;

            var parents = new HashSet<string>();
            var extensions = new HashSet<string>();
            int totalCount = 0;
            long totalSize = 0;

            var concurrencySemaphore = new SemaphoreSlim(MaxConcurrent);

            // Add file records
            var taskList = new List<Task>();
            do
            {
                var listingResult = await blobContainer
                    .ListBlobsSegmentedAsync("", useFlatBlobListing, blobListingDetails, maxBlobsPerRequest, continuationToken, null, null, cancellationToken)
                    .ConfigureAwait(false);
                continuationToken = listingResult.ContinuationToken;
                var results = listingResult.Results
                    .Cast<CloudBlockBlob>()
                    .Where(r => r.Name != "_metadata.txt")
                    .Select(blob => new
                    {
                        Segments = blob.Uri.Segments
                            .Skip(2)
                            .Select(s => s.Trim('/'))
                            .Take(blob.Uri.Segments.Length - 3),
                        File = new FileSystemItem
                        {
                            Id = Guid.NewGuid(),
                            Name = Path.GetFileName(blob.Name),
                            FullName = blob.Name,
                            FileType = GetFileExtension(blob.Name),
                            CanPreview = false,
                            DatasetId = DatasetId,
                            DataType = StorageDataType.FileSystem,
                            EntryType = FileSystemEntryType.File,
                            Length = blob.Properties.Length,
                            Parent = Path.GetDirectoryName(blob.Name).Replace(@"\", @"/"),
                            SortKey = GenerateSortKey("1", blob.Name),
                            Modified = blob.Properties.LastModified ?? DateTimeOffset.UtcNow,
                        },
                        Blob = new FileSystemItemBlobDetails
                        {
                            DatasetId = DatasetId,
                            Account = storage.AccountName,
                            Container = storage.ContainerName,
                            Name = Path.GetFileName(blob.Name),
                            ContentType = blob.Properties.ContentType,
                            Uri = blob.Uri.ToString(),
                        }
                    })
                    .ToList();

                foreach (var result in results)
                {
                    result.Segments
                        .Aggregate(new List<string>(), (list, s) =>
                        {
                            list.Add(list.Count == 0 ? s : string.Concat(list[list.Count - 1], "/", s));
                            return list;
                        })
                        .ToList()
                        .ForEach(p => parents.Add(p));
                    extensions.Add(result.File.FileType);

                    await concurrencySemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

                    var task = Task.Run(async () =>
                    {
                        await DatasetStorage.CreateFileRecord(result.File, result.Blob).ConfigureAwait(false);
                        concurrencySemaphore.Release();
                        if (Interlocked.Increment(ref totalCount) % 100 == 0)
                        {
                            Log.Add($"Loaded {totalCount} file records ...");
                        }
                    }, cancellationToken);

                    taskList.Add(task);
                }

                totalSize += results.Sum(t => t.File.Length ?? 0);

            } while (continuationToken != null);

            await Task.WhenAll(taskList).ConfigureAwait(false);
            Log.Add($"Loaded {totalCount} total file records.");

            // Add the file summary record
            var fileTypes = extensions.OrderBy(e => e).ToList();
            var fileSummary = new FileSystemSummary
            {
                Id = Guid.NewGuid(),
                DatasetId = DatasetId,
                DataType = StorageDataType.FileSummary,
                FileCount = totalCount,
                Size = totalSize,
                FileTypes = fileTypes,
            };
            await DatasetStorage.CreateFileSummaryRecord(fileSummary).ConfigureAwait(false);

            // Add file folder records
            foreach (var folder in parents)
            {
                var fileItem = new FileSystemItem
                {
                    Id = Guid.NewGuid(),
                    Name = Path.GetFileName(folder),
                    FullName = folder,
                    FileType = null,
                    CanPreview = null,
                    DatasetId = DatasetId,
                    DataType = StorageDataType.FileSystem,
                    EntryType = FileSystemEntryType.Folder,
                    Length = null,
                    Parent = Path.GetDirectoryName(folder).Replace(@"\", @"/"),
                    SortKey = GenerateSortKey("0", folder),
                    Modified = DateTimeOffset.UtcNow,
                };
                await DatasetStorage.CreateFileRecord(fileItem).ConfigureAwait(false);
            }
            Log.Add($"Loaded {parents.Count} folder records.");

            return (fileTypes, totalCount, totalSize, blobContainer.StorageUri.PrimaryUri.ToString());
        }

        private string GenerateSortKey(string prefix, string name)
        {
            var key = CultureInfo.InvariantCulture.CompareInfo.GetSortKey(prefix + name, CompareOptions.StringSort | CompareOptions.IgnoreCase);
            return Convert.ToBase64String(key.KeyData);
        }

        private string GetFileExtension(string name)
        {
            var ext = Path.GetExtension(name);
            return (ext.StartsWith(".") ? ext.Substring(1) : ext).ToLowerInvariant();
        }
    }
}
