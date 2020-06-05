// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Msr.Odr.Model;
using Msr.Odr.Model.Datasets;

namespace Msr.Odr.Services
{
    public class DatasetOwnersService
    {
        private const string EligibleDatasetOwnersCacheKey = "EligibleDatasetOwners";

        private IMemoryCache MemoryCache { get; }
        private DatasetStorageService Storage { get; }

        public DatasetOwnersService(IMemoryCache memoryCache, DatasetStorageService storage)
        {
            this.MemoryCache = memoryCache;
            this.Storage = storage;
        }

        public bool IsUserDatasetOwner(DatasetStorageItem dataset, IPrincipal user)
        {
            if (dataset is null)
            {
                throw new ArgumentNullException(nameof(dataset));
            }

            return IsUserInDatasetOwnersList(dataset.DatasetOwners, user);
        }

        public bool IsUserDatasetOwner(Dataset dataset, IPrincipal user)
        {
            if (dataset is null)
            {
                throw new ArgumentNullException(nameof(dataset));
            }

            return IsUserInDatasetOwnersList(dataset.DatasetOwners, user);
        }

        public async Task<ICollection<Regex>> GetEligibleDatasetOwners(CancellationToken cancellationToken)
        {
            return await MemoryCache.GetOrCreateAsync(EligibleDatasetOwnersCacheKey, async entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(15);
                return await Storage.GetEligibleDatasetOwners(cancellationToken);
            });
        }

        private bool IsUserInDatasetOwnersList(ICollection<DatasetOwner> datasetOwners, IPrincipal user)
        {
            if (datasetOwners == null || datasetOwners.Count == 0)
            {
                return false;
            }

            var ownerEmails = datasetOwners
                .Where(d => !string.IsNullOrWhiteSpace(d.Email))
                .Select(d => d.Email)
                .ToList();
            if (ownerEmails.Count == 0)
            {
                return false;
            }

            var userEmail = user.GetUserEmail();
            if (string.IsNullOrWhiteSpace(userEmail))
            {
                return false;
            }

            return ownerEmails.Any(ownerEmail => string.Equals(userEmail, ownerEmail, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
