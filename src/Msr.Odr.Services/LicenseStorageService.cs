// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Extensions.Options;
using Msr.Odr.Model;
using Msr.Odr.Model.Configuration;
using Msr.Odr.Model.Datasets;
using Msr.Odr.Services.Configuration;

namespace Msr.Odr.Services
{
    /// <summary>
    /// Provides access to persisted license details
    /// </summary>
    public class LicenseStorageService : CosmosStorageService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LicenseStorageService" /> class.
        /// </summary>
        /// <param name="options">The current request context</param>
        public LicenseStorageService(IOptions<CosmosConfiguration> options)
			: base(options)
        {
        }

     //   /// <summary>
     //   /// Asynchronously gets all of the available licenses.
     //   /// </summary>
     //   /// <param name="cancellationToken">The cancellation token.</param>
     //   /// <returns>The matching license entry</returns>
     //   public async Task<IEnumerable<License>> GetAsync(CancellationToken cancellationToken)
     //   {
     //       cancellationToken.ThrowIfCancellationRequested();
     //       var options = new FeedOptions
     //       {
     //           MaxItemCount = 100,
     //           PartitionKey = new PartitionKey(LicenseStorageItem.LicenseDatasetId.ToString())
     //       };

     //       var query = this.Client.CreateDocumentQuery<LicenseStorageItem>(this.DatasetDocumentCollectionUri, options)
     //               .Where(f => f.DatasetId == LicenseStorageItem.LicenseDatasetId)
					//.Where(f => f.DataType == StorageDataType.License)
     //               .AsDocumentQuery();

     //       var documents = await query.ExecuteNextAsync<LicenseStorageItem>(cancellationToken)
     //                                .ConfigureAwait(false);
     //       var licenses = documents.Select(doc => new License(doc)).ToList();
     //       return licenses;
     //   }

        /// <summary>
        /// Asynchronously gets the standard available licenses.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The matching license entry</returns>
        public async Task<IEnumerable<License>> GetStandardAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var options = new FeedOptions
            {
                MaxItemCount = 500,
                PartitionKey = new PartitionKey(LicenseStorageItem.LicenseDatasetId.ToString())
            };

            var query = this.Client.CreateDocumentQuery<LicenseStorageItem>(this.DatasetDocumentCollectionUri, options)
                .Where(f => f.DatasetId == LicenseStorageItem.LicenseDatasetId && f.IsStandard && f.DataType == StorageDataType.License)
                .Select(f => new LicenseStorageItem
                {
                    Id = f.Id,
                    Name = f.Name,
                    IsStandard = f.IsStandard,
                    IsFileBased = f.IsFileBased,
                    FileContentType = f.FileContentType,
                    FileName = f.FileName,
                })
                .AsDocumentQuery();

            return (await query.GetQueryResultsAsync(cancellationToken))
                .Select(doc => new License(doc))
                .ToList();
        }

        /// <summary>
        /// Asynchronously gets the license by its identifier.
        /// </summary>
        /// <param name="id">The license identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The matching license entry</returns>
        public async Task<License> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var options = new FeedOptions
            {
                PartitionKey = new PartitionKey(LicenseStorageItem.LicenseDatasetId.ToString())
            };

            try
            {
                var query = this.Client.CreateDocumentQuery<LicenseStorageItem>(this.DatasetDocumentCollectionUri, options)
                    .Where(f => f.DatasetId == LicenseStorageItem.LicenseDatasetId && 
                                f.DataType == StorageDataType.License &&
                                f.Id == id)
                    .AsDocumentQuery();

                var documents = await query.ExecuteNextAsync<LicenseStorageItem>(cancellationToken)
                    .ConfigureAwait(false);

                var document = documents.FirstOrDefault();
                if (document != null)
                {
                    return new License(document);
                }

                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        /// <summary>
        /// Asynchronously gets the license content.
        /// </summary>
		/// <param name="id">The license identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The license content</returns>
        public async Task<string> GetContentAsync(Guid id, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var options = new RequestOptions
            {
                PartitionKey = new PartitionKey(LicenseStorageItem.LicenseDatasetId.ToString()),
            };

            var contentLink = this.CreateDatasetDocumentAttachmentUri(id, "Content");
            var response = await this.Client.ReadAttachmentAsync(contentLink.ToString(), options).ConfigureAwait(false);
            var resource = response?.Resource;
            if (resource != null)
            {
                var mediaLink = resource.MediaLink;
                var mediaLinkResponse = await this.Client.ReadMediaAsync(mediaLink).ConfigureAwait(false);
                var stream = mediaLinkResponse.Media;
                if (stream != null)
                {
                    var result = await new StreamReader(stream).ReadToEndAsync().ConfigureAwait(false);
                    result = Regex.Replace(result, @"\r\n?|\n", "<br />");
                    return result;
                }
            }

            return null;
        }


        /// <summary>
        /// Asynchronously gets the license file.
        /// </summary>
        /// <param name="id">The license identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The license file content</returns>
        public async Task<OtherLicenseFile> GetLicenseFileAsync(Guid id, CancellationToken cancellationToken)
        {
            var license = await this.GetByIdAsync(id, cancellationToken);

            if (license != null && license.IsFileBased && !string.IsNullOrEmpty(license.FileContent))
            {
                var fileContent = Convert.FromBase64String(license.FileContent);
                return new OtherLicenseFile
                {
                    Content = fileContent,
                    FileName = license.FileName,
                    ContentType = license.FileContentType
                };
            }

            return null;
        }
    }
}
