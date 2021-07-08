// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

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
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Extensions.Options;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Auth;
using Msr.Odr.Model;
using Msr.Odr.Model.Configuration;
using Msr.Odr.Model.Datasets;
using Msr.Odr.Model.UserData;
using Msr.Odr.Services.Configuration;
using Newtonsoft.Json.Linq;
using Microsoft.Azure.Storage.Blob;
using Newtonsoft.Json;

namespace Msr.Odr.Services
{
    /// <summary>
    /// Provides access to persisted, unindexed dataset details
    /// </summary>
    public class UserDataStorageService : UserClaimsStorageService
    {
        public UserDataStorageService(
            IOptions<CosmosConfiguration> options,
            SasTokenService sasTokens,
            ArmTemplatesMap armTemplatesMap,
            StaticAssetsMap staticAssetsMap,
            IOptions<WebServerConfiguration> webServerOptions,
            IOptions<BatchConfiguration> batchOptions,
            ValidationService validationService)
            : base(options, sasTokens)
        {
            ArmTemplatesMap = armTemplatesMap;
            StaticAssetsMap = staticAssetsMap;
            BatchOptions = batchOptions;
            ValidationService = validationService;
            WebServerConfiguration = webServerOptions.Value;
        }

        private ArmTemplatesMap ArmTemplatesMap { get; }
        private StaticAssetsMap StaticAssetsMap { get; }
        public IOptions<BatchConfiguration> BatchOptions { get; }
        private ValidationService ValidationService { get; }
        private WebServerConfiguration WebServerConfiguration { get; }

        /// <summary>
        /// Get license status
        /// </summary>
        /// <param name="datasetId">The dataset ID</param>
        /// <param name="user">The user</param>
        /// <param name="licenseId">The license ID</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<bool> GetLicenseStatusAsync(Guid datasetId, ClaimsPrincipal user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var userId = GetUserId(user);
            if (userId == null)
            {
                return false;
            }

            var dataset = await this.RetrieveDatasetDocument(datasetId, cancellationToken);
            if (dataset == null)
            {
                throw new InvalidOperationException("Invalid dataset id.");
            }

            var options = new FeedOptions
            {
                PartitionKey = new PartitionKey(datasetId.ToString()),
                EnableCrossPartitionQuery = true
            };

            try
            {
                var query = this.Client.CreateDocumentQuery<AcceptedLicenseStorage>(UserDataDocumentCollectionUri, options)
                                            .Where(f => f.DatasetId == datasetId &&
                                                        f.UserId == userId &&
                                                        f.LicenseId == dataset.LicenseId &&
                                                        f.DataType == UserDataTypes.AcceptedLicense)
                                            .AsDocumentQuery();

                var acceptedLicenses = await query.ExecuteNextAsync<AcceptedLicenseStorage>(cancellationToken)
                    .ConfigureAwait(false);

                if (acceptedLicenses.Any())
                {
                    return true;
                }
            }
            catch (Exception ex) when (ex.GetType().Name == "NotFoundException")
            {
                return false;
            }

            return false;
        }

        public async Task<OtherLicenseFile> GetOtherLicenseFileAsync(Guid nominationId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var options = new FeedOptions
            {
                PartitionKey = new PartitionKey(WellKnownIds.DatasetNominationDatasetId.ToString())
            };

            var query = this.Client.CreateDocumentQuery<DatasetNominationStorageItem>(UserDataDocumentCollectionUri, options)
                .Where(f => f.DataType == UserDataTypes.DatasetNomination &&
                            f.Id == nominationId &&
                            f.NominationLicenseType == NominationLicenseType.InputFile)
                .AsDocumentQuery();

            var storageItems = await query.ExecuteNextAsync<DatasetNominationStorageItem>(cancellationToken).ConfigureAwait(false);
            var storageItem = storageItems.FirstOrDefault();
            if (storageItem != null && !string.IsNullOrEmpty(storageItem.OtherLicenseFileContent))
            {
                var byteArray = Convert.FromBase64String(storageItem.OtherLicenseFileContent);

                return new OtherLicenseFile
                {
                    Content = byteArray,
                    FileName = storageItem.OtherLicenseFileName,
                    ContentType = storageItem.OtherLicenseFileContentType
                };
            }

            return null;
        }
        
        /// <summary>
        /// Accepts the license for the specified dataset and returns the current
        /// license identifier associated with the dataset.
        /// </summary>
        /// <param name="datasetId">The dataset ID</param>
        /// <param name="user">The user</param>
        /// <param name="reason">The reason for downloading the dataset</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The current license identifier for the dataset or null if the user is not authenticated</returns>
        public async Task<Guid?> AcceptLicenseAsync(Guid datasetId, ClaimsPrincipal user, string reason, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var doc = await this.RetrieveDatasetDocument(datasetId, cancellationToken);
            if (doc == null)
            {
                throw new InvalidOperationException("Invalid dataset id.");
            }

            var userId = GetUserId(user);
            if (!userId.HasValue)
            {
                return null;
            }
            var name = GetUserName(user);
            var email = GetUserEmail(user);

            var options = new RequestOptions
            {
                PartitionKey = new PartitionKey(datasetId.ToString())
            };
            var uri = this.UserDataDocumentCollectionUri;
            var record = new AcceptedLicenseStorage
            {
                DatasetId = datasetId,
                LicenseId = doc.LicenseId,
                UserId = userId.Value,
                UserEmail = email,
                UserName = name,
                ReasonForUse = reason
            };
            await this.Client.UpsertDocumentAsync(uri, record, options).ConfigureAwait(false);
            return doc.LicenseId;
        }

        public async Task<Guid?> CreateDatasetNominationAsync(ClaimsPrincipal user, DatasetNomination nomination,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var userId = GetUserId(user);
            if (!userId.HasValue)
            {
                return null;
            }
            var name = GetUserName(user);
            var email = GetUserEmail(user);
            var record = nomination.ToDatasetNominationStorageItem(d =>
            {
                d.CreatedByUserId = userId.Value.ToString();
                d.CreatedByUserName = name;
                d.CreatedByUserEmail = email;
                d.Created = DateTime.UtcNow;
                d.DatasetId = WellKnownIds.DatasetNominationDatasetId;
            });

            await this.Client.CreateDocumentAsync(UserDataDocumentCollectionUri, record).ConfigureAwait(false);
            return record.Id;
        }

        public async Task<Guid?> UpdateDatasetNominationAsync(ClaimsPrincipal user, DatasetNomination nomination, CancellationToken cancellationToken)
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
                PartitionKey = new PartitionKey(WellKnownIds.DatasetNominationDatasetId.ToString())
            };

            var uri = this.UserDataDocumentCollectionUri;
            var existingDocument = this.Client.CreateDocumentQuery<DatasetNominationStorageItem>(uri, options)
                .Where(r => r.Id == nomination.Id)
                .AsEnumerable()
                .SingleOrDefault();

            if (existingDocument == null)
            {
                throw new InvalidOperationException("Dataset Nomination not found.");
            }

            var storageItem = nomination.ToDatasetNominationStorageItem((d) =>
            {
                d.Created = existingDocument.Created;
                d.CreatedByUserId = existingDocument.CreatedByUserId;
                d.CreatedByUserName = existingDocument.CreatedByUserName;
                d.CreatedByUserEmail = existingDocument.CreatedByUserEmail;
                d.ModifiedByUserName = name;
                d.ModifiedByUserEmail = email;
                d.Modified = DateTime.UtcNow;
            });

            uri = UserDataDocumentUriById(storageItem.Id.ToString());
            await this.Client.ReplaceDocumentAsync(uri, storageItem);

            return nomination.Id;
        }

        

        /// <summary>
        /// Asynchronously gets the dataset by its identifier.
        /// </summary>
        /// <param name="id">The dataset nomination identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The matching dataset</returns>
        public async Task<DatasetNomination> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var options = new FeedOptions
            {
                EnableCrossPartitionQuery = true
            };

            var query = this.Client.CreateDocumentQuery<DatasetNominationStorageItem>(UserDataDocumentCollectionUri, options)
              .Where(f => f.DataType == UserDataTypes.DatasetNomination && f.Id == id)
              .AsDocumentQuery();

            var documents = await query.ExecuteNextAsync<DatasetNominationStorageItem>(cancellationToken).ConfigureAwait(false);
            var document = documents.FirstOrDefault();
            if (document == null)
            {
                return null;
            }

            return document.ToDatasetNomination();
        }

        /// <summary>
        /// Approves the nominated dataset
        /// </summary>
        /// <param name="id">The dataset ID</param>
        /// <param name="user">user approving the nomination</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The current identifier for the nominated dataset or null if nomination is not found</returns>
        public async Task<bool> ApproveNominationAsync(Guid id, ClaimsPrincipal user, CancellationToken cancellationToken)
        {
            var nomination = await GetByIdAsync(id, cancellationToken);
            if (nomination == null || nomination.NominationStatus != NominationStatus.PendingApproval)
            {
                return false;
            }

            var contentLink = CreateUserDataDocumentAttachmentUri(id, "Content");
            var options = new RequestOptions
            {
                PartitionKey = new PartitionKey(WellKnownIds.DatasetNominationDatasetId.ToString())
            };
            bool foundStorage = false;
            try
            {
                var response = await Client.ReadAttachmentAsync(contentLink.ToString(), options).ConfigureAwait(false);
                var resource = response?.Resource;
                if (resource != null)
                {
                    var accountName = resource.GetPropertyValue<string>("account");
                    var containerName = resource.GetPropertyValue<string>("container");
                    foundStorage = !string.IsNullOrWhiteSpace(accountName) && !string.IsNullOrWhiteSpace(containerName);
                }
            }
            catch (DocumentClientException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
            }

            NominationStatus status = foundStorage ? NominationStatus.Uploading : NominationStatus.Approved;
            return await UpdateNominationStatus(id, status, user, cancellationToken);
        }

        /// <summary>
        /// Creates the Azure storage container for the dataset and adds the attachment records
        /// to the nomination document.
        /// </summary>
        /// <param name="storage"></param>
        /// <param name="user"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<NominationStatus?> CreateDatasetStorageAsync(DatasetStorage storage, ClaimsPrincipal user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var docUri = CreateUserDataDocumentUri(storage.Id);
            var options = new RequestOptions
            {
                PartitionKey = new PartitionKey(WellKnownIds.DatasetNominationDatasetId.ToString())
            };

            Document document = await Client.ReadDocumentAsync(docUri, options);
            if (document == null)
            {
                return null;
            }

            var containerUri = await SasTokens.CreateDatasetContainer(storage);

            var name = GetUserName(user);
            var email = GetUserEmail(user);
            var status = NominationStatus.Uploading;

            document.SetPropertyValue("modified", DateTime.UtcNow);
            document.SetPropertyValue("modifiedByUserName", name);
            document.SetPropertyValue("modifiedByUserEmail", email);
            document.SetPropertyValue("nominationStatus", status.ToString());
            await Client.ReplaceDocumentAsync(document.SelfLink, document);

            var datasetRecordLink = new Attachment
            {
                Id = "Content",  // "Slug" is ID with hard-attach
                ContentType = "x-azure-blockstorage",
                MediaLink = containerUri,
            };

            datasetRecordLink.SetPropertyValue("storageType", "blob");
            datasetRecordLink.SetPropertyValue("container", storage.ContainerName);
            datasetRecordLink.SetPropertyValue("account", storage.AccountName);

            await Client.UpsertAttachmentAsync(document.SelfLink, datasetRecordLink, options);

            return status;
        }

        /// <summary>
        /// Rejects the nominated dataset
        /// </summary>
        /// <param name="id">The dataset ID</param>
        /// <param name="user">user rejecting the nomination</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The current identifier for the nominated dataset or null if nomination is not found</returns>
        public async Task<bool> RejectNominationAsync(Guid id, ClaimsPrincipal user, CancellationToken cancellationToken)
        {
            return await UpdateNominationStatus(id, NominationStatus.Rejected, user, cancellationToken);
        }

        public async Task<DatasetImportProperties> GetDatasetImportPropertiesForNomination(Guid id, CancellationToken cancellationToken)
        {
            var nomination = await GetByIdAsync(id, cancellationToken);
            if (nomination == null)
            {
                return null;
            }

            var contentLink = CreateUserDataDocumentAttachmentUri(id, "Content");
            var options = new RequestOptions
            {
                PartitionKey = new PartitionKey(WellKnownIds.DatasetNominationDatasetId.ToString())
            };
            var response = await Client.ReadAttachmentAsync(contentLink.ToString(), options).ConfigureAwait(false);
            var resource = response?.Resource;
            if (resource == null)
            {
                return null;
            }

            var accountName = resource.GetPropertyValue<string>("account");
            var containerName = resource.GetPropertyValue<string>("container");
            var accessToken = await SasTokens.GenerateSasTokenForUpdatingDatasetContainer(accountName, containerName);

            return new DatasetImportProperties
            {
                Id = nomination.Id,
                DatasetName = nomination.Name,
                AccessToken = accessToken.ToString(),
                ContainerName = containerName,
                AccountName = accountName,
            };
        }

        public async Task<bool> UpdateNominationStatus(Guid id, NominationStatus status, ClaimsPrincipal user, CancellationToken cancellationToken)
        {
            var (found, _) = await UpdateNominationStatusAndReturnName(id, status, user, cancellationToken);
            return found;
        }

        public async Task<(bool found, string name)> UpdateNominationStatusAndReturnName(
            Guid id,
            NominationStatus status,
            ClaimsPrincipal user,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var docUri = CreateUserDataDocumentUri(id);
            var options = new RequestOptions
            {
                PartitionKey = new PartitionKey(WellKnownIds.DatasetNominationDatasetId.ToString())
            };

            var nomination = await Client.ReadDocumentAsync<DatasetNominationStorageItem>(docUri, options);
            if (nomination == null)
            {
                return (false, null);
            }

            if (!IsValidTransition(nomination.Document.NominationStatus, status))
            {
                throw new InvalidOperationException($"Invalid transition of nomination to {status} state.");
            }

            nomination.Document.Modified = DateTime.UtcNow;
            nomination.Document.NominationStatus = status;

            if (user != null)
            {
                var name = GetUserName(user);
                var email = GetUserEmail(user);
                nomination.Document.ModifiedByUserName = name;
                nomination.Document.ModifiedByUserEmail = email;
            }

            await Client.ReplaceDocumentAsync(docUri, nomination.Document);

            return (true, nomination.Document.Name);
        }

        private bool IsValidTransition(NominationStatus fromStatus, NominationStatus toStatus)
        {
            switch (toStatus)
            {
                case NominationStatus.Approved:
                    return fromStatus == NominationStatus.PendingApproval;
                case NominationStatus.Uploading:
                    return fromStatus == NominationStatus.Approved || fromStatus == NominationStatus.PendingApproval;
                case NominationStatus.Importing:
                    return fromStatus == NominationStatus.Uploading;
                case NominationStatus.Complete:
                    return fromStatus == NominationStatus.Importing || fromStatus == NominationStatus.Error;
                case NominationStatus.PendingApproval:
                case NominationStatus.Rejected:
                case NominationStatus.Error:
                    return true;
                default:
                    return false;
            }
        }

        public async Task<Guid?> SubmitDatasetIssue(ClaimsPrincipal user, DatasetIssue issue, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var doc = await this.RetrieveDatasetDocument(issue.DatasetId, cancellationToken);
            if (doc == null)
            {
                throw new InvalidOperationException("Invalid dataset id.");
            }

            var userId = GetUserId(user);
            if (!userId.HasValue)
            {
                return null;
            }
            var name = GetUserName(user);
            var email = GetUserEmail(user);

            var options = new RequestOptions
            {
                PartitionKey = new PartitionKey(issue.DatasetId.ToString())
            };
            var uri = this.UserDataDocumentCollectionUri;
            var record = new DatasetIssueStorageItem
            {
                Id = Guid.NewGuid(),
                DatasetId = issue.DatasetId,
                Name = issue.Name,
                Description = issue.Description,
                ContactName = issue.ContactName,
                ContactInfo = issue.ContactInfo,
                UserId = userId.Value,
                UserName = name,
                UserEmail = email,
            };
            await this.Client.CreateDocumentAsync(uri, record, options).ConfigureAwait(false);
            return record.Id;
        }

        public async Task<Guid?> SubmitGeneralIssue(ClaimsPrincipal user, GeneralIssue issue, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var userId = GetUserId(user);
            if (!userId.HasValue)
            {
                return null;
            }
            var name = GetUserName(user);
            var email = GetUserEmail(user);

            var options = new RequestOptions
            {
                PartitionKey = new PartitionKey(WellKnownIds.GeneralIssueDatasetId.ToString())
            };
            var uri = this.UserDataDocumentCollectionUri;
            var record = new GeneralIssueStorageItem
            {
                Id = Guid.NewGuid(),
                DatasetId = WellKnownIds.GeneralIssueDatasetId,
                Name = issue.Name,
                Description = issue.Description,
                ContactName = issue.ContactName,
                ContactInfo = issue.ContactInfo,
                UserId = userId.Value,
                UserName = name,
                UserEmail = email,
            };
            await Client.CreateDocumentAsync(uri, record, options).ConfigureAwait(false);
            return record.Id;
        }

        public async Task<Guid?> CreateDeployment(
            DeploymentCreation deployment,
            Uri storageUri,
            ClaimsPrincipal user,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var doc = await this.RetrieveDatasetDocument(deployment.DatasetId, cancellationToken);
            if (doc == null)
            {
                return null;
            }

            var userId = GetUserId(user);
            if (!userId.HasValue)
            {
                throw new InvalidOperationException("Invalid user information.");
            }
            var name = GetUserName(user);
            var email = GetUserEmail(user);

            var options = new RequestOptions
            {
                PartitionKey = new PartitionKey(deployment.DatasetId.ToString())
            };
            var uri = this.UserDataDocumentCollectionUri;
            var record = new DeploymentStorage
            {
                Id = Guid.NewGuid(),
                DatasetId = deployment.DatasetId,
                DeploymentId = deployment.DeploymentId,
                StorageUri = storageUri.ToString(),
                UserId = userId.Value,
                UserEmail = email,
                UserName = name,
            };
            await this.Client.UpsertDocumentAsync(uri, record, options).ConfigureAwait(false);
            return record.Id;
        }

        public async Task<string> GenerateDeploymentTemplate(
            Guid datasetId,
            Guid deploymentId,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Get the deployment instance document
            DeploymentStorage deployment;
            var options = new RequestOptions
            {
                PartitionKey = new PartitionKey(datasetId.ToString())
            };
            var uri = UserDataDocumentUriById(deploymentId.ToString());
            try
            {
                var response = await Client.ReadDocumentAsync<DeploymentStorage>(uri, options);
                deployment = response.Document;
            }
            catch (Microsoft.Azure.Documents.DocumentClientException ex)
            {
                if (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }
                throw;
            }

            // Get the ARM template
            ArmTemplatesItem templateItem;
            Document templateDoc;
            if (!ArmTemplatesMap.TryGetValue(deployment.DeploymentId, out templateItem))
            {
                return null;
            }
            options = new RequestOptions
            {
                PartitionKey = new PartitionKey(WellKnownIds.ConfigurationDatasetId.ToString())
            };
            uri = DatasetDocumentUriById(templateItem.Id);
            try
            {
                var response = await Client.ReadDocumentAsync(uri, options);
                templateDoc = response.Resource;
            }
            catch (Microsoft.Azure.Documents.DocumentClientException ex)
            {
                if (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }
                throw;
            }

            // Get the dataset storage document
            DatasetStorageItem dataset;
            options = new RequestOptions
            {
                PartitionKey = new PartitionKey(datasetId.ToString())
            };
            uri = DatasetDocumentUriById(datasetId.ToString());
            try
            {
                var response = await Client.ReadDocumentAsync<DatasetStorageItem>(uri, options);
                dataset = response.Document;
            }
            catch (Microsoft.Azure.Documents.DocumentClientException ex)
            {
                if (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }
                throw;
            }

            // if synapse workspace option is selected in deploy to Azure option
            // to populate template with the selected dataset to deploy on Azure
            if (deployment.DeploymentId.Contains("synapse"))
            {
                var doc = templateDoc.GetPropertyValue<JObject>("template");

                var template = JsonConvert.SerializeObject(doc);
                var templateObj = JsonConvert.DeserializeObject<Dictionary<string, object>>(template);
                var variablesString = JsonConvert.SerializeObject(templateObj["variables"]);
                var variablesObj = JsonConvert.DeserializeObject<Dictionary<string, object>>(variablesString);
                variablesObj["givenDataSet"] = deployment.StorageUri;
                templateObj["variables"] = variablesObj;
                return JsonConvert.SerializeObject(templateObj, Formatting.Indented);
            }
            else
            {
                // Set default values for ARM template parameters for other options then Synapse WorkSpace
                var defaultUserName = "dsuser";
                var nameRegex = new Regex(@"[^a-z0-9]", RegexOptions.IgnoreCase);
                string deploymentName = string.IsNullOrWhiteSpace(dataset.Name)
                    ? "datasetname"
                    : nameRegex.Replace(dataset.Name, (m) => string.Empty);
                var parametersMap = new Dictionary<string, string>
                {
                    { "adminUsername", defaultUserName },
                    { "datasetUrl", deployment.StorageUri },
                    { "datasetPath", $"/home/{defaultUserName}/datasets/{deploymentName}" },
                    { "datasetDirectory", $"C:\\Datasets\\{deploymentName}" },
                };
                var variablesMap = new Dictionary<string, string>
                {
                    { "assetsRootUrl", $"{WebServerConfiguration.URL}azure-deploy/assets" },
                };
                var doc = templateDoc.GetPropertyValue<JObject>("template");
                doc
                    .GetValue("parameters")
                    .Cast<JProperty>()
                    .Select(p => new
                    {
                        p.Name,
                        Value = (JObject)p.Value
                    })
                    .ToList()
                    .ForEach(v =>
                    {
                        string value;
                        if (parametersMap.TryGetValue(v.Name, out value))
                        {
                            v.Value["defaultValue"] = value;
                        }
                    });
                doc
                    .GetValue("variables")
                    .Cast<JProperty>()
                    .ToList()
                    .ForEach(prop =>
                    {
                        string value;
                        if (variablesMap.TryGetValue(prop.Name, out value))
                        {
                            prop.Value = value;
                        }
                    });
                return Newtonsoft.Json.JsonConvert.SerializeObject(doc, Newtonsoft.Json.Formatting.Indented);
            }
        }

        public async Task<StaticAssetItem> GenerateDeploymentAsset(
            string name,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Get the static asset document
            StaticAssetsItem assetItem;
            Document assetDoc;
            if (!StaticAssetsMap.TryGetValue(name, out assetItem))
            {
                return null;
            }
            var options = new RequestOptions
            {
                PartitionKey = new PartitionKey(WellKnownIds.ConfigurationDatasetId.ToString())
            };
            var uri = DatasetDocumentUriById(assetItem.Id);
            try
            {
                var response = await Client.ReadDocumentAsync(uri, options);
                assetDoc = response.Resource;
            }
            catch (Microsoft.Azure.Documents.DocumentClientException ex)
            {
                if (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }
                throw;
            }

            return new StaticAssetItem
            {
                Name = name,
                MimeType = assetDoc.GetPropertyValue<string>("mimeType"),
                Content = assetDoc.GetPropertyValue<string>("content"),
            };
        }

        public async Task<Stream> OpenDatasetUtil(string datasetUtilStorageUrl)
        {
            if(string.IsNullOrWhiteSpace(datasetUtilStorageUrl))
            {
                throw new ArgumentNullException(nameof(datasetUtilStorageUrl));
            }

            var addr = new Uri(datasetUtilStorageUrl);
            var names = addr.AbsolutePath.Split('/');
            var credentials = new StorageCredentials(BatchOptions.Value.StorageName, BatchOptions.Value.StorageKey);
            var storageAcct = new CloudStorageAccount(credentials, true);
            var blobClient = storageAcct.CreateCloudBlobClient();
            var blobContainer = blobClient.GetContainerReference(names[1]);
            var blobRef = blobContainer.GetBlockBlobReference(names[2]);
            return await blobRef.OpenReadAsync();
        }

        //public async Task RefreshLicenseAttachment(DatasetNomination nomination, string selfLink)
        //{
        //    if (nomination.NominationLicenseType == NominationLicenseType.HtmlText)
        //    {
        //        RequestOptions dbOptions = new RequestOptions
        //        {
        //            PartitionKey = new PartitionKey(LicenseStorageItem.LicenseDatasetId.ToString())
        //        };

        //        var contentRecordOptions = new MediaOptions
        //        {
        //            Slug = "Content",  // "Slug" is ID with hard-attach
        //            ContentType = "text/plain"
        //        };

        //        await Client.UpsertAttachmentAsync(
        //            selfLink,
        //            new MemoryStream(Encoding.UTF8.GetBytes(nomination.OtherLicenseContentHtml)),
        //            contentRecordOptions,
        //            dbOptions);
        //    }
        //}
    }
}
