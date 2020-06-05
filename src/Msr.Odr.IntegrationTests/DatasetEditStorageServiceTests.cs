// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Auth;
using Microsoft.Azure.Storage.Blob;
using Msr.Odr.IntegrationTests.Setup;
using Msr.Odr.Model;
using Msr.Odr.Services;
using Msr.Odr.Services.Configuration;
using Xunit;
using Xunit.Abstractions;
using System.Linq;
using System.Threading;
using Msr.Odr.Model.Datasets;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents;
using Msr.Odr.Model.UserData;
using Msr.Odr.Model.Configuration;

namespace Msr.Odr.IntegrationTests
{
    [Collection(ServicesFixture.Name)]
    public class DatasetEditStorageServiceTests
    {
        const string TestUserName = "Some User";
        const string TestUserEmail = "some.user@microsoft.com";

        private ServicesFixture Services { get; }
        private ITestOutputHelper Output { get; }

        public DatasetEditStorageServiceTests(ServicesFixture services, ITestOutputHelper output)
        {
            Services = services;
            Output = output;
        }

        [Fact]
        public async Task ShouldGetUnmodifiedDatasetEdit()
        {
            await TestUtils.ExecAndCleanup(async cleanup =>
            {
                var dataset = CreateDatasetDoc();
                cleanup.Push(await SetupDataset(dataset));

                var editService = Services.GetService<DatasetEditStorageService>();
                var user = CreateTestUser();

                var updated = await editService.GetDatasetEditById(dataset.Id, user, default);
                Assert.NotNull(updated);
                Assert.Equal(dataset.Id, updated.Id);
                Assert.Equal(DatasetEditStatus.Unmodified, updated.EditStatus);
            });
        }

        [Fact]
        public async Task ShouldUpdateDatasetDetails()
        {
            await TestUtils.ExecAndCleanup(async cleanup =>
            {
                var dataset = CreateDatasetDoc();
                cleanup.Push(await SetupDataset(dataset));

                var editService = Services.GetService<DatasetEditStorageService>();
                var user = CreateTestUser();

                var updated = await editService.GetDatasetEditById(dataset.Id, user, default);
                updated.Description = "My new description.";
                updated = await editService.UpdateDataset(dataset.Id, user, updated, default);
                Assert.Equal("My new description.", updated.Description);
                Assert.Equal(DatasetEditStatus.DetailsModified, updated.EditStatus);

                updated = await editService.GetDatasetEditById(dataset.Id, user, default);
                Assert.Equal(dataset.Id, updated.Id);
                Assert.Equal("My new description.", updated.Description);
                Assert.Equal(DatasetEditStatus.DetailsModified, updated.EditStatus);
            });
        }

        [Fact]
        public async Task ShouldCancelUpdateDatasetDetails()
        {
            await TestUtils.ExecAndCleanup(async cleanup =>
            {
                var dataset = CreateDatasetDoc();
                cleanup.Push(await SetupDataset(dataset));

                var editService = Services.GetService<DatasetEditStorageService>();
                var user = CreateTestUser();

                var updated = await editService.GetDatasetEditById(dataset.Id, user, default);
                updated.Description = "My new description.";
                await editService.UpdateDataset(dataset.Id, user, updated, default);

                await editService.CancelDatasetChanges(dataset.Id, user, default);

                updated = await editService.GetDatasetEditById(dataset.Id, user, default);
                Assert.Equal(dataset.Id, updated.Id);
                Assert.Equal("Lorem ipsum ...", updated.Description);
                Assert.Equal(DatasetEditStatus.Unmodified, updated.EditStatus);
            });
        }

        [Fact]
        public async Task ShouldUpdateDatasetContent()
        {
            await TestUtils.ExecAndCleanup(async cleanup =>
            {
                var dataset = CreateDatasetDoc();
                DatasetStorage datasetStorage = null;
                cleanup.Push(await SetupDataset(dataset, (storage) =>
                {
                    datasetStorage = storage;
                }));

                var editService = Services.GetService<DatasetEditStorageService>();
                var user = CreateTestUser();

                var updated = await editService.InitiateDatasetContentEdit(dataset.Id, user, default);
                Assert.Equal(DatasetEditStatus.ContentsModified, updated.EditStatus);
                Assert.False(string.IsNullOrWhiteSpace(updated.ContentEditAccount));
                Assert.False(string.IsNullOrWhiteSpace(updated.ContentEditContainer));
                Assert.Equal(datasetStorage.AccountName, updated.OriginalStorageAccount);
                Assert.Equal(datasetStorage.ContainerName, updated.OriginalStorageContainer);

                var blobClient = await Services.GetBlobClient();
                var container = blobClient.GetContainerReference(updated.ContentEditContainer);
                var exists = await container.ExistsAsync();
                Assert.True(exists);

                cleanup.Push(async () =>
                {
                    var cosmosClient = await Services.GetCosmosClient();
                    var cosmosConfig = Services.GetService<IOptions<CosmosConfiguration>>().Value;
                    var datasetId = dataset.Id.ToString();
                    await cosmosClient.DeleteDocumentAsync(
                        UriFactory.CreateDocumentUri(cosmosConfig.Database, cosmosConfig.UserDataCollection, datasetId),
                        new RequestOptions
                        {
                            PartitionKey = new PartitionKey(WellKnownIds.DatasetEditDatasetId.ToString())
                        });
                    await container.DeleteAsync();
                });

                updated = await editService.GetDatasetEditById(dataset.Id, user, default);
                Assert.Equal(dataset.Id, updated.Id);
                Assert.Equal(DatasetEditStatus.ContentsModified, updated.EditStatus);
                Assert.False(string.IsNullOrWhiteSpace(updated.ContentEditAccount));
                Assert.False(string.IsNullOrWhiteSpace(updated.ContentEditContainer));
            });
        }

        [Fact]
        public async Task ShouldCancelUpdateDatasetContent()
        {
            await TestUtils.ExecAndCleanup(async cleanup =>
            {
                var dataset = CreateDatasetDoc();
                cleanup.Push(await SetupDataset(dataset));

                var editService = Services.GetService<DatasetEditStorageService>();
                var user = CreateTestUser();

                var updated = await editService.InitiateDatasetContentEdit(dataset.Id, user, default);
                var blobClient = await Services.GetBlobClient();
                var container = blobClient.GetContainerReference(updated.ContentEditContainer);
                foreach (var testFile in TestFiles)
                {
                    var blob = container.GetBlockBlobReference($"{testFile}-updated.txt");
                    var content = $"{testFile}, updated {DateTime.UtcNow.ToString()}";
                    await blob.UploadTextAsync(content);
                }

                await editService.CancelDatasetChanges(dataset.Id, user, default);

                updated = await editService.GetDatasetEditById(dataset.Id, user, default);
                Assert.Equal(dataset.Id, updated.Id);
                Assert.Equal(DatasetEditStatus.Unmodified, updated.EditStatus);

                var exists = await container.ExistsAsync();
                Assert.False(exists);
            });
        }

        [Fact]
        public async Task ShouldPublishUpdateDatasetDetails()
        {
            await TestUtils.ExecAndCleanup(async cleanup =>
            {
                var dataset = CreateDatasetDoc();
                cleanup.Push(await SetupDataset(dataset));

                var editService = Services.GetService<DatasetEditStorageService>();
                var user = CreateTestUser();

                var updated = await editService.GetDatasetEditById(dataset.Id, user, default);
                updated.Description = "My new description.";
                updated = await editService.UpdateDataset(dataset.Id, user, updated, default);

                var result = await editService.PublishUpdatedDataset(dataset.Id, user, default);
                Assert.True(result.IsPublished);
                Assert.Equal(DatasetEditStatus.DetailsModified, result.UsingStatus);
                Assert.False(result.ShouldQueueBatchOperation);
                Assert.Equal(result.DatasetId, dataset.Id);
                Assert.Equal(result.DatasetName, dataset.Name);

                updated = await editService.GetDatasetEditById(dataset.Id, user, default);
                Assert.Equal(dataset.Id, updated.Id);
                Assert.Equal("My new description.", updated.Description);
                Assert.Equal(DatasetEditStatus.Unmodified, updated.EditStatus);
                Assert.Null(updated.ContentEditAccount);
                Assert.Null(updated.ContentEditContainer);
                Assert.Null(updated.OriginalStorageAccount);
                Assert.Null(updated.OriginalStorageContainer);
            });
        }

        [Fact]
        public async Task ShouldPublishUpdateDatasetContent()
        {
            await TestUtils.ExecAndCleanup(async cleanup =>
            {
                var dataset = CreateDatasetDoc();
                cleanup.Push(await SetupDataset(dataset));

                var editService = Services.GetService<DatasetEditStorageService>();
                var user = CreateTestUser();

                var updated = await editService.InitiateDatasetContentEdit(dataset.Id, user, default);
                var blobClient = await Services.GetBlobClient();
                var container = blobClient.GetContainerReference(updated.ContentEditContainer);
                foreach (var testFile in TestFiles)
                {
                    var blob = container.GetBlockBlobReference($"{testFile}-updated.txt");
                    var content = $"{testFile}, updated {DateTime.UtcNow.ToString()}";
                    await blob.UploadTextAsync(content);
                }

                var result = await editService.PublishUpdatedDataset(dataset.Id, user, default);
                Assert.True(result.IsPublished);
                Assert.Equal(DatasetEditStatus.ContentsModified, result.UsingStatus);
                Assert.True(result.ShouldQueueBatchOperation);
                Assert.Equal(result.DatasetId, dataset.Id);
                Assert.Equal(result.DatasetName, dataset.Name);

                var cosmosClient = await Services.GetCosmosClient();
                var cosmosConfig = Services.GetService<IOptions<CosmosConfiguration>>().Value;
                var docUri = UriFactory.CreateDocumentUri(cosmosConfig.Database, cosmosConfig.UserDataCollection, dataset.Id.ToString());
                var reqOpts = new RequestOptions
                {
                    PartitionKey = new PartitionKey(WellKnownIds.DatasetNominationDatasetId.ToString())
                };
                cleanup.Push(async () =>
                {
                    await cosmosClient.DeleteDocumentAsync(docUri, reqOpts);
                    await container.DeleteAsync();
                });

                updated = await editService.GetDatasetEditById(dataset.Id, user, default);
                Assert.Equal(dataset.Id, updated.Id);
                Assert.Equal(DatasetEditStatus.Importing, updated.EditStatus);
                Assert.NotNull(updated.ContentEditAccount);
                Assert.NotNull(updated.ContentEditContainer);
                Assert.NotNull(updated.OriginalStorageAccount);
                Assert.NotNull(updated.OriginalStorageContainer);

                var nomination = (await cosmosClient.ReadDocumentAsync<DatasetNominationStorageItem>(docUri, reqOpts)).Document;
                Assert.Equal(dataset.Id, nomination.Id);
                Assert.Equal(dataset.Name, nomination.Name);
                Assert.Equal(NominationStatus.Importing, nomination.NominationStatus);

                var nominationLink = await cosmosClient.ReadAttachmentAsync(
                    UriFactory.CreateAttachmentUri(cosmosConfig.Database, cosmosConfig.UserDataCollection, dataset.Id.ToString(), "Content"),
                    reqOpts);
                Assert.Equal(container.Uri.ToString(), nominationLink.Resource.MediaLink);
            });
        }

        [Fact]
        public async Task ShouldCleanUpDatasetAfterImport()
        {
            await TestUtils.ExecAndCleanup(async cleanup =>
            {
                var dataset = CreateDatasetDoc();
                string originalStorageContainer = null;
                cleanup.Push(await SetupDataset(dataset, (storage) =>
                {
                    originalStorageContainer = storage.ContainerName;
                }));

                var editService = Services.GetService<DatasetEditStorageService>();
                var user = CreateTestUser();

                var updated = await editService.InitiateDatasetContentEdit(dataset.Id, user, default);
                var blobClient = await Services.GetBlobClient();
                var container = blobClient.GetContainerReference(updated.ContentEditContainer);
                foreach (var testFile in TestFiles)
                {
                    var blob = container.GetBlockBlobReference($"{testFile}-updated.txt");
                    var content = $"{testFile}, updated {DateTime.UtcNow.ToString()}";
                    await blob.UploadTextAsync(content);
                }

                await UpdateDatasetEditDoc(dataset.Id, doc =>
                {
                    doc.EditStatus = DatasetEditStatus.Importing;
                });

                var result = await editService.CleanUpDatasetEditAfterImport(dataset.Id);
                Assert.True(result);

                updated = await editService.GetDatasetEditById(dataset.Id, user, default);
                Assert.Equal(dataset.Id, updated.Id);
                Assert.Equal(DatasetEditStatus.Unmodified, updated.EditStatus);
                Assert.Null(updated.ContentEditAccount);
                Assert.Null(updated.ContentEditContainer);
                Assert.Null(updated.OriginalStorageAccount);
                Assert.Null(updated.OriginalStorageContainer);

                container = blobClient.GetContainerReference(originalStorageContainer);
                var exists = await container.ExistsAsync();
                Assert.False(exists);
            });
        }

        private ClaimsPrincipal CreateTestUser()
        {
            return new ClaimsPrincipal(
                new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Email, TestUserEmail)
                })
            );
        }

        private DatasetStorageItem CreateDatasetDoc(Action<DatasetStorageItem> initFn = null)
        {
            var id = Guid.NewGuid();
            var dataset = new DatasetStorageItem
            {
                Id = id,
                Name = $"Automated Test Dataset {id.ToString()}",
                Description = "Lorem ipsum ...",
                SourceUri = "https://www.microsoft.com/en-us/research/tools/?facet%5Btax%5D%5Bmsr-product-type%5D%5B%5D=243083",
                ProjectUri = "https://www.microsoft.com/en-us/research/",
                Version = "1.2.3",
                Published = new DateTime(2018, 12, 13).ToUniversalTime(),
                DigitalObjectIdentifier = "/Some/Id",
                Created = new DateTime(2019, 7, 1).ToUniversalTime(),
                CreatedByUserEmail = TestUserEmail,
                CreatedByUserName = TestUserName,
                License = "Creative Commons",
                LicenseId = new Guid("19c49d7e-16e9-4f2d-b914-439b0488cdbe"),
                Domain = "computer science",
                DomainId = "COMPUTER SCIENCE",
                DatasetOwners = new List<DatasetOwner>
                {
                    new DatasetOwner
                    {
                        Name = TestUserName,
                        Email = TestUserEmail,
                    }
                }
            };
            initFn?.Invoke(dataset);
            return dataset;
        }

        private readonly string[] TestFiles = new string[]
        {
            "One",
            "Two",
            "Three",
        };

        private async Task<Func<Task>> SetupDataset(DatasetStorageItem dataset, Action<DatasetStorage> setParamsFn = null)
        {
            var sasTokens = Services.GetService<SasTokenService>();
            var datasetStorage = new DatasetStorage
            {
                Id = dataset.Id,
                DatasetName = dataset.Name,
                AccountName = sasTokens.DefaultDatasetStorageAccount(),
            };
            await sasTokens.FindUniqueDatasetContainerName(datasetStorage);
            await sasTokens.CreateDatasetContainer(datasetStorage);
            var blobClient = await Services.GetBlobClient();
            var container = blobClient.GetContainerReference(datasetStorage.ContainerName);
            foreach(var testFile in TestFiles)
            {
                var blob = container.GetBlockBlobReference($"{testFile}.txt");
                var content = $"{testFile}, generated {DateTime.UtcNow.ToString()}";
                await blob.UploadTextAsync(content);
            }

            var datasetId = dataset.Id.ToString();
            var requestOptions = new RequestOptions
            {
                PartitionKey = new PartitionKey(datasetId)
            };
            var cosmosConfig = Services.GetService<IOptions<CosmosConfiguration>>().Value;
            var cosmosClient = await Services.GetCosmosClient();
            var response = await cosmosClient.CreateDocumentAsync(
                UriFactory.CreateDocumentCollectionUri(cosmosConfig.Database, cosmosConfig.DatasetCollection),
                dataset,
                requestOptions);

            var containerDetails = new DatasetItemContainerDetails
            {
                DatasetId = dataset.Id,
                Account = datasetStorage.AccountName,
                Container = datasetStorage.ContainerName,
                Uri = container.Uri.ToString(),
            };
            var link = new Attachment
            {
                Id = containerDetails.Name,
                ContentType = containerDetails.ContentType,
                MediaLink = containerDetails.Uri
            };
            link.SetPropertyValue("storageType", "blob");
            link.SetPropertyValue("container", containerDetails.Container);
            link.SetPropertyValue("account", containerDetails.Account);
            await cosmosClient.UpsertAttachmentAsync(
                response.Resource.SelfLink,
                link,
                requestOptions);

            setParamsFn?.Invoke(datasetStorage);

            return async () =>
            {
                await cosmosClient.DeleteDocumentAsync(
                    UriFactory.CreateDocumentUri(cosmosConfig.Database, cosmosConfig.DatasetCollection, datasetId),
                    requestOptions);
                await sasTokens.DeleteDatasetContainer(datasetStorage);
            };
        }

        private async Task UpdateDatasetEditDoc(Guid datasetId, Action<DatasetEditStorageItem> updateFn)
        {
            var cosmosClient = await Services.GetCosmosClient();
            var cosmosConfig = Services.GetService<IOptions<CosmosConfiguration>>().Value;
            var docUri = UriFactory.CreateDocumentUri(cosmosConfig.Database, cosmosConfig.UserDataCollection, datasetId.ToString());
            var reqOpts = new RequestOptions
            {
                PartitionKey = new PartitionKey(WellKnownIds.DatasetEditDatasetId.ToString())
            };
            var datasetDoc = await cosmosClient.ReadDocumentAsync<DatasetEditStorageItem>(docUri, reqOpts);
            updateFn(datasetDoc.Document);
            await cosmosClient.ReplaceDocumentAsync(docUri, datasetDoc.Document, reqOpts);
        }
    }
}
