using System;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Extensions.Options;
using Msr.Odr.Model;
using Msr.Odr.Model.Configuration;
using Msr.Odr.Model.Datasets;
using Msr.Odr.Model.UserData;
using Msr.Odr.Services.Configuration;

namespace Msr.Odr.Services
{
    /// <summary>
    /// Provides access to dataset owner edit functionality.
    /// </summary>
    public class DatasetEditStorageService : CosmosStorageService
    {
        private StorageConfiguration StorageConfig { get; }
        private SasTokenService SasTokens { get; }
        private DatasetOwnersService DatasetOwners { get; }
        public DatasetStorageService DatasetStorage { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DatasetEditStorageService" /> class.
        /// </summary>
        public DatasetEditStorageService(
            IOptions<CosmosConfiguration> cosmosConfig,
            IOptions<StorageConfiguration> storageConfig,
            SasTokenService sasTokens,
            DatasetOwnersService datasetOwnersService,
            DatasetStorageService datasetStorage)
            : base(cosmosConfig)
        {
            this.StorageConfig = storageConfig.Value;
            this.SasTokens = sasTokens;
            this.DatasetOwners = datasetOwnersService;
            this.DatasetStorage = datasetStorage;
        }

        public async Task<DatasetEditStorageItem> GetDatasetEditById(Guid id, IPrincipal user, CancellationToken cancellationToken)
        {
            var (original, modified) = await VerifyDatasetOwnership(id, user, cancellationToken);
            if(original == null)
            {
                return null;
            }

            if(modified == null)
            {
                modified = original.ToDatasetEditStorageItem();
            }

            return modified;
        }

        public async Task<DatasetEditStorageItem> UpdateDataset(Guid id, IPrincipal user, DatasetEditStorageItem updated, CancellationToken token)
        {
            var (original, current) = await VerifyDatasetOwnership(id, user, token);
            if(original == null)
            {
                throw new InvalidOperationException("Invalid dataset id.");
            }

            if(current == null)
            {
                current = original.ToDatasetEditStorageItem();
                current.EditStatus = DatasetEditStatus.DetailsModified;
            }

            current.Name = updated.Name;
            current.Description = updated.Description;
            current.Domain = updated.Domain;
            current.DomainId = updated.DomainId;
            current.SourceUri = updated.SourceUri;
            current.ProjectUri = updated.ProjectUri;
            current.Version = updated.Version;
            current.Published = updated.Published;
            current.License = updated.License;
            current.LicenseId = updated.LicenseId;
            current.Tags = updated.Tags;
            current.DigitalObjectIdentifier = updated.DigitalObjectIdentifier;
            current.IsDownloadAllowed = updated.IsDownloadAllowed;

            current.NominationLicenseType = updated.NominationLicenseType;
            switch (updated.NominationLicenseType)
            {
                case NominationLicenseType.Unknown:
                case NominationLicenseType.Standard:
                    current.OtherLicenseName = null;
                    current.OtherLicenseAdditionalInfoUrl = null;
                    current.OtherLicenseContentHtml = null;
                    current.OtherLicenseFileContent = null;
                    current.OtherLicenseFileContentType = null;
                    current.OtherLicenseFileName = null;
                    break;
                case NominationLicenseType.HtmlText:
                    current.OtherLicenseName = updated.OtherLicenseName;
                    current.OtherLicenseAdditionalInfoUrl = updated.OtherLicenseAdditionalInfoUrl;
                    current.OtherLicenseContentHtml = updated.OtherLicenseContentHtml;
                    current.OtherLicenseFileContent = null;
                    current.OtherLicenseFileContentType = null;
                    current.OtherLicenseFileName = null;
                    break;
                case NominationLicenseType.InputFile:
                    current.OtherLicenseName = updated.OtherLicenseName;
                    current.OtherLicenseAdditionalInfoUrl = updated.OtherLicenseAdditionalInfoUrl;
                    current.OtherLicenseContentHtml = null;
                    if(updated.OtherLicenseFileContent != null)
                    {
                        current.OtherLicenseFileContent = updated.OtherLicenseFileContent;
                        current.OtherLicenseFileContentType = updated.OtherLicenseFileContentType;
                        current.OtherLicenseFileName = updated.OtherLicenseFileName;
                    }
                    break;
            }

            return await UpdateDatasetEditItemDocument(user, current, token);
        }

        public async Task<DatasetEditStorageItem> InitiateDatasetContentEdit(Guid id, IPrincipal user, CancellationToken token)
        {
            var dataset = await GetDatasetEditById(id, user, token);
            if(dataset.EditStatus == DatasetEditStatus.ContentsModified)
            {
                // Already in content edit mode
                return dataset;
            }

            var details = await DatasetStorage.GetDatasetStorageDetails(id, token);
            var blobDetails = details as DatasetBlobStorageDetails;
            if (blobDetails == null)
            {
                throw new InvalidOperationException("Dataset storage must be blob storage.");
            }

            var datasetStorage = new DatasetStorage
            {
                Id = dataset.Id,
                DatasetName = dataset.Name,
                AccountName = blobDetails.Account
            };
            await SasTokens.FindUniqueDatasetUpdateContainerName(datasetStorage);

            await SasTokens.CreateDatasetContainer(datasetStorage);

            dataset.EditStatus = DatasetEditStatus.ContentsModified;
            dataset.ContentEditAccount = datasetStorage.AccountName;
            dataset.ContentEditContainer = datasetStorage.ContainerName;
            dataset.OriginalStorageAccount = blobDetails.Account;
            dataset.OriginalStorageContainer = blobDetails.Container;
            return await UpdateDatasetEditItemDocument(user, dataset, token);
        }

        public async Task<string> GetReadOnlySasTokenForOriginalDatasetContent(Guid id, IPrincipal user, CancellationToken token)
        {
            var dataset = await GetDatasetEditById(id, user, token);
            var containerToken = string.Empty;
            if(dataset.EditStatus == DatasetEditStatus.DetailsModified || dataset.EditStatus == DatasetEditStatus.ContentsModified)
            {
                var details = await DatasetStorage.GetDatasetStorageDetails(id, token);
                if(details is DatasetBlobStorageDetails blobDetails)
                {
                    containerToken = SasTokens.CreateContainerSasToken(blobDetails.Account, blobDetails.Container).ToString();
                }
            }

            return containerToken;
        }

        public async Task<string> GetReadWriteSasTokenForUpdatedDatasetContent(Guid id, IPrincipal user, CancellationToken token)
        {
            var dataset = await GetDatasetEditById(id, user, token);
            var containerToken = string.Empty;
            if(dataset.EditStatus == DatasetEditStatus.ContentsModified)
            {
                containerToken = (await SasTokens.GenerateSasTokenForUpdatingDatasetContainer(dataset.ContentEditAccount, dataset.ContentEditContainer)).ToString();
            }

            return containerToken;
        }

        public async Task<PublishUpdatedDatasetResult> PublishUpdatedDataset(Guid id, IPrincipal user, CancellationToken token)
        {
            var dataset = await GetDatasetEditById(id, user, token);
            var result = new PublishUpdatedDatasetResult
            {
                IsPublished = true,
                UsingStatus = dataset.EditStatus,
                DatasetId = dataset.Id,
                DatasetName = dataset.Name,
            };

            switch(dataset.EditStatus)
            {
                case DatasetEditStatus.DetailsModified:
                    // Only update the dataset database record.
                    await UpdateDatsetDocFromEditDoc(dataset, token);
                    break;
                case DatasetEditStatus.ContentsModified:
                    // Create a nomination for submission to the import batch process
                    await CreateNominationDocFromEditDoc(dataset, token);
                    break;
                default:
                    result.IsPublished = false;
                    break;
            }

            return result;
        }

        private async Task UpdateDatsetDocFromEditDoc(DatasetEditStorageItem dataset, CancellationToken token)
        {
            // Create the license document (if applicable)
            if (dataset.NominationLicenseType == NominationLicenseType.HtmlText || dataset.NominationLicenseType == NominationLicenseType.InputFile)
            {
                var license = await DatasetStorage.CreateLicense(dataset);
                if (license != null)
                {
                    dataset.LicenseId = license.Id;
                    dataset.License = license.Name;
                }
            }

            // Update the dataset record
            var docUri = CreateDatasetDocumentUri(dataset.Id);
            var reqOpts = new RequestOptions
            {
                PartitionKey = new PartitionKey(dataset.Id.ToString())
            };
            var document = await Client.ReadDocumentAsync<DatasetStorageItem>(docUri, reqOpts);
            var current = document.Document;
            current.Name = dataset.Name;
            current.Description = dataset.Description;
            current.Domain = dataset.Domain;
            current.DomainId = dataset.DomainId;
            current.SourceUri = dataset.SourceUri;
            current.ProjectUri = dataset.ProjectUri;
            current.Version = dataset.Version;
            current.Published = dataset.Published;
            current.License = dataset.License;
            current.LicenseId = dataset.LicenseId.GetValueOrDefault();
            current.Tags = dataset.Tags;
            current.DigitalObjectIdentifier = dataset.DigitalObjectIdentifier;
            current.IsDownloadAllowed = dataset.IsDownloadAllowed;
            await Client.ReplaceDocumentAsync(docUri, current, reqOpts);

            // Delete the original dataset edit record
            await Client.DeleteDocumentAsync(
                UserDataDocumentUriById(dataset.Id.ToString()),
                new RequestOptions
                {
                    PartitionKey = new PartitionKey(WellKnownIds.DatasetEditDatasetId.ToString())
                },
                token);

            // Update Azure Search with the latest document
            await DatasetStorage.UpdateDatasetDetailsInSearchIndex(dataset.Id, token);
        }

        private async Task CreateNominationDocFromEditDoc(DatasetEditStorageItem dataset, CancellationToken token)
        {
            // Disallow changes to the edited container
            await SasTokens.DisableSasTokenForUpdatingDatasetContainer(dataset.ContentEditAccount, dataset.ContentEditContainer);

            // Update the status of edit document.
            dataset.EditStatus = DatasetEditStatus.Importing;
            await Client.ReplaceDocumentAsync(
                UserDataDocumentUriById(dataset.Id.ToString()),
                dataset,
                new RequestOptions
                {
                    PartitionKey = new PartitionKey(WellKnownIds.DatasetEditDatasetId.ToString())
                },
                token);

            // Create nomination document
            var nomination = new DatasetNominationStorageItem
            {
                Id = dataset.Id,
                DatasetId = WellKnownIds.DatasetNominationDatasetId,
                Name = dataset.Name,
                Description = dataset.Description,
                Domain = dataset.Domain,
                DomainId = dataset.DomainId,
                SourceUri = dataset.SourceUri,
                ProjectUri = dataset.ProjectUri,
                Version = dataset.Version,
                Published = dataset.Published,
                Created = dataset.Created,
                Modified = dataset.Modified,
                License = dataset.License,
                LicenseId = dataset.LicenseId,
                Tags = (dataset.Tags ?? Enumerable.Empty<string>()).ToList(),
                ContactName = dataset.ContactName,
                ContactInfo = dataset.ContactInfo,
                CreatedByUserId = dataset.CreatedByUserId,
                CreatedByUserName = dataset.CreatedByUserName,
                CreatedByUserEmail = dataset.CreatedByUserEmail,
                ModifiedByUserName = dataset.ModifiedByUserName,
                ModifiedByUserEmail = dataset.ModifiedByUserEmail,
                IsDownloadAllowed = dataset.IsDownloadAllowed,
                NominationStatus = NominationStatus.Importing,
                NominationLicenseType = dataset.NominationLicenseType,
                DigitalObjectIdentifier = dataset.DigitalObjectIdentifier,
                OtherLicenseContentHtml = dataset.OtherLicenseContentHtml,
                OtherLicenseFileContent = dataset.OtherLicenseFileContent,
                OtherLicenseFileContentType = dataset.OtherLicenseFileContentType,
                OtherLicenseAdditionalInfoUrl = dataset.OtherLicenseAdditionalInfoUrl,
                OtherLicenseName = dataset.OtherLicenseName,
                OtherLicenseFileName = dataset.OtherLicenseFileName
            };
            await Client.UpsertDocumentAsync(
                UserDataDocumentCollectionUri,
                nomination,
                new RequestOptions
                {
                    PartitionKey = new PartitionKey(WellKnownIds.DatasetNominationDatasetId.ToString())
                },
                false,
                token);

            // Point nomination document to updated container
            var datasetRecordLink = new Attachment
            {
                Id = "Content",
                ContentType = "x-azure-blockstorage",
                MediaLink = SasTokens.GetContainerMediaLink(
                    dataset.ContentEditAccount,
                    dataset.ContentEditContainer)
            };
            datasetRecordLink.SetPropertyValue("storageType", "blob");
            datasetRecordLink.SetPropertyValue("container", dataset.ContentEditContainer);
            datasetRecordLink.SetPropertyValue("account", dataset.ContentEditAccount);
            await Client.UpsertAttachmentAsync(
                UserDataDocumentUriById(dataset.Id.ToString()),
                datasetRecordLink,
                new RequestOptions
                {
                    PartitionKey = new PartitionKey(WellKnownIds.DatasetNominationDatasetId.ToString())
                },
                token);
        }

        public async Task<bool> CleanUpDatasetEditAfterImport(Guid datasetId)
        {
            // Update Azure Search with the latest document
            await DatasetStorage.UpdateDatasetDetailsInSearchIndex(datasetId, default);

            // Retreive the dataset edit record (if present)
            var dataset = (await Client.CreateDocumentQuery<DatasetEditStorageItem>(
                UserDataDocumentCollectionUri,
                new FeedOptions
                {
                    PartitionKey = new PartitionKey(WellKnownIds.DatasetEditDatasetId.ToString())
                })
                .Where(d => d.Id == datasetId)
                .AsDocumentQuery()
                .GetQueryResultsAsync())
                .SingleOrDefault();
            if(dataset == null)
            {
                return false;
            }

            // Delete the original dataset container (if applicable)
            if (
                dataset.EditStatus == DatasetEditStatus.Importing &&
                !string.IsNullOrWhiteSpace(dataset.OriginalStorageAccount) &&
                !string.IsNullOrWhiteSpace(dataset.OriginalStorageContainer)
            )
            {
                await SasTokens.DeleteDatasetContainer(new DatasetStorage
                {
                    Id = dataset.Id,
                    DatasetName = dataset.Name,
                    AccountName = dataset.OriginalStorageAccount,
                    ContainerName = dataset.OriginalStorageContainer,
                });
            }

            // Delete the edit record
            await Client.DeleteDocumentAsync(
                UserDataDocumentUriById(datasetId.ToString()),
                new RequestOptions
                {
                    PartitionKey = new PartitionKey(WellKnownIds.DatasetEditDatasetId.ToString())
                },
                default);

            return true;
        }

        public async Task<bool> CancelDatasetChanges(Guid id, IPrincipal user, CancellationToken token)
        {
            var (original, modified) = await VerifyDatasetOwnership(id, user, token);
            if (original == null)
            {
                throw new InvalidOperationException("Invalid dataset id.");
            }

            var status = modified?.EditStatus ?? DatasetEditStatus.Unmodified;
            if (!(status == DatasetEditStatus.DetailsModified || status == DatasetEditStatus.ContentsModified))
            {
                return false;
            }

            await Client.DeleteDocumentAsync(
                UserDataDocumentUriById(id.ToString()),
                new RequestOptions
                {
                    PartitionKey = new PartitionKey(WellKnownIds.DatasetEditDatasetId.ToString())
                },
                token);

            if (modified.EditStatus == DatasetEditStatus.ContentsModified)
            {
                var datasetStorage = new DatasetStorage
                {
                    Id = id,
                    DatasetName = original.Name,
                    AccountName = modified.ContentEditAccount,
                    ContainerName = modified.ContentEditContainer,
                };

                await SasTokens.DeleteDatasetContainer(datasetStorage);
            }

            return true;
        }

        private async Task<(DatasetStorageItem Original, DatasetEditStorageItem Modified)> VerifyDatasetOwnership(Guid id, IPrincipal user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var originalDataset = (await Client.CreateDocumentQuery<DatasetStorageItem>(
                DatasetDocumentCollectionUri,
                new FeedOptions
                {
                    PartitionKey = new PartitionKey(id.ToString())
                })
                .Where(d => d.Id == id)
                .AsDocumentQuery()
                .GetQueryResultsAsync(cancellationToken))
                .SingleOrDefault();
            if(originalDataset == null)
            {
                return (null, null);
            }

            if (!DatasetOwners.IsUserDatasetOwner(originalDataset, user))
            {
                throw new DatasetOwnerException();
            }

            var modifiedDataset = (await Client.CreateDocumentQuery<DatasetEditStorageItem>(
                UserDataDocumentCollectionUri,
                new FeedOptions
                {
                    PartitionKey = new PartitionKey(WellKnownIds.DatasetEditDatasetId.ToString())
                })
                .Where(d => d.Id == id)
                .AsDocumentQuery()
                .GetQueryResultsAsync(cancellationToken))
                .SingleOrDefault();

            return (originalDataset, modifiedDataset);
        }

        private async Task<DatasetEditStorageItem> UpdateDatasetEditItemDocument(IPrincipal user, DatasetEditStorageItem updated, CancellationToken token)
        {
            updated.Modified = DateTime.UtcNow;
            updated.ModifiedByUserName = user.GetUserName();
            updated.ModifiedByUserEmail = user.GetUserEmail();

            var result = await Client.UpsertDocumentAsync(
                UserDataDocumentCollectionUri,
                updated,
                new RequestOptions
                {
                    PartitionKey = new PartitionKey(WellKnownIds.DatasetEditDatasetId.ToString())
                },
                false,
                token);

            updated = (dynamic)result.Resource;
            return updated;
        }
    }
}
