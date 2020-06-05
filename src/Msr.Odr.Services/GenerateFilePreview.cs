// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Auth;
using Msr.Odr.Model.Datasets;
using Msr.Odr.Model.FileSystem;
using Msr.Odr.Services.Configuration;
using Microsoft.Azure.Storage.Blob;

namespace Msr.Odr.Services
{
    public class GenerateFilePreview
    {
        private const int MaxPreviewSize = 1024 * 1024;
        public StorageConfiguration StorageConfig { get; }
        public FileStorageService FileStorageService { get; }
        public HashSet<string> PreviewFileExtensions { get; }

        public GenerateFilePreview(
            IOptions<StorageConfiguration> storageConfig,
            FileStorageService fileStorageService)
        {
            StorageConfig = storageConfig.Value;
            FileStorageService = fileStorageService;
            PreviewFileExtensions = new HashSet<string>(
                PreviewFileTypes.List,
                StringComparer.InvariantCultureIgnoreCase);
        }

        public async Task<string> GeneratePreview(Guid datasetId, Guid fileId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var details = await FileStorageService.GetDatasetFileStorageDetails(datasetId, fileId, cancellationToken);
            var blob = details as DatasetFileBlobStorageDetails;
            if (blob == null)
            {
                return null;
            }

            var fileExt = Path.GetExtension(blob.Name).TrimStart('.');
            if (!PreviewFileExtensions.Contains(fileExt))
            {
                return null;
            }

            var credentials = new StorageCredentials(blob.Account, StorageConfig.Accounts[blob.Account]);
            var storageAcct = new CloudStorageAccount(credentials, true);
            var blobClient = storageAcct.CreateCloudBlobClient();
            var blobContainer = blobClient.GetContainerReference(blob.Container);
            var blobReference = blobContainer.GetBlockBlobReference(blob.FullName);

            int readLen;
            var readBytes = new byte[MaxPreviewSize];
            using (var readStream = await blobReference.OpenReadAsync(null, null, null, cancellationToken))
            {
                readLen = await readStream.ReadAsync(readBytes, 0, readBytes.Length, cancellationToken);
            }

            int previewLen = readLen;
            while (previewLen > 0)
            {
                byte ch = readBytes[previewLen - 1];
                if (ch == '\n')
                {
                    break;
                }

                --previewLen;
            }

            if (previewLen == 0)
            {
                previewLen = readLen;
            }

            return Encoding.UTF8.GetString(readBytes, 0, previewLen);
        }
    }
}
