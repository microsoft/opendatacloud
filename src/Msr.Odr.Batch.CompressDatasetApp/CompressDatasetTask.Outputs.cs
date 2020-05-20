using System;
using System.Linq;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.GZip;
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
        private (CloudBlockBlob zipBlobReference, CloudBlockBlob tgzBlobReference) GetContainerReferences(CompressContext ctx)
        {
            var zipBlobName = $"{ctx.ContainerName}.zip";
            var zipBlobReference = ctx.ArchiveContainer.GetBlockBlobReference(zipBlobName);

            var tgzBlobName = $"{ctx.ContainerName}.tar.gz";
            var tgzBlobReference = ctx.ArchiveContainer.GetBlockBlobReference(tgzBlobName);

            return (zipBlobReference, tgzBlobReference);
        }

        private async Task OpenOutputArchives(CompressContext ctx, CancellationToken cancellationToken)
        {
            (var zipBlobReference, var tgzBlobReference) = GetContainerReferences(ctx);

            Log.Add($"Writing zip to {zipBlobReference.StorageUri.PrimaryUri}");
            Log.Add($"Block Size: {zipBlobReference.StreamWriteSizeInBytes:n0}");
            var zipBlobStream = await zipBlobReference.OpenWriteAsync(
                null,
                new BlobRequestOptions
                {
                    ParallelOperationThreadCount = 1,
                    SingleBlobUploadThresholdInBytes = 4 * 1024 * 1024,
                },
                null,
                cancellationToken);
            ctx.ZipStream = new ZipOutputStream(new WriteBlocksStream(zipBlobStream));
            ctx.ZipStream.SetLevel(9);

            Log.Add($"Writing tgz to {tgzBlobReference.StorageUri.PrimaryUri}");
            Log.Add($"Block Size: {tgzBlobReference.StreamWriteSizeInBytes:n0}");
            var tgzBlobStream = await tgzBlobReference.OpenWriteAsync(
                null,
                new BlobRequestOptions
                {
                    ParallelOperationThreadCount = 1,
                    SingleBlobUploadThresholdInBytes = 4 * 1024 * 1024,
                },
                null,
                cancellationToken);
            var gzipStream = new GZipOutputStream(new WriteBlocksStream(tgzBlobStream));
            gzipStream.SetLevel(9);
            ctx.TarStream = new TarOutputStream(gzipStream);
        }

        private void CloseOutputArchives(CompressContext ctx, CancellationToken cancellationToken)
        {
            Log.Add("Closing Zip Stream");
            ctx.ZipStream.IsStreamOwner = true;
            ctx.ZipStream.Close();

            Log.Add("Closing Tar Stream");
            ctx.TarStream.IsStreamOwner = true;
            ctx.TarStream.Close();
        }

        private async Task<(long zipSize, long tarGzSize)> GetArchiveDetails(CompressContext ctx, CancellationToken cancellationToken)
        {
            (var zipBlobReference, var tgzBlobReference) = GetContainerReferences(ctx);
            await zipBlobReference.FetchAttributesAsync();
            await tgzBlobReference.FetchAttributesAsync();
            return (zipBlobReference.Properties.Length, tgzBlobReference.Properties.Length);
        }

        private void AddToZipArchive(CompressContext ctx, FileDetails fileDetails)
        {
            var zipEntry = new ZipEntry(fileDetails.FullName)
            {
                DateTime = fileDetails.Modified.UtcDateTime,
                Size = fileDetails.Length,
            };

            ctx.ZipStream.PutNextEntry(zipEntry);
        }

        private void AddToTarArchive(CompressContext ctx, FileDetails fileDetails)
        {
            var tarEntry = TarEntry.CreateTarEntry(fileDetails.FullName);
            tarEntry.Size = fileDetails.Length;
            tarEntry.ModTime = fileDetails.Modified.UtcDateTime;

            ctx.TarStream.PutNextEntry(tarEntry);
        }
    }
}
