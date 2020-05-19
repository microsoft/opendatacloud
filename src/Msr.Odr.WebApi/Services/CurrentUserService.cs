using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Msr.Odr.Model;
using Msr.Odr.Services;
using Msr.Odr.WebApi.ViewModels;

namespace Msr.Odr.WebApi.Services
{
    public class CurrentUserService
    {
        private DatasetOwnersService DatasetOwners { get; }

        public CurrentUserService(DatasetOwnersService datasetOwners)
        {
            this.DatasetOwners = datasetOwners;
        }

        public async Task<CurrentUserDetails> GetCurrentUserDetails(IPrincipal user, CancellationToken cancellationToken)
        {
            var isAuthenticated = (user?.Identity?.IsAuthenticated).GetValueOrDefault();

            var canNominateDataset = false;
            var email = user.GetUserEmail();
            if (!string.IsNullOrWhiteSpace(email))
            {
                var eligibleDatasetOwners = await DatasetOwners.GetEligibleDatasetOwners(cancellationToken);
                canNominateDataset = eligibleDatasetOwners.Any(r => r.IsMatch(email));
            }

            return new CurrentUserDetails
            {
                IsAuthenticated = isAuthenticated,
                CanNominateDataset = canNominateDataset,
            };
        }

        public bool IsCurrentUserDatasetOwner(Dataset dataset, IPrincipal user)
        {
            return DatasetOwners.IsUserDatasetOwner(dataset, user);
        }
    }
}
