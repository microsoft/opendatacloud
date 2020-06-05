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
    public class BatchConfigurationTests
    {
        [Fact]
        public void ProvidesBatchKey()
        {
            var batchConfig = BuildBatchConfiguration();
            batchConfig.Key.Should().Be("batch-key");
        }

        [Fact]
        public void ProvidesBatchAccount()
        {
            var batchConfig = BuildBatchConfiguration();
            batchConfig.Account.Should().Be("batch-account");
        }

        [Fact]
        public void ProvidesBatchUrl()
        {
            var batchConfig = BuildBatchConfiguration();
            batchConfig.Url.Should().Be("batch-url");
        }

        [Fact]
        public void ProvidesBatchStorageName()
        {
            var batchConfig = BuildBatchConfiguration();
            batchConfig.StorageName.Should().Be("batch-storage-name");
        }

        [Fact]
        public void ProvidesBatchStorageKey()
        {
            var batchConfig = BuildBatchConfiguration();
            batchConfig.StorageKey.Should().Be("batch-storage-key");
        }

        [Fact]
        public void PassesValidation()
        {
            var batchConfig = BuildBatchConfiguration();
            BatchConfiguration.Validate(batchConfig);
        }

        private BatchConfiguration BuildBatchConfiguration()
        {
            var config = BuildConfig();
            var batch = new BatchConfiguration();
            config.GetSection("batch").Bind(batch);
            return batch;
        }

        private IConfiguration BuildConfig()
        {
            var initialData = new Dictionary<string, string>
            {
                {"Batch:Key", "batch-key"},
                {"Batch:Account", "batch-account"},
                {"Batch:Url", "batch-url"},
                {"Batch:StorageName", "batch-storage-name"},
                {"Batch:StorageKey", "batch-storage-key"},
            };

            var builder = new ConfigurationBuilder();
            builder.AddInMemoryCollection(initialData);
            return builder.Build();
        }
    }
}
