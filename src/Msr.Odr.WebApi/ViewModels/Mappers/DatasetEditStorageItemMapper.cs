// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Boxed.Mapping;
using Msr.Odr.Model;
using Msr.Odr.Model.UserData;
using Msr.Odr.Services;

namespace Msr.Odr.WebApi.ViewModels.Mappers
{
    public class DatasetEditStorageItemMapper : IMapper<DatasetEditStorageItem, DatasetEditViewModel>
    {
        public void Map(DatasetEditStorageItem source, DatasetEditViewModel destination)
        {
            destination.Id = source.Id;
            destination.Name = source.Name;
            destination.Description = source.Description;
            destination.Domain = source.Domain;
            destination.DomainId = source.DomainId;
            destination.DatasetUrl = source.SourceUri;
            destination.ProjectUrl = source.ProjectUri;
            destination.Version = source.Version;
            destination.Published = source.Published;
            destination.Tags = source.Tags;
            destination.IsDownloadAllowed = source.IsDownloadAllowed;
            destination.DigitalObjectIdentifier = source.DigitalObjectIdentifier;

            destination.ContactName = source.ContactName;
            destination.ContactInfo = source.ContactInfo;

            destination.LicenseId = source.LicenseId;
            destination.LicenseName = source.License;

            destination.LicenseType = source.NominationLicenseType;
            destination.OtherLicenseContentHtml = source.OtherLicenseContentHtml;
            destination.OtherLicenseFileName = source.OtherLicenseFileName;
            destination.OtherLicenseFileContent = source.OtherLicenseFileContent;
            destination.OtherLicenseFileContentType = source.OtherLicenseFileContentType;
            destination.OtherLicenseAdditionalInfoUrl = source.OtherLicenseAdditionalInfoUrl;
            destination.OtherLicenseName = source.OtherLicenseName;

            destination.EditStatus = source.EditStatus;
        }
    }
}
