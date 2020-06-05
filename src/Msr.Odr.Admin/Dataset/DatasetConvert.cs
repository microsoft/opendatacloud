// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Msr.Odr.Admin.Commands.Options;
using Msr.Odr.Model.Configuration;
using Msr.Odr.Model.Datasets;
using Msr.Odr.Model.UserData;

namespace Msr.Odr.Admin.Dataset
{
    public static class DatasetConvert
    {
        public static DatasetNominationStorageItem DatasetToNomination(DatasetStorageItem dataset, ContactInfoOptions contactOptions, LicenseStorageItem license)
        {
            return new DatasetNominationStorageItem
            {
                Id = dataset.Id,
                DatasetId = WellKnownIds.DatasetNominationDatasetId,
                Name = dataset.Name,
                Description = dataset.Description,
                Domain = dataset.Domain,
                DomainId = dataset.DomainId,
                ProjectUri = dataset.ProjectUri ?? dataset.SourceUri,
                SourceUri = dataset.SourceUri ?? dataset.ProjectUri,
                Version = dataset.Version,
                Published = dataset.Published,
                Created = dataset.Created,
                Modified = dataset.Modified,
                License = license?.Name ?? dataset.License,
                LicenseId = license?.Id ?? dataset.LicenseId,
                Tags = (dataset.Tags ?? Enumerable.Empty<string>()).ToList(),
                DigitalObjectIdentifier = dataset.DigitalObjectIdentifier,
                ContactName = contactOptions.Name,
                ContactInfo = contactOptions.Email,
                UserId = Guid.Empty,
                CreatedByUserName = dataset.CreatedByUserName,
                CreatedByUserEmail = dataset.CreatedByUserEmail,
                ModifiedByUserName = dataset.ModifiedByUserName,
                ModifiedByUserEmail = dataset.ModifiedByUserEmail,
                IsDownloadAllowed = dataset?.IsDownloadAllowed ?? false,
                NominationStatus = NominationStatus.PendingApproval,
            };
        }
    }
}
