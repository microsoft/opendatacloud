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
    public class StorageConfigurationTests
    {
        [Fact]
        public void ShouldHaveZeroAccountsForEmptyConfigData()
        {
            var config = BuildConfig(EmptyConfigData);
            config.Accounts.Count.Should().Be(0);
        }

        [Fact]
        public void ShouldThrowExceptionForNoAccounts()
        {
            var config = BuildConfig(EmptyConfigData);
            Action act = () =>
            {
                StorageConfiguration.Validate(config);
            };
            act.Should().Throw<FluentValidation.ValidationException>();
        }

        [Fact]
        public void ShouldHaveFourAccountsForTestConfigData()
        {
            var config = BuildConfig(AccountsConfigData);
            config.Accounts.Count.Should().Be(4);
        }

        [Fact]
        public void ShouldContainExpectedAccounts()
        {
            var config = BuildConfig(AccountsConfigData);
            config.Accounts["msrodrdata001"].Should().Be("msrodrdata001-key");
            config.Accounts["msrodrdata002"].Should().Be("msrodrdata002-key");
            config.Accounts["msrodrdata004"].Should().Be("msrodrdata004-key");
        }

        [Fact]
        public void ShouldThrowExceptionForMissingAccountName()
        {
            var config = BuildConfig(AccountsConfigData);
            Action act = () =>
            {
                var ignore = config.Accounts["unknown"];
            };
            act.Should().Throw<StorageAccountNotFoundException>();
        }

        [Fact]
        public void ShouldThrowExceptionForMissingAccountKey()
        {
            var config = BuildConfig(AccountsConfigData);
            Action act = () =>
            {
                var ignore = config.Accounts["empty"];
            };
            act.Should().Throw<StorageAccountNotFoundException>();
        }

        [Fact]
        public void ShouldValidate()
        {
            var config = BuildConfig(AccountsConfigData);
            StorageConfiguration.Validate(config);
        }

        [Fact]
        public void ShouldHaveDefaultStorageAccount()
        {
            var config = BuildConfig(AccountsConfigData);
            config.Accounts.DefaultStorageAccount.Should().NotBeNullOrWhiteSpace();
        }

        private static readonly Dictionary<string, string> EmptyConfigData = new Dictionary<string, string>();

        private static readonly Dictionary<string, string> AccountsConfigData = new Dictionary<string, string>
        {
            {"Storage:Accounts:msrodrdata001", "msrodrdata001-key"},
            {"Storage:Accounts:msrodrdata002", "msrodrdata002-key"},
            {"Storage:Accounts:empty", ""},
            {"Storage:Accounts:msrodrdata004", "msrodrdata004-key"},
        };

        private StorageConfiguration BuildConfig(Dictionary<string, string> initialData)
        {
            var builder = new ConfigurationBuilder();
            builder.AddInMemoryCollection(initialData);
            var config = builder.Build();
            var storage = new StorageConfiguration();
            config.GetSection("storage").Bind(storage);
            return storage;
        }
    }
}
