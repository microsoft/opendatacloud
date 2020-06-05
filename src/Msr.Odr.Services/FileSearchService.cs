// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.Extensions.Options;
using Msr.Odr.Model;
using Msr.Odr.Model.Configuration;
using Msr.Odr.Model.FileSystem;
using Msr.Odr.Services.Configuration;

namespace Msr.Odr.Services
{
    /// <summary>
    /// Provides access to the stored file data from Azure search
    /// </summary>
    public class FileSearchService : AzureSearchService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileSearchService" /> class.
        /// </summary>
        /// <param name="options">The current request context</param>
        public FileSearchService(IOptions<SearchConfiguration> options)
            : base(options)
        {
        }

        /// <summary>
        /// Gets the asynchronous.
        /// </summary>
        /// <param name="datasetId">The parent dataset id</param>
        /// <param name="folder">The parent folder</param>
        /// <param name="page">The page.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The file records</returns>
        public async Task<PagedResult<FileEntry>> GetAsync(Guid datasetId, string folder, int page, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var parameters = new SearchParameters()
            {
                OrderBy = new[] { "sortKey asc" },
                Top = PageHelper.PageSize,
                Skip = page * PageHelper.PageSize,
                IncludeTotalResultCount = true,
				Filter = $"datasetId eq '{datasetId.ToString().Replace("'", "''")}' and parent eq '{(folder??string.Empty).Replace("'", "''")}'"
            };

            var results = await this.Client.Documents.SearchAsync<FileSystemItem>("*", parameters, null, cancellationToken);

            return new PagedResult<FileEntry>
            {
                PageCount = PageHelper.CalculateNumberOfPages(results.Count),
                Value = (from item in results.Results
                         let doc = item.Document
                         select Convert(doc)).ToList()
            };
        }

        public async Task<int> DeleteAllFilesDocumentsByDatasetId(Guid datasetId, CancellationToken cancellationToken)
        {
            var count = 0;
            var parameters = new SearchParameters
            {
                Filter = $"datasetId eq '{datasetId}'",
                Select = new[] { "id" },
            };
            var response = await Client.Documents.SearchAsync<DocumentId>(string.Empty, parameters, null, cancellationToken);
            while(true)
            {
                if (response.Results.Any())
                {
                    var batch = IndexBatch.Delete("id", response.Results.Select(result => result.Document.Id.ToString()));
                    await Client.Documents.IndexAsync(batch, null, cancellationToken);
                    count += response.Results.Count();
                }

                if (response.ContinuationToken == null)
                {
                    break;
                }
                else
                {
                    response = await Client.Documents.ContinueSearchAsync<DocumentId>(response.ContinuationToken, null,
                        cancellationToken);
                }
            }

            return count;
        }

        /// <summary>
        /// Creates the search client.
        /// </summary>
        /// <returns>The search client instance.</returns>
        protected override SearchIndexClient CreateSearchClient()
        {
            return this.CreateSearchClient(Configuration.FileIndex);
        }

        /// <summary>
        /// Converts the specified stored document to a Dataset model instance.
        /// </summary>
        /// <param name="doc">The document.</param>
        /// <returns>The dataset model instance.</returns>
        private FileEntry Convert(FileSystemItem doc)
        {
			if (doc == null)
            {
                return null;
            }

            Uri previewUrl = null;
            if (doc.EntryType == FileSystemEntryType.File && (doc.CanPreview ?? false))
            {
                previewUrl = new Uri($"/datasets/{doc.DatasetId}/files/{doc.Id}/preview", UriKind.Relative);
            }

            return new FileEntry
            {
                Id = doc.Id,
                Name = doc.Name,
                EntryType = doc.EntryType == FileSystemEntryType.File ? FileEntryType.File : FileEntryType.Folder,
                ParentFolder = doc.Parent,
                PreviewUrl = previewUrl,
                Length = doc.Length
            };
        }

        private class DocumentId
        {
            public Guid Id { get; set; }
        }
    }
}