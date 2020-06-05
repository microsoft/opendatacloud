// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Boxed.Mapping;
using Microsoft.AspNetCore.Http;
using Msr.Odr.Model;
using Msr.Odr.Model.UserData;
using Msr.Odr.Services;

namespace Msr.Odr.WebApi.ViewModels.Mappers
{
    public class DatasetEditViewModelMapper : IMapper<DatasetEditViewModel, DatasetEditStorageItem>
    {
        public void Map(DatasetEditViewModel source, DatasetEditStorageItem destination)
        {
            destination.Id = source.Id;
            destination.Name = source.Name;
            destination.Description = source.Description;
            destination.Domain = source.Domain;
            destination.DomainId = source.DomainId;
            destination.SourceUri = source.DatasetUrl;
            destination.ProjectUri = source.ProjectUrl;
            destination.Version = source.Version;
            destination.Published = source.Published;
            destination.Tags = ProcessTags(source.Tags).ToList();
            destination.IsDownloadAllowed = source.IsDownloadAllowed;
            destination.DigitalObjectIdentifier = source.DigitalObjectIdentifier;

            destination.ContactName = source.ContactName;
            destination.ContactInfo = source.ContactInfo;

            destination.LicenseId = source.LicenseId;
            destination.License = source.LicenseName;

            destination.NominationLicenseType = source.LicenseType;
            switch(source.LicenseType)
            {
                case NominationLicenseType.Unknown:
                case NominationLicenseType.Standard:
                    destination.OtherLicenseName = null;
                    destination.OtherLicenseAdditionalInfoUrl = null;
                    destination.OtherLicenseContentHtml = null;
                    destination.OtherLicenseFileContent = null;
                    destination.OtherLicenseFileContentType = null;
                    destination.OtherLicenseFileName = null;
                    break;
                case NominationLicenseType.HtmlText:
                    destination.OtherLicenseName = source.OtherLicenseName;
                    destination.OtherLicenseAdditionalInfoUrl = source.OtherLicenseAdditionalInfoUrl;
                    destination.OtherLicenseContentHtml = EncodeLicenseContentHtml(source.OtherLicenseContentHtml);
                    destination.OtherLicenseFileContent = null;
                    destination.OtherLicenseFileContentType = null;
                    destination.OtherLicenseFileName = null;
                    break;
                case NominationLicenseType.InputFile:
                    destination.OtherLicenseName = source.OtherLicenseName;
                    destination.OtherLicenseAdditionalInfoUrl = source.OtherLicenseAdditionalInfoUrl;
                    destination.OtherLicenseContentHtml = null;
                    if (source.OtherLicenseFile != null)
                    {
                        destination.OtherLicenseFileContent = EncodeLicenseFileContent(source.OtherLicenseFile);
                        destination.OtherLicenseFileContentType = source.OtherLicenseFile.ContentType;
                        destination.OtherLicenseFileName = source.OtherLicenseFile.FileName;
                    }
                    break;
            }
        }

        private IEnumerable<string> ProcessTags(IEnumerable<string> sourceTags)
        {
            if (sourceTags != null)
            {
                foreach (var tag in sourceTags.ToList())
                {
                    if (!string.IsNullOrWhiteSpace(tag))
                    {
                        if (tag.Contains(","))
                        {
                            var splitTags = tag.Split(',').ToList();
                            foreach (var splitTag in splitTags)
                            {
                                yield return splitTag.Trim();
                            }
                        }
                        else
                        {
                            yield return tag.Trim();
                        }
                    }
                }
            }
        }

        private string EncodeLicenseFileContent(IFormFile licenseFile)
        {
            using (var fileStream = licenseFile.OpenReadStream())
            using (var memoryStream = new MemoryStream())
            {
                fileStream.CopyTo(memoryStream);
                var fileBytes = memoryStream.ToArray();
                return Convert.ToBase64String(fileBytes);
            }
        }

        private string EncodeLicenseContentHtml(string sourceHtml)
        {
            if (string.IsNullOrEmpty(sourceHtml))
            {
                return string.Empty;
            }

            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(sourceHtml);
            var encodedContentHtml = Convert.ToBase64String(plainTextBytes);
            return encodedContentHtml;
        }
    }
}
