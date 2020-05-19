using System.Collections.Generic;
using System.Linq;
using Boxed.Mapping;
using Msr.Odr.Model;
using Msr.Odr.Model.UserData;

namespace Msr.Odr.Services.Mappers
{
    public class DatasetNominationStorageItemMapper : IMapper<DatasetNominationStorageItem, DatasetNomination>
    {
        public void Map(DatasetNominationStorageItem source, DatasetNomination destination)
        {
            destination.DatasetId = source.DatasetId;
            destination.Id = source.Id;
            destination.Name = source.Name;
            destination.Description = source.Description;
            destination.Tags = ProcessTags(source.Tags).ToList();
            destination.Published = source.Published;
            destination.Created = source.Created;
            destination.Modified = source.Modified;
            destination.Version = source.Version;
            destination.ProjectUrl = source.ProjectUri;
            destination.DatasetUrl = source.SourceUri;
            destination.Domain = source.Domain;
            destination.DomainId = source.DomainId;
            destination.LicenseId = source.LicenseId;
            destination.LicenseName = source.License;
            destination.ContactName = source.ContactName;
            destination.ContactInfo = source.ContactInfo;
            destination.CreatedByUserId = source.CreatedByUserId;
            destination.CreatedByUserName = source.CreatedByUserName;
            destination.CreatedByUserEmail = source.CreatedByUserEmail;
            destination.ModifiedByUserName = source.ModifiedByUserName;
            destination.ModifiedByUserEmail = source.ModifiedByUserEmail;
            destination.IsDownloadAllowed = source.IsDownloadAllowed;
            destination.DigitalObjectIdentifier = source.DigitalObjectIdentifier;
            destination.NominationStatus = source.NominationStatus;

            destination.NominationLicenseType = source.NominationLicenseType;
            destination.OtherLicenseFile = null;
            if (source.NominationLicenseType == NominationLicenseType.InputFile)
            {
                destination.OtherLicenseAdditionalInfoUrl = source.OtherLicenseAdditionalInfoUrl;
                destination.OtherLicenseName = source.OtherLicenseName;
                destination.OtherLicenseFileName = source.OtherLicenseFileName;
                destination.OtherLicenseFileContentType = source.OtherLicenseFileContentType;
                destination.OtherLicenseFileContent = source.OtherLicenseFileContent;
                destination.OtherLicenseContentHtml = null;
            }
            else if (source.NominationLicenseType == NominationLicenseType.HtmlText)
            {
                destination.OtherLicenseAdditionalInfoUrl = source.OtherLicenseAdditionalInfoUrl;
                destination.OtherLicenseName = source.OtherLicenseName;
                destination.OtherLicenseFileName = null;
                destination.OtherLicenseFileContentType = null;
                destination.OtherLicenseFileContent = null;
                destination.OtherLicenseContentHtml = DecodeFromBase64(source.OtherLicenseContentHtml);
            }
            else
            {
                destination.OtherLicenseAdditionalInfoUrl = null;
                destination.OtherLicenseName = null;
                destination.OtherLicenseFileName = null;
                destination.OtherLicenseFileContentType = null;
                destination.OtherLicenseFileContent = null;
                destination.OtherLicenseContentHtml = null;
            }
        }

        private IEnumerable<string> ProcessTags(IEnumerable<string> sourceTags)
        {
            if (sourceTags != null)
            {
                foreach (var tag in sourceTags.ToList())
                {
                    if (tag.Contains(","))
                    {
                        var splitTags = tag.Split(',').ToList();
                        foreach (var splitTag in splitTags)
                        {
                            yield return splitTag;
                        }
                    }
                    else if (!string.IsNullOrWhiteSpace(tag))
                    {
                        yield return tag;
                    }
                }
            }
        }

        private string DecodeFromBase64(string encodedContent)
        {
            if (string.IsNullOrEmpty(encodedContent))
            {
                return string.Empty;
            }

            var base64EncodedBytes = System.Convert.FromBase64String(encodedContent);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }
    }
}
