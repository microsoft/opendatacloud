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

namespace Msr.Odr.IntegrationTests
{
    [Collection(ServicesFixture.Name)]
    public class SasTokensTests
    {
        private ServicesFixture Services { get; }
        private ITestOutputHelper Output { get; }

        public SasTokensTests(ServicesFixture services, ITestOutputHelper output)
        {
            Services = services;
            Output = output;
        }

        [Fact]
        public void ShouldHaveDefaultStorageAccount()
        {
            var sasTokens = Services.GetService<SasTokenService>();
            Assert.True(!string.IsNullOrWhiteSpace(sasTokens.DefaultDatasetStorageAccount()));
            Output.WriteLine($"Default storage account: {sasTokens.DefaultDatasetStorageAccount()}");
        }

        [Theory]
        [InlineData(
            "My Dataset Name",
            new string[] { },
            "mydatasetname"
        )]
        [InlineData(
            "$__ Another @@# Dataset ! + = Name",
            new string[] { },
            "anotherdatasetname"
        )]
        [InlineData(
            "abcdefghijklmnopqrstuvwxyz01234567890abcdefghijklmnopqrstuvwxyz01234567890",
            new string[] { },
            "abcdefghijklmnopqrstuvwxyz01234567890abcdefghijklmnopqrstuvwxyz"
        )]
        [InlineData(
            "abcdefghijklmnopqrstuvwxyz01234567890abcdefghijklmnopqrstuvwxyz01234567890",
            new string[] { "a" },
            "abcdefghijklmnopqrstuvwxyz01234567890abcdefghijklmnopqrstuvwx-a"
        )]
        [InlineData(
            "abcdefghijklmnopqrstuvwxyz01234567890abcdefghijklmnopqrstuvwxyz01234567890",
            new string[] { "u20191216", "x" },
            "abcdefghijklmnopqrstuvwxyz01234567890abcdefghijklmn-u20191216-x"
        )]
        public void ShouldNormalizeContainerNames(string originalName, string[] suffixes, string expectedName)
        {
            var sasTokens = Services.GetService<SasTokenService>();
            var actualName = sasTokens.ContainerNameFromDatasetName(originalName, suffixes);
            Assert.Equal(expectedName, actualName);
        }

        [Fact]
        public async Task ShouldFindContainerNameWhenContainerDoesNotExist()
        {
            var sasTokens = Services.GetService<SasTokenService>();
            var datasetStorage = GetDatasetStorage();
            var expectedName = datasetStorage.ContainerName;
            datasetStorage.ContainerName = null;

            await sasTokens.FindUniqueDatasetContainerName(datasetStorage);
            Assert.Equal(expectedName, datasetStorage.ContainerName);
        }

        [Fact]
        public async Task ShouldFindContainerNameWhenContainerAlreadyExists()
        {
            var sasTokens = Services.GetService<SasTokenService>();
            var datasetStorage = GetDatasetStorage();
            var originalName = datasetStorage.ContainerName;
            try
            {
                await sasTokens.CreateDatasetContainer(datasetStorage);
                datasetStorage.ContainerName = null;

                await sasTokens.FindUniqueDatasetContainerName(datasetStorage);
                Assert.Equal(originalName + "-2", datasetStorage.ContainerName);
            }
            finally
            {
                bool result = await sasTokens.DeleteDatasetContainer(new DatasetStorage
                {
                    Id = datasetStorage.Id,
                    AccountName = datasetStorage.AccountName,
                    DatasetName = datasetStorage.DatasetName,
                    ContainerName = originalName
                });
                Assert.True(result);
            }
        }

        [Fact]
        public async Task ShouldFindUpdateContainerNameWhenContainerDoesNotExist()
        {
            var sasTokens = Services.GetService<SasTokenService>();
            var datasetStorage = GetDatasetStorage();
            var expectedName = datasetStorage.ContainerName.Substring(0, datasetStorage.ContainerName.Length - 1) +
                $"-u{DateTime.UtcNow.ToString("yyyyMMdd")}";
            datasetStorage.ContainerName = null;

            await sasTokens.FindUniqueDatasetUpdateContainerName(datasetStorage);
            Assert.Equal(expectedName, datasetStorage.ContainerName);
        }

        [Fact]
        public async Task ShouldFindUpdateContainerNameWhenContainerAlreadyExists()
        {
            var sasTokens = Services.GetService<SasTokenService>();
            var datasetStorage = GetDatasetStorage();
            var originalName =
                datasetStorage.ContainerName.Substring(0, datasetStorage.ContainerName.Length - 1) +
                $"-u{DateTime.UtcNow.ToString("yyyyMMdd")}";
            var expectedName =
                datasetStorage.ContainerName.Substring(0, datasetStorage.ContainerName.Length - 3) +
                $"-u{DateTime.UtcNow.ToString("yyyyMMdd")}-2";
            datasetStorage.ContainerName = originalName;
            try
            {
                await sasTokens.CreateDatasetContainer(datasetStorage);
                datasetStorage.ContainerName = null;

                await sasTokens.FindUniqueDatasetUpdateContainerName(datasetStorage);
                Assert.Equal(expectedName, datasetStorage.ContainerName);
            }
            finally
            {
                bool result = await sasTokens.DeleteDatasetContainer(new DatasetStorage
                {
                    Id = datasetStorage.Id,
                    AccountName = datasetStorage.AccountName,
                    DatasetName = datasetStorage.DatasetName,
                    ContainerName = originalName
                });
                Assert.True(result);
            }
        }

        [Fact]
        public async Task ShouldGenerateSasTokenForUpdatingDatasetContents()
        {
            var sasTokens = Services.GetService<SasTokenService>();
            var datasetStorage = GetDatasetStorage();
            var blobClient = Services.GetBlobClient();
            try
            {
                await sasTokens.CreateDatasetContainer(datasetStorage);

                var sasToken = await sasTokens.GenerateSasTokenForUpdatingDatasetContainer(
                    datasetStorage.AccountName,
                    datasetStorage.ContainerName);

                //Output.WriteLine("Sas Token:");
                //Output.WriteLine(sasToken.ToString());

                // Might take up to 30 seconds for policy to be applied
                await TestUtils.Retry(async () =>
                {
                    var testContainer = new CloudBlobContainer(sasToken);
                    var list = await testContainer.ListBlobsSegmentedAsync(null);
                    var count = list.Results.Count();
                    return count == 0;
                });
            }
            finally
            {
                bool result = await sasTokens.DeleteDatasetContainer(datasetStorage);
                Assert.True(result);
            }
        }

        [Fact]
        public async Task ShouldDisableSasTokenForUpdatingDatasetContents()
        {
            var sasTokens = Services.GetService<SasTokenService>();
            var datasetStorage = GetDatasetStorage();
            var blobClient = Services.GetBlobClient();
            try
            {
                await sasTokens.CreateDatasetContainer(datasetStorage);

                var sasToken = await sasTokens.GenerateSasTokenForUpdatingDatasetContainer(
                    datasetStorage.AccountName,
                    datasetStorage.ContainerName);

                //Output.WriteLine("Sas Token:");
                //Output.WriteLine(sasToken.ToString());

                // Might take up to 30 seconds for policy to be applied
                await TestUtils.Retry(async () =>
                {
                    var testContainer = new CloudBlobContainer(sasToken);
                    var list = await testContainer.ListBlobsSegmentedAsync(null);
                    var count = list.Results.Count();
                    return count == 0;
                });

                await sasTokens.DisableSasTokenForUpdatingDatasetContainer(
                    datasetStorage.AccountName,
                    datasetStorage.ContainerName);

                await Assert.ThrowsAsync<StorageException>(async () =>
                {
                    var testContainer = new CloudBlobContainer(sasToken);
                    var list = await testContainer.ListBlobsSegmentedAsync(null);
                    var count = list.Results.Count();
                    Assert.Equal(0, count);
                });
            }
            finally
            {
                bool result = await sasTokens.DeleteDatasetContainer(datasetStorage);
                Assert.True(result);
            }
        }

        private DatasetStorage GetDatasetStorage()
        {
            var sasTokens = Services.GetService<SasTokenService>();
            var id = Guid.NewGuid();
            var name = $"Automated Test Container {id.ToString()}";
            return new DatasetStorage
            {
                Id = id,
                DatasetName = name,
                AccountName = sasTokens.DefaultDatasetStorageAccount(),
                ContainerName = sasTokens.ContainerNameFromDatasetName(name)
            };
        }
    }
}
