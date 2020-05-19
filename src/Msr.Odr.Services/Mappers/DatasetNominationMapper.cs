using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Boxed.Mapping;
using Msr.Odr.Model;
using Msr.Odr.Model.Datasets;
using Msr.Odr.Model.UserData;
using Microsoft.AspNetCore.Http;
using System.IO;
using Msr.Odr.Model.Configuration;

namespace Msr.Odr.Services.Mappers
{
    public class DatasetNominationMapper : IMapper<DatasetNomination, DatasetNominationStorageItem>
    {
        public void Map(DatasetNomination source, DatasetNominationStorageItem destination)
        {
            destination.Id = source.Id == default ? Guid.NewGuid() : source.Id;
            destination.DatasetId = WellKnownIds.DatasetNominationDatasetId;
            destination.Name = source.Name?.Trim();
            destination.Description = source.Description?.Trim();
            destination.Domain = source.Domain?.Trim();
            destination.DomainId = source.DomainId?.Trim();
            destination.Version = source.Version?.Trim();
            destination.SourceUri = source.DatasetUrl?.Trim();
            destination.ProjectUri = source.ProjectUrl?.Trim();
            destination.Published = source.Published;
            destination.License = source.LicenseName?.Trim();
            destination.LicenseId = source.LicenseId;
            destination.Tags = ProcessTags(source.Tags).ToList();
            destination.ContactName = source.ContactName?.Trim();
            destination.ContactInfo = source.ContactInfo?.Trim();
            destination.IsDownloadAllowed = source.IsDownloadAllowed;
            destination.NominationStatus = source.NominationStatus;
            destination.DigitalObjectIdentifier = source.DigitalObjectIdentifier?.Trim();

            destination.NominationLicenseType = source.NominationLicenseType;

            if (source.NominationLicenseType == NominationLicenseType.HtmlText || source.NominationLicenseType == NominationLicenseType.InputFile)
            {
                destination.License = null;
                destination.OtherLicenseAdditionalInfoUrl = source.OtherLicenseAdditionalInfoUrl?.Trim();
                destination.OtherLicenseName = source.OtherLicenseName?.Trim();

                if (source.NominationLicenseType == NominationLicenseType.InputFile)
                {
                    destination.OtherLicenseFileName = source.OtherLicenseFileName;

                    if (source.OtherLicenseFile != null)
                    {
                        destination.OtherLicenseFileContentType = source.OtherLicenseFile.ContentType;
                        destination.OtherLicenseFileContent = EncodeLicenseFileContent(source.OtherLicenseFile);
                        destination.OtherLicenseFileName = source.OtherLicenseFile.FileName;
                    }
                    else if (!string.IsNullOrEmpty(source.OtherLicenseFileContent))
                    {
                        destination.OtherLicenseFileContent = source.OtherLicenseFileContent;
                    }

                    destination.OtherLicenseContentHtml = null;
                }
                else
                {
                    destination.OtherLicenseFileContentType = null;
                    destination.OtherLicenseFileContent = null;
                    destination.OtherLicenseFileName = null;
                    destination.OtherLicenseContentHtml = EncodeLicenseContentHtml(source.OtherLicenseContentHtml);
                }
            }
            else
            {
                destination.OtherLicenseAdditionalInfoUrl = null;
                destination.OtherLicenseContentHtml = null;
                destination.OtherLicenseFileContentType = null;
                destination.OtherLicenseFileContent = null;
                destination.OtherLicenseFileName = null;
                destination.OtherLicenseName = null;

                destination.NominationLicenseType = (source.LicenseId.GetValueOrDefault() == default)
                    ? NominationLicenseType.Unknown
                    : NominationLicenseType.Standard;
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
