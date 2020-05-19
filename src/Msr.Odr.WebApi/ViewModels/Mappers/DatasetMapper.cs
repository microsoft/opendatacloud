using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Boxed.Mapping;
using Msr.Odr.Model;
using Msr.Odr.Services;

namespace Msr.Odr.WebApi.ViewModels.Mappers
{
    public class DatasetMapper : IMapper<Dataset, DatasetViewModel>
    {
        public void Map(Dataset source, DatasetViewModel destination)
        {
            destination.Id = source.Id;
            destination.Name = source.Name;
            destination.Description = source.Description;
            destination.DatasetUrl = source.DatasetUrl;
            destination.ProjectUrl = source.ProjectUrl;
            destination.FileTypes = source.FileTypes;
            destination.Tags = source.Tags;
            destination.Created = source.Created;
            destination.Modified = source.Modified;
            destination.FileCount = source.FileCount;
            destination.Domain = source.Domain;
            destination.DomainId = source.DomainId;
            destination.LicenseId = source.LicenseId;
            destination.LicenseName = source.LicenseName;
            destination.Version = source.Version;
            destination.Published = source.Published;
            destination.Size = source.Size;
            destination.GzipFileSize = source.GzipFileSize;
            destination.ZipFileSize = source.ZipFileSize;
            destination.IsDownloadAllowed = source.IsDownloadAllowed.GetValueOrDefault();
            destination.IsCompressedAvailable = source.IsCompressedAvailable.GetValueOrDefault();
            destination.IsFeatured = source.IsFeatured.GetValueOrDefault();
            destination.DigitalObjectIdentifier = source.DigitalObjectIdentifier;
            destination.IsCurrentUserOwner = false;
        }
    }
}
