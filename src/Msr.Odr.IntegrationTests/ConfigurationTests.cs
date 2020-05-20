using System;
using Microsoft.Extensions.Options;
using Msr.Odr.IntegrationTests.Setup;
using Msr.Odr.Services.Configuration;
using Xunit;
using Xunit.Abstractions;

namespace Msr.Odr.IntegrationTests
{
    [Collection(ServicesFixture.Name)]
    public class ConfigurationTests
    {
        private ServicesFixture Services { get; }
        private ITestOutputHelper Output { get; }

        public ConfigurationTests(ServicesFixture services, ITestOutputHelper output)
        {
            Services = services;
            Output = output;
        }

        [Fact]
        public void ShouldLoadCosmosConfiguration()
        {
            var config = Services.GetService<IOptions<CosmosConfiguration>>();
            Assert.NotNull(config);
            Assert.True(!string.IsNullOrEmpty(config.Value.Database));
        }

        [Fact]
        public void ShouldLoadStorageConfiguration()
        {
            var config = Services.GetService<IOptions<StorageConfiguration>>();
            Assert.NotNull(config);
            Assert.True(config.Value.Accounts.Count > 0);
        }
    }
}