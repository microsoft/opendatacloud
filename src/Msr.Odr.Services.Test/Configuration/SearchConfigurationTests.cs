// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

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
    public class SearchConfigurationTests
    {
        [Fact]
        public void ProvidesSearchAccount()
        {
            var config = BuildSearchConfiguration();
            config.Account.Should().Be("search-acct");
        }

        [Fact]
        public void ProvidesSearchKey()
        {
            var config = BuildSearchConfiguration();
            config.Key.Should().Be("search-key");
        }

        [Fact]
        public void ProvidesSearchDatasetIndex()
        {
            var config = BuildSearchConfiguration();
            config.DatasetIndex.Should().Be("search-datasets");
        }

        [Fact]
        public void ProvidesSearchFileIndex()
        {
            var config = BuildSearchConfiguration();
            config.FileIndex.Should().Be("search-files");
        }

        [Fact]
        public void ProvidesSearchNominationsIndex()
        {
            var config = BuildSearchConfiguration();
            config.NominationsIndex.Should().Be("nomination-ix");
        }

        [Fact]
        public void PassesValidation()
        {
            var config = BuildSearchConfiguration();
            SearchConfiguration.Validate(config);
        }

        private SearchConfiguration BuildSearchConfiguration()
        {
            var config = BuildConfig();
            var batch = new SearchConfiguration();
            config.GetSection("search").Bind(batch);
            return batch;
        }

        private IConfiguration BuildConfig()
        {
            var initialData = new Dictionary<string, string>
            {
                {"Search:Account", "search-acct"},
                {"Search:Key", "search-key"},
                {"Search:DatasetIndex", "search-datasets"},
                {"Search:FileIndex", "search-files"},
            };

            var builder = new ConfigurationBuilder();
            builder.AddInMemoryCollection(initialData);
            return builder.Build();
        }
    }
}
