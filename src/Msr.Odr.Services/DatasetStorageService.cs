using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Azure.Search.Models;
using Microsoft.Extensions.Options;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Auth;
using Msr.Odr.Model;
using Msr.Odr.Model.Configuration;
using Msr.Odr.Model.Datasets;
using Msr.Odr.Model.FileSystem;
using Msr.Odr.Model.UserData;
using Msr.Odr.Services.Configuration;
using Msr.Odr.Services.Mappers;
using Microsoft.Azure.Storage.Blob;

namespace Msr.Odr.Services
{
    /// <summary>
    /// Provides access to persisted, unindexed dataset details
    /// </summary>
    public class DatasetStorageService : UserClaimsStorageService
    {
        private readonly DatasetSearchService _datasetSearchService;
        private readonly IOptions<CosmosConfiguration> _options;
        private readonly StorageConfiguration _storageConfig;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatasetStorageService" /> class.
        /// </summary>
        /// <param name="options">The current request context</param>
        /// <param name="sasTokens">The SAS token generation service.</param>
        public DatasetStorageService(
            IOptions<CosmosConfiguration> options,
            IOptions<StorageConfiguration> storageConfig,
            SasTokenService sasTokens,
            DatasetSearchService datasetSearchService)
            : base(options, sasTokens)
        {
            _datasetSearchService = datasetSearchService;
            _options = options;
            _storageConfig = storageConfig.Value;
        }

        /// <summary>
        /// Asynchronously gets the dataset by its identifier.
        /// </summary>
        /// <param name="datasetId">The dataset identifier.</param>
        /// <param name="user">The user .</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The matching dataset</returns>
        public async Task<Dataset> GetByIdAsync(Guid datasetId, ClaimsPrincipal user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var dataset = (await Client.CreateDocumentQuery<DatasetStorageItem>(
                DatasetDocumentCollectionUri,
                new FeedOptions
                {
                    PartitionKey = new PartitionKey(datasetId.ToString())
                })
                .Where(d => d.Id == datasetId)
                .AsDocumentQuery()
                .GetQueryResultsAsync(cancellationToken))
                .SingleOrDefault();
            if (dataset == null)
            {
                return null;
            }

            return dataset.ToDataset();
        }

        public async Task<bool> DeleteDatasetAsync(ClaimsPrincipal user, Guid datasetId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var userId = GetUserId(user);
            if (!userId.HasValue)
            {
                return false;
            }

            var options = new FeedOptions
            {
                PartitionKey = new PartitionKey(datasetId.ToString())
            };

            var deleteOptions = new RequestOptions
            {
                PartitionKey = new PartitionKey(datasetId.ToString())
            };

            var existingDocument = this.Client.CreateDocumentQuery<Microsoft.Azure.Documents.Document>(DatasetDocumentCollectionUri, options)
                .Where(r => r.Id == datasetId.ToString())
                .AsEnumerable()
                .SingleOrDefault();

            if (existingDocument == null)
            {
                throw new InvalidOperationException("Dataset not found.");
            }

            await DeleteAzureSearchIndexForDataset(datasetId, cancellationToken);
            await DeleteContainersForDataset(datasetId, cancellationToken);
            await DeleteDatasetDocuments(datasetId, cancellationToken);

            return true;
        }

        public async Task<Guid?> UpdateDatasetAsync(ClaimsPrincipal user, Dataset dataset, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var userId = GetUserId(user);
            if (!userId.HasValue)
            {
                return null;
            }
            var name = GetUserName(user);
            var email = GetUserEmail(user);

            var options = new FeedOptions
            {
                EnableCrossPartitionQuery = true,
                PartitionKey = new PartitionKey(dataset.Id.ToString())
            };

            var uri = DatasetDocumentCollectionUri;
            var existingDocument = this.Client.CreateDocumentQuery<DatasetStorageItem>(uri, options)
                .Where(r => r.Id == dataset.Id)
                .AsEnumerable()
                .SingleOrDefault();

            if (existingDocument == null)
            {
                throw new InvalidOperationException("Dataset not found.");
            }

            var record = dataset.ToDatasetStorageItem(d =>
            {
                d.Created = existingDocument.Created;
                d.CreatedByUserName = existingDocument.CreatedByUserName;
                d.CreatedByUserEmail = existingDocument.CreatedByUserEmail;
                d.ModifiedByUserName = name;
                d.ModifiedByUserEmail = email;
                d.Modified = DateTime.UtcNow;

                d.FileCount = existingDocument.FileCount;
                d.FileTypes = existingDocument.FileTypes;
                d.Size = existingDocument.Size;
                d.ZipFileSize = existingDocument.ZipFileSize;
                d.GzipFileSize = existingDocument.GzipFileSize;
                d.IsCompressedAvailable = existingDocument.IsCompressedAvailable;
            });

            uri = DatasetDocumentUriById(record.Id.ToString());
            await this.Client.ReplaceDocumentAsync(uri, record);

            return dataset.Id;
        }

        /// <summary>
        /// Asynchronously gets the zip file URI for a dataset.
        /// </summary>
        /// <param name="datasetId">The dataset identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The zip file download URI</returns>
        public async Task<Uri> GetZipUriAsync(Guid datasetId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var options = new RequestOptions
            {
                PartitionKey = new PartitionKey(datasetId.ToString())
            };

            try
            {
                var zipLink = this.CreateDatasetDocumentAttachmentUri(datasetId, "Content");
                var response = await this.Client.ReadAttachmentAsync(zipLink, options).ConfigureAwait(false);
                var resource = response?.Resource;
                if (resource != null)
                {
                    var storageType = resource?.GetPropertyValue<string>("storageType");
                    if (storageType == "blob")
                    {
                        var account = resource?.GetPropertyValue<string>("account");
                        var container = resource?.GetPropertyValue<string>("container");
                        var blob = $"{container}.zip";
                        return this.SasTokens.CreateFileSasToken(account, $"{container}-x", blob);
                    }
                }
            }
            catch (DocumentClientException ex)
            {
                if (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }
                throw;
            }

            return null;
        }

        /// <summary>
        /// Asynchronously gets the gzip file URI for a dataset.
        /// </summary>
        /// <param name="datasetId">The dataset identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The gzip file download URI</returns>
        public async Task<Uri> GetGzipUriAsync(Guid datasetId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var options = new RequestOptions
            {
                PartitionKey = new PartitionKey(datasetId.ToString())

            };

            try
            {
                var gzipLink = this.CreateDatasetDocumentAttachmentUri(datasetId, "Content");
                var response = await this.Client.ReadAttachmentAsync(gzipLink, options).ConfigureAwait(false);
                var resource = response?.Resource;
                if (resource != null)
                {
                    var storageType = resource?.GetPropertyValue<string>("storageType");
                    if (storageType == "blob")
                    {
                        var account = resource?.GetPropertyValue<string>("account");
                        var container = resource?.GetPropertyValue<string>("container");
                        var blob = $"{container}.tar.gz";
                        return this.SasTokens.CreateFileSasToken(account, $"{container}-x", blob);
                    }
                }
            }
            catch (DocumentClientException ex)
            {
                if (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }
                throw;
            }

            return null;
        }

        /// <summary>
        /// Asynchronously gets the download URI for a dataset.
        /// </summary>
        /// <param name="id">The dataset identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The download URI</returns>
        public async Task<Uri> GetDownloadUriAsync(Guid id, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var options = new RequestOptions
            {
                PartitionKey = new PartitionKey(id.ToString())
            };

            try
            {
                var documentLink = this.CreateDatasetDocumentAttachmentUri(id, "Content");
                var response = await this.Client.ReadAttachmentAsync(documentLink, options).ConfigureAwait(false);
                var resource = response?.Resource;
                if (resource != null)
                {
                    var storageType = resource?.GetPropertyValue<string>("storageType");
                    if (storageType == "blob")
                    {
                        var account = resource?.GetPropertyValue<string>("account");
                        var container = resource?.GetPropertyValue<string>("container");

                        return this.SasTokens.CreateContainerSasToken(account, container);
                    }
                }
            }
            catch (DocumentClientException ex)
            {
                if (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }
                throw;
            }

            return null;
        }

        public async Task<DatasetStorageDetails> GetDatasetStorageDetails(Guid datasetId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var options = new RequestOptions
            {
                PartitionKey = new PartitionKey(datasetId.ToString())
            };

            var documentLink = CreateDatasetDocumentAttachmentUri(datasetId, "Content");
            var response = await Client.ReadAttachmentAsync(documentLink, options).ConfigureAwait(false);
            var resource = response?.Resource;
            if (resource == null)
            {
                return null;
            }

            DatasetStorageDetails details = null;
            var storageType = resource.GetPropertyValue<string>("storageType") ?? string.Empty;
            switch (storageType)
            {
                case "blob":
                    details = new DatasetBlobStorageDetails
                    {
                        DatasetId = datasetId,
                        StorageType = DatasetStorageTypes.Blob,
                        Account = resource.GetPropertyValue<string>("account"),
                        Container = resource.GetPropertyValue<string>("container"),
                    };
                    break;
                default:
                    throw new InvalidOperationException($"Unknown storage type, \"{storageType}\", for dataset.");
            }

            return details;
        }

        public async Task<IEnumerable<Domain>> GetAllDomains(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var domainsList = new Domain[0];

            var options = new RequestOptions
            {
                PartitionKey = new PartitionKey(WellKnownIds.ConfigurationDatasetId.ToString())
            };

            var documentLink = this.CreateDatasetDocumentUri(WellKnownIds.DomainsDocId);
            DomainsStorageItem doc;
            try
            {
                var document = await this.Client.ReadDocumentAsync<DomainsStorageItem>(documentLink.ToString(), options).ConfigureAwait(false);

                doc = document.Document;
                if (doc == null)
                {
                    return domainsList;
                }
            }
            catch (Exception ex) when (ex.GetType().Name == "NotFoundException")
            {
                return domainsList;
            }
            return doc.Domains;
        }

        public async Task<IEnumerable<Domain>> GetDomainsInUseByDatasets(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var domainsList = new Domain[0];
            var options = new FeedOptions
            {
                EnableCrossPartitionQuery = true
            };

            try
            {
                var query = this.Client.CreateDocumentQuery<DatasetStorageItem>(this.DatasetDocumentCollectionUri, options)
                        .Where(f => f.DataType == StorageDataType.Dataset)
                        .AsDocumentQuery();

                var datasets = await query.ExecuteNextAsync<DatasetStorageItem>(cancellationToken)
                                         .ConfigureAwait(false);

                if (!datasets.Any())
                {
                    return new Domain[0];
                }

                var domains = datasets
                    .GroupBy(ds => ds.DomainId)
                    .Select(grp => new Domain { Id = grp.Key, Name = grp.First().Domain })
                    .OrderBy(domain => domain.Id);

                return domains;
            }
            catch (Exception ex) when (ex.GetType().Name == "NotFoundException")
            {
                return domainsList;
            }
        }

        public async Task<IEnumerable<ReportItem>> GetCountOfDatasetsByDomain(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var reportItemList = new ReportItem[0];
            var options = new FeedOptions
            {
                EnableCrossPartitionQuery = true
            };

            try
            {
                var query = this.Client.CreateDocumentQuery<DatasetStorageItem>(this.DatasetDocumentCollectionUri, options)
                    .Where(f => f.DataType == StorageDataType.Dataset)
                    .AsDocumentQuery();

                var datasets = await query.ExecuteNextAsync<DatasetStorageItem>(cancellationToken)
                    .ConfigureAwait(false);

                if (!datasets.Any())
                {
                    return new ReportItem[0];
                }

                var reportItems = datasets
                    .GroupBy(ds => ds.DomainId)
                    .Select(grp => new ReportItem { Name = grp.Key, Count = grp.Count() })
                    .OrderBy(grp => grp.Name);

                return reportItems;
            }
            catch (Exception ex) when (ex.GetType().Name == "NotFoundException")
            {
                return reportItemList;
            }
        }

        public async Task<List<DatasetDigestItem>> GetDatasetDigestItems(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var reportItemList = new ReportItem[0];
            var options = new FeedOptions
            {
                EnableCrossPartitionQuery = true
            };

            try
            {
                var query = this.Client.CreateDocumentQuery<DatasetStorageItem>(this.DatasetDocumentCollectionUri, options)
                    .Where(f => f.DataType == StorageDataType.Dataset)
                    .AsDocumentQuery();

                var datasets = await query.ExecuteNextAsync<DatasetStorageItem>(cancellationToken)
                                                .ConfigureAwait(false);

                if (datasets != null)
                {
                    var digestItems = datasets.Select(ds =>
                        new DatasetDigestItem
                        {
                            Name = ds.Name,
                            DatasetId = ds.DatasetId
                        })
                        .ToList();
                    return digestItems;
                }

                return new List<DatasetDigestItem>();
            }
            catch (Exception ex) when (ex.GetType().Name == "NotFoundException")
            {
                return new List<DatasetDigestItem>();
            }
        }

        public async Task<IEnumerable<ReportItem>> GetCountOfDatasetsByLicense(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var reportItemList = new ReportItem[0];
            var options = new FeedOptions
            {
                EnableCrossPartitionQuery = true
            };

            try
            {
                var query = this.Client.CreateDocumentQuery<DatasetStorageItem>(this.DatasetDocumentCollectionUri, options)
                    .Where(f => f.DataType == StorageDataType.Dataset)
                    .AsDocumentQuery();

                var datasets = await query.ExecuteNextAsync<DatasetStorageItem>(cancellationToken)
                    .ConfigureAwait(false);

                if (!datasets.Any())
                {
                    return new ReportItem[0];
                }

                var reportItems = datasets
                    .GroupBy(ds => ds.License)
                    .Select(grp => new ReportItem { Name = grp.Key, Count = grp.Count() })
                    .OrderBy(grp => grp.Name);

                return reportItems;
            }
            catch (Exception ex) when (ex.GetType().Name == "NotFoundException")
            {
                return reportItemList;
            }
        }

        public async Task<IEnumerable<FAQ>> GetFAQs(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var options = new FeedOptions
            {
                MaxItemCount = 100,
                PartitionKey = new PartitionKey(WellKnownIds.ConfigurationDatasetId.ToString())
            };

            var query = this.Client.CreateDocumentQuery<FAQStorageItem>(this.DatasetDocumentCollectionUri, options)
                    .Where(f => f.DatasetId == WellKnownIds.ConfigurationDatasetId)
                    .Where(f => f.DataType == StorageDataType.FAQ)
                    .AsDocumentQuery();

            var documents = await query.ExecuteNextAsync<FAQStorageItem>(cancellationToken)
                                     .ConfigureAwait(false);
            var faqList = (from doc in documents
                           orderby doc.Order
                           select new FAQ
                           {
                               Id = doc.Id,
                               Title = doc.Title,
                               Content = doc.Content,
                               Order = doc.Order,
                           }).ToList();

            return faqList;
        }

        public async Task<int> DeleteDatasetDocuments(Guid datasetId, CancellationToken cancellationToken)
        {
            int documentCount = 0;

            cancellationToken.ThrowIfCancellationRequested();
            var options = new FeedOptions
            {
                PartitionKey = new PartitionKey(datasetId.ToString())
            };
            var deleteOptions = new RequestOptions
            {
                PartitionKey = new PartitionKey(datasetId.ToString())
            };

            var query = Client
                .CreateDocumentQuery<DatasetStorageItem>(DatasetDocumentCollectionUri, options)
                .Select(doc => doc.Id)
                .AsDocumentQuery();

            while (query.HasMoreResults)
            {
                foreach (var id in await query.ExecuteNextAsync<string>(cancellationToken))
                {
                    ++documentCount;
                    var docLink = DatasetDocumentUriById(id);
                    await Client.DeleteDocumentAsync(docLink, deleteOptions);
                }
            }
          
            return documentCount;
        }

        public async Task CreateFileRecord(FileSystemItem fileItem, FileSystemItemBlobDetails blob = null)
        {
            var options = new RequestOptions
            {
                PartitionKey = new PartitionKey(fileItem.DatasetId.ToString())
            };
            var fileRecord = await Client.UpsertDocumentAsync(DatasetDocumentCollectionUri, fileItem, options).ConfigureAwait(false);

            if (blob != null)
            {
                var link = new Attachment
                {
                    Id = blob.Name,  // "Slug" is ID with hard-attach
                    ContentType = blob.ContentType,
                    MediaLink = blob.Uri,
                };

                link.SetPropertyValue("storageType", "blob");
                link.SetPropertyValue("container", blob.Container);
                link.SetPropertyValue("account", blob.Account);
                link.SetPropertyValue("blob", blob.Name);

                await Client.UpsertAttachmentAsync(fileRecord.Resource.SelfLink, link, options).ConfigureAwait(false);
            }
        }

        public async Task CreateFileSummaryRecord(FileSystemSummary fileSummary)
        {
            var options = new RequestOptions
            {
                PartitionKey = new PartitionKey(fileSummary.DatasetId.ToString())
            };
            await Client.UpsertDocumentAsync(DatasetDocumentCollectionUri, fileSummary, options).ConfigureAwait(false);
        }

        public async Task CreateDatasetRecord(Dataset dataset, DatasetItemContainerDetails containerDetails)
        {
            var datasetItem = dataset.ToDatasetStorageItem(d =>
            {
                d.Created = DateTime.UtcNow;
                d.CreatedByUserName = dataset.CreatedByUserName;
                d.CreatedByUserEmail = dataset.CreatedByUserEmail;
            });

            var options = new RequestOptions
            {
                PartitionKey = new PartitionKey(dataset.Id.ToString())
            };
            var datasetRecord = await Client.UpsertDocumentAsync(DatasetDocumentCollectionUri, datasetItem, options).ConfigureAwait(false);

            var link = new Attachment
            {
                Id = containerDetails.Name,
                ContentType = containerDetails.ContentType,
                MediaLink = containerDetails.Uri
            };

            link.SetPropertyValue("storageType", "blob");
            link.SetPropertyValue("container", containerDetails.Container);
            link.SetPropertyValue("account", containerDetails.Account);

            await Client.UpsertAttachmentAsync(datasetRecord.Resource.SelfLink, link, options).ConfigureAwait(false);
        }

        public async Task CreateDatasetStorageDetailsRecord(DatasetItemContainerDetails containerDetails)
        {
            var datasetItem = new DatasetStorageDetailsStorageItem
            {
                DatasetId = containerDetails.DatasetId,
                Id = Guid.NewGuid(),
                Account = containerDetails.Account,
                Container = containerDetails.Container,
                StorageType = "blob",
                PrimaryUri = new Uri(containerDetails.Uri),
            };

            var options = new RequestOptions
            {
                PartitionKey = new PartitionKey(containerDetails.DatasetId.ToString())
            };
            await Client.UpsertDocumentAsync(DatasetDocumentCollectionUri, datasetItem, options).ConfigureAwait(false);
        }

        public async Task UpdateDatasetCompressedDetails(Guid datasetId, long zipFileSize, long targzFileSize)
        {
            var options = new RequestOptions
            {
                PartitionKey = new PartitionKey(datasetId.ToString())
            };
            var documentUri = DatasetDocumentUriById(datasetId.ToString());
            Microsoft.Azure.Documents.Document dataset = await Client.ReadDocumentAsync(documentUri, options);
            if (dataset == null)
            {
                throw new InvalidOperationException("Could not find dataset document.");
            }

            dataset.SetPropertyValue("zipFileSize", zipFileSize);
            dataset.SetPropertyValue("gzipFileSize", targzFileSize);
            dataset.SetPropertyValue("isCompressedAvailable", true);
            await Client.ReplaceDocumentAsync(dataset.SelfLink, dataset);
        }

        public async Task<LicenseStorageItem> CreateLicense(IOtherLicenseDetails nomination)
        {
            var licenseId = Guid.NewGuid();

            RequestOptions dbOptions = new RequestOptions
            {
                PartitionKey = new PartitionKey(LicenseStorageItem.LicenseDatasetId.ToString())
            };

            var licenseStorageItem = new LicenseStorageItem
            {
                Id = licenseId,
                IsStandard = false,
                Name = nomination.OtherLicenseName
            };

            if (nomination.NominationLicenseType == NominationLicenseType.InputFile)
            {
                licenseStorageItem.FileName = nomination.OtherLicenseFileName;
                licenseStorageItem.FileContentType = nomination.OtherLicenseFileContentType;
                licenseStorageItem.IsFileBased = true;
                licenseStorageItem.FileContent = nomination.OtherLicenseFileContent;
            }

            var documentCollection = this.DatasetDocumentCollectionUri;

            // Create document record
            ResourceResponse<Microsoft.Azure.Documents.Document> licenseRecord = await Client.UpsertDocumentAsync(
                documentCollection,
                licenseStorageItem,
                dbOptions);

            if (nomination.NominationLicenseType == NominationLicenseType.HtmlText)
            {
                await CreateHtmlContentLicenseContentAttachment(nomination, licenseRecord);
            }

            if (!string.IsNullOrEmpty(nomination.OtherLicenseAdditionalInfoUrl))
            {
                var licenseRecordLink = new Attachment
                {
                    Id = "Source", // "Slug" is ID with hard-attach
                    ContentType = "text/plain",
                    MediaLink = nomination.OtherLicenseAdditionalInfoUrl
                };

                var licenseRefAttachment = await Client.UpsertAttachmentAsync(licenseRecord.Resource.SelfLink,
                    licenseRecordLink,
                    dbOptions);
            }

            return licenseStorageItem;
        }

        public async Task<ICollection<Regex>> GetEligibleDatasetOwners(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var options = new RequestOptions
            {
                PartitionKey = new PartitionKey(WellKnownIds.ConfigurationDatasetId.ToString())
            };

            var documentLink = this.CreateDatasetDocumentUri(WellKnownIds.DatasetOwnersDocId);
            DatasetOwnersStorageItem doc;
            try
            {
                var document = await this.Client.ReadDocumentAsync<DatasetOwnersStorageItem>(documentLink, options).ConfigureAwait(false);
                doc = document.Document;
            }
            catch (Exception ex) when (ex.GetType().Name == "NotFoundException")
            {
                doc = new DatasetOwnersStorageItem
                {
                    Eligible = new List<string>()
                };
            }

            return doc.Eligible.Select(pattern => new Regex(pattern, RegexOptions.IgnoreCase)).ToList();
        }

        public async Task UpdateDatasetDetailsInSearchIndex(Guid datasetId, CancellationToken token)
        {
            var response = await Client.ReadDocumentAsync<DatasetStorageItem>(
                CreateDatasetDocumentUri(datasetId),
                new RequestOptions
                {
                    PartitionKey = new PartitionKey(datasetId.ToString())
                });

            await _datasetSearchService.UpdateDatasetDocInSearchIndex(response.Document, token);
        }

        public async Task<ICollection<DatasetSiteMapEntry>> GetAllDatasetsForSiteMap(CancellationToken cancellationToken)
        {
            var idQuery = this.Client
                .CreateDocumentQuery<DatasetStorageItem>(this.DatasetDocumentCollectionUri, new FeedOptions
                {
                    EnableCrossPartitionQuery = true
                })
                .AsQueryable()
                .Where(item => item.DataType == StorageDataType.Dataset)
                .Select(item => new DatasetSiteMapEntry
                {
                    DatasetId = item.DatasetId,
                    Modified = item.Modified,
                })
                .AsDocumentQuery();

            var datasetEntries = new List<DatasetSiteMapEntry>();
            while (idQuery.HasMoreResults)
            {
                foreach (var entry in await idQuery.ExecuteNextAsync<DatasetSiteMapEntry>(cancellationToken))
                {
                    datasetEntries.Add(entry);
                }
            }

            return datasetEntries;
        }

        private async Task CreateHtmlContentLicenseContentAttachment(IOtherLicenseDetails nomination, ResourceResponse<Microsoft.Azure.Documents.Document> licenseRecord)
        {
            RequestOptions dbOptions = new RequestOptions
            {
                PartitionKey = new PartitionKey(LicenseStorageItem.LicenseDatasetId.ToString())
            };

            var contentRecordOptions = new MediaOptions
            {
                Slug = "Content",  // "Slug" is ID with hard-attach
                ContentType = "text/plain"
            };

            await Client.UpsertAttachmentAsync(
                licenseRecord.Resource.SelfLink,
                new MemoryStream(Encoding.UTF8.GetBytes(nomination.OtherLicenseContentHtml)),
                contentRecordOptions,
                dbOptions);
        }

        private async Task<List<string>> GetIdsForDatasetFiles(Guid datasetId, FeedOptions options, CancellationToken cancellationToken)
        {
            var existingDocumentIdQuery = this.Client
                .CreateDocumentQuery<FileSystemItem>(this.DatasetDocumentCollectionUri, options)
                .AsQueryable()
                .Where(item => item.DataType == StorageDataType.FileSystem)
                .Select(item => item.Id)
                .AsDocumentQuery();

            var existingDocumentIds = new List<string>();

            while (existingDocumentIdQuery.HasMoreResults)
            {
                foreach (var id in await existingDocumentIdQuery.ExecuteNextAsync<string>(cancellationToken))
                {
                    existingDocumentIds.Add(id);
                }
            }

            return existingDocumentIds;
        }

        private async Task DeleteAzureSearchIndexForDataset(Guid datasetId, CancellationToken cancellationToken)
        {
            var options = new FeedOptions
            {
                PartitionKey = new PartitionKey(datasetId.ToString())
            };

            var existingDocumentIds = await GetIdsForDatasetFiles(datasetId, options, cancellationToken);

            var deleteOptions = new RequestOptions
            {
                PartitionKey = new PartitionKey(datasetId.ToString())
            };

            // add the dataset to the delete list for index docs
            existingDocumentIds.Add(datasetId.ToString());

            var batchDelete = IndexBatch.Delete("id", existingDocumentIds);
            await _datasetSearchService.UpdateIndexAsync(batchDelete);
        }

        private async Task DeleteContainersForDataset(Guid datasetId, CancellationToken cancellationToken)
        {
            var datasetStorageDetails = await GetDatasetStorageDetails(datasetId, cancellationToken);

            if (datasetStorageDetails == null)
            {
                throw new InvalidOperationException("Dataset storage details not found.");
            }

            var blobStorageDetails = datasetStorageDetails as DatasetBlobStorageDetails;
            if (blobStorageDetails == null)
            {
                throw new InvalidOperationException("Blob storage details not found.");
            }

            var credentials = new StorageCredentials(blobStorageDetails.Account, _storageConfig.Accounts[blobStorageDetails.Account]);
            var storageAcct = new CloudStorageAccount(credentials, true);
            
            var blobClient = storageAcct.CreateCloudBlobClient();
            var containerName = blobStorageDetails.Container;

            var blobContainer = blobClient.GetContainerReference(containerName);
            await blobContainer.DeleteIfExistsAsync();

            var compressedContainer = blobClient.GetContainerReference($"{containerName}-x");
            await compressedContainer.DeleteIfExistsAsync();
        }
    }
}
