// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using Boxed.Mapping;
using Msr.Odr.Model;
using Msr.Odr.Model.Configuration;
using Msr.Odr.Model.Datasets;
using Msr.Odr.Model.UserData;

namespace Msr.Odr.Services.Mappers
{
    public class DatasetStorageItemMapper :
        IMapper<DatasetStorageItem, Dataset>,
        IMapper<DatasetStorageItem, DatasetEditStorageItem>
    {
        public void Map(DatasetStorageItem source, Dataset destination)
        {
            destination.Id = source.Id;
            destination.Name = source.Name;
            destination.Description = source.Description;
            destination.FileTypes = source.FileTypes;
            destination.Tags = source.Tags;
            destination.Created = source.Created;
            destination.FileCount = source.FileCount;
            destination.Modified = source.Modified;
            destination.Domain = source.Domain;
            destination.DomainId = source.DomainId;
            destination.DatasetUrl = source.SourceUri;
            destination.ProjectUrl = source.ProjectUri;
            destination.LicenseId = source.LicenseId;
            destination.LicenseName = source.License;
            destination.Version = source.Version;
            destination.Published = source.Published;
            destination.Size = source.Size;
            destination.GzipFileSize = source.GzipFileSize;
            destination.ZipFileSize = source.ZipFileSize;
            destination.CreatedByUserName = source.CreatedByUserName;
            destination.CreatedByUserEmail = source.CreatedByUserEmail;
            destination.ModifiedByUserName = source.ModifiedByUserName;
            destination.ModifiedByUserEmail = source.ModifiedByUserEmail;
            destination.IsDownloadAllowed = source.IsDownloadAllowed;
            destination.IsCompressedAvailable = source.IsCompressedAvailable;
            destination.IsFeatured = source.IsFeatured;
            destination.DigitalObjectIdentifier = source.DigitalObjectIdentifier;
            destination.DatasetOwners = source.DatasetOwners;
        }

        public void Map(DatasetStorageItem source, DatasetEditStorageItem destination)
        {
            destination.Id = source.Id;
            destination.DatasetId = WellKnownIds.DatasetEditDatasetId;
            destination.Name = source.Name;
            destination.Description = source.Description;
            destination.Domain = source.Domain;
            destination.DomainId = source.DomainId;
            destination.SourceUri = source.SourceUri;
            destination.ProjectUri = source.ProjectUri;
            destination.Version = source.Version;
            destination.Published = source.Published;
            destination.Created = source.Created;
            destination.Modified = source.Modified;
            destination.License = source.License;
            destination.LicenseId = source.LicenseId;
            destination.Tags = source.Tags ?? new List<string>();
            destination.CreatedByUserName = source.CreatedByUserName;
            destination.CreatedByUserEmail = source.CreatedByUserEmail;
            destination.ModifiedByUserName = source.ModifiedByUserName;
            destination.ModifiedByUserEmail = source.ModifiedByUserEmail;
            destination.IsDownloadAllowed = source.IsDownloadAllowed.GetValueOrDefault();
            destination.DigitalObjectIdentifier = source.DigitalObjectIdentifier;
            destination.EditStatus = DatasetEditStatus.Unmodified;
            destination.NominationStatus = NominationStatus.PendingApproval;
            destination.NominationLicenseType = NominationLicenseType.Standard;
            destination.OtherLicenseAdditionalInfoUrl = null;
            destination.OtherLicenseContentHtml = null;
            destination.OtherLicenseFileContent = null;
            destination.OtherLicenseFileContentType = null;
            destination.OtherLicenseName = null;
            destination.OtherLicenseFileName = null;
            if(source.DatasetOwners != null & source.DatasetOwners.Count > 0)
            {
                destination.ContactName = string.Join(";", source.DatasetOwners.Select(d => d.Name));
                destination.ContactInfo = string.Join(";", source.DatasetOwners.Select(d => d.Email));
            }
        }
    }
}
