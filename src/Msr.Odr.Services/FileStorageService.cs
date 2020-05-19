using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Options;
using Msr.Odr.Model;
using Msr.Odr.Model.Configuration;
using Msr.Odr.Model.Datasets;
using Msr.Odr.Model.FileSystem;
using Msr.Odr.Services.Configuration;

//using Microsoft.ApplicationInsights;
//using Microsoft.AspNetCore.Mvc;

namespace Msr.Odr.Services
{
    /// <summary>
    /// Provides access to persisted, unindexed file details
    /// </summary>
    public class FileStorageService : CosmosStorageService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileStorageService" /> class.
        /// </summary>
        /// <param name="options">The current request context</param>
        /// <param name="sasTokens">The SAS token generation service.</param>
        public FileStorageService(IOptions<CosmosConfiguration> options, SasTokenService sasTokens)
			: base(options)
        {
            this.SasTokens = sasTokens;
        }

        /// <summary>
        /// The SAS Token generation service
        /// </summary>
        protected SasTokenService SasTokens { get; }

        /// <summary>
        /// Asynchronously gets the file item by its identifier.
        /// </summary>
        /// <param name="datasetId">The dataset identifier</param>
        /// <param name="id">The file identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The matching file entry</returns>
        public async Task<FileEntry> GetByIdAsync(Guid datasetId, Guid id, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var options = new RequestOptions
            {
                PartitionKey = new PartitionKey(datasetId.ToString())
            };

            var documentLink = this.CreateDatasetDocumentUri(id);
            var previewLinkUri = this.CreateDatasetDocumentAttachmentUri(id, "Preview");
            var document = await this.Client.ReadDocumentAsync<FileSystemItem>(documentLink.ToString(), options).ConfigureAwait(false);

            var doc = document.Document;
            if (doc == null)
            {
                return null;
            }

            Uri previewUrl = null;
            if (doc.EntryType == FileSystemEntryType.File && (doc.CanPreview??false))
            {
                previewUrl = new Uri($"/datasets/{doc.DatasetId}/files/{doc.Id}/preview", UriKind.Relative);
            }

            var result = new FileEntry
            {
                Id = doc.Id,
                Name = doc.Name,
                EntryType = doc.EntryType == FileSystemEntryType.File ? FileEntryType.File : FileEntryType.Folder,
                ParentFolder = doc.Parent,
                PreviewUrl = previewUrl,
                Length = doc.Length
            };

            return result;
        }

        public async Task<DatasetFileStorageDetails> GetDatasetFileStorageDetails(Guid datasetId, Guid fileId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var options = new RequestOptions
            {
                PartitionKey = new PartitionKey(datasetId.ToString())
            };

            var documentLink = CreateDatasetDocumentUri(fileId);
            var document = await Client.ReadDocumentAsync<FileSystemItem>(documentLink.ToString(), options).ConfigureAwait(false);
            var doc = document.Document;
            if (doc == null)
            {
                return null;
            }

            var fileAttachmentUri = CreateDatasetDocumentAttachmentUri(fileId, doc.Name);
            var response = await Client.ReadAttachmentAsync(fileAttachmentUri, options).ConfigureAwait(false);
            var resource = response?.Resource;
            if (resource == null)
            {
                return null;
            }

            DatasetFileStorageDetails details = null;
            var storageType = resource.GetPropertyValue<string>("storageType") ?? string.Empty;
            switch (storageType)
            {
                case "blob":
                    details = new DatasetFileBlobStorageDetails
                    {
                        DatasetId = datasetId,
                        FileId = fileId,
                        StorageType = DatasetFileStorageTypes.Blob,
                        Account = resource.GetPropertyValue<string>("account"),
                        Container = resource.GetPropertyValue<string>("container"),
                        Name = resource.GetPropertyValue<string>("blob"),
                        FullName = doc.FullName,
                        ContentType = resource.GetPropertyValue<string>("contentType"),
                    };
                    break;
                default:
                    throw new InvalidOperationException($"Unknown storage type, \"{storageType}\", for dataset file.");
            }

            return details;
        }

        /// <summary>
        /// Asynchronously gets the download URI for a dataset.
        /// </summary>
        /// <param name="datasetId">The dataset identifier</param>
		/// <param name="id">The file identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The download URI</returns>
        public async Task<Uri> GetDownloadUriAsync(Guid datasetId, Guid id, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var options = new FeedOptions
            {
				MaxItemCount = 5,
                PartitionKey = new PartitionKey(datasetId.ToString())
            };

            var documentLink = this.CreateDatasetDocumentUri(id);
            var response = await this.Client.ReadAttachmentFeedAsync(documentLink, options).ConfigureAwait(false);
            var resource = response?.FirstOrDefault(t => t.Id != "Preview" && t.Id != "Content");
            if (resource != null)
            {
                var storageType = resource?.GetPropertyValue<string>("storageType");
                if (storageType == "blob")
                {
                    var account = resource?.GetPropertyValue<string>("account");
                    var container = resource?.GetPropertyValue<string>("container");
                    var blob = resource?.GetPropertyValue<string>("blob");

                    return this.SasTokens.CreateFileSasToken(account, container, blob);
                }
            }

            return null;
        }
    }
}
