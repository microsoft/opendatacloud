// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Boxed.Mapping;
using Msr.Odr.Model;
using Msr.Odr.Model.Datasets;
using Msr.Odr.Model.UserData;

namespace Msr.Odr.Services.Mappers
{
    public class DatasetMapper : IMapper<Dataset, DatasetStorageItem>
    {
        public void Map(Dataset source, DatasetStorageItem destination)
        {
            destination.Id = source.Id == default ? Guid.NewGuid() : source.Id;
            destination.Name = source.Name?.Trim();
            destination.Description = source.Description?.Trim();
            destination.Version = source.Version?.Trim();
            destination.Published = source.Published;
            destination.License = source.LicenseName?.Trim();
            destination.LicenseId = source.LicenseId ?? Guid.Empty;
            destination.Domain = source.Domain?.Trim();
            destination.DomainId = source.DomainId?.Trim();
            destination.SourceUri = source.DatasetUrl?.Trim();
            destination.ProjectUri = source.ProjectUrl;
            destination.Tags = source.Tags.OrEmptyList().Where(t => !string.IsNullOrWhiteSpace(t)).ToList();
            destination.FileCount = source.FileCount;
            destination.FileTypes = source.FileTypes.OrEmptyList();
            destination.Size = source.Size;
            destination.ZipFileSize = source.ZipFileSize;
            destination.GzipFileSize = source.GzipFileSize;
            destination.IsCompressedAvailable = source.IsCompressedAvailable;
            destination.IsDownloadAllowed = source.IsDownloadAllowed;
            destination.IsFeatured = source.IsFeatured;
            destination.DigitalObjectIdentifier = source.DigitalObjectIdentifier;
            destination.DatasetOwners = source.DatasetOwners.OrEmptyList().ToList();
        }
    }
}
