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

namespace Msr.Odr.IntegrationTests
{
    [Collection(ServicesFixture.Name)]
    public class DatasetOwnersServiceTests
    {
        private ServicesFixture Services { get; }
        private ITestOutputHelper Output { get; }

        public DatasetOwnersServiceTests(ServicesFixture services, ITestOutputHelper output)
        {
            Services = services;
            Output = output;
        }

        [Fact]
        public void ShouldIndicateOwnerOfDataset()
        {
            var ownersService = Services.GetService<DatasetOwnersService>();
            var dataset = new Dataset
            {
                DatasetOwners = new List<DatasetOwner>
                {
                    new DatasetOwner
                    {
                        Name = "Some User",
                        Email = "some.user@microsoft.com"
                    }
                }
            };
            var user = CreateTestUser("some.user@microsoft.com");
            var isOwner = ownersService.IsUserDatasetOwner(dataset, user);
            Assert.True(isOwner);
        }

        [Fact]
        public void ShouldIndicateNotOwnerForNullDatasetOwners()
        {
            var ownersService = Services.GetService<DatasetOwnersService>();
            var dataset = new Dataset
            {
                DatasetOwners = null
            };
            var user = CreateTestUser("some.user@microsoft.com");
            var isOwner = ownersService.IsUserDatasetOwner(dataset, user);
            Assert.False(isOwner);
        }

        [Fact]
        public void ShouldIndicateOwnerOfDatasetStorageItem()
        {
            var ownersService = Services.GetService<DatasetOwnersService>();
            var dataset = new DatasetStorageItem
            {
                DatasetOwners = new List<DatasetOwner>
                {
                    new DatasetOwner
                    {
                        Name = "Some User",
                        Email = "some.user@microsoft.com"
                    }
                }
            };
            var user = CreateTestUser("some.user@microsoft.com");
            var isOwner = ownersService.IsUserDatasetOwner(dataset, user);
            Assert.True(isOwner);
        }

        [Fact]
        public async Task ShouldRetrieveElibleDatasetOwners()
        {
            var ownersService = Services.GetService<DatasetOwnersService>();
            var list = await ownersService.GetEligibleDatasetOwners(CancellationToken.None);
            Output.WriteLine($"Owners:\n{string.Join("\n", list.Select(r => r.ToString()))}");
            Assert.True(list.Count > 0);
        }

        private ClaimsPrincipal CreateTestUser(string emailAddr)
        {
            return new ClaimsPrincipal(
                new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Email, emailAddr)
                })
            );
        }
    }
}
