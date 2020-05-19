using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Msr.Odr.Services.Configuration;
using Xunit;

namespace Msr.Odr.Services.Test.Configuration
{
    public class CosmosConfigurationTests
    {
        [Fact]
        public void ProvidesDocumentsAccount()
        {
            var config = BuildCosmosConfiguration();
            config.Account.Should().Be("cosmos-acct");
        }

        [Fact]
        public void ProvidesDocumentsKey()
        {
            var config = BuildCosmosConfiguration();
            config.Key.Should().Be("cosmos-key");
        }

        [Fact]
        public void ProvidesDocumentsDatabase()
        {
            var config = BuildCosmosConfiguration();
            config.Database.Should().Be("cosmos-database");
        }

        [Fact]
        public void ProvidesDocumentsDatasetsCollection()
        {
            var config = BuildCosmosConfiguration();
            config.DatasetCollection.Should().Be("cosmos-datasets");
        }

        [Fact]
        public void ProvidesDocumentsUserDataCollection()
        {
            var config = BuildCosmosConfiguration();
            config.UserDataCollection.Should().Be("cosmos-userdata");
        }

        [Fact]
        public void ProvidesDocumentsUri()
        {
            var config = BuildCosmosConfiguration();
            config.Uri.Should().Be(new Uri($"https://cosmos-acct.documents.azure.com"));
        }

        [Fact]
        public void PassesValidation()
        {
            var config = BuildCosmosConfiguration();
            CosmosConfiguration.Validate(config);
        }

        private CosmosConfiguration BuildCosmosConfiguration()
        {
            var config = BuildConfig();
            var batch = new CosmosConfiguration();
            config.GetSection("documents").Bind(batch);
            return batch;
        }

        private IConfiguration BuildConfig()
        {
            var initialData = new Dictionary<string, string>
            {
                {"Documents:Account", "cosmos-acct"},
                {"Documents:Key", "cosmos-key"},
                {"Documents:Database", "cosmos-database"},
                {"Documents:DatasetCollection", "cosmos-datasets"},
                {"Documents:UserDataCollection", "cosmos-userdata"},
            };

            var builder = new ConfigurationBuilder();
            builder.AddInMemoryCollection(initialData);
            return builder.Build();
        }
    }
}
