using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Msr.Odr.Model;
using Msr.Odr.Model.Datasets;
using Msr.Odr.Model.UserData;
using Msr.Odr.Services;

namespace Msr.Odr.Batch.ImportDatasetApp
{
    public partial class ImportTask
    {
        private async Task DeleteDatasetDocuments(CancellationToken cancellationToken)
        {
            Log.Add("Deleting current dataset documents (if any).");

            var count = await FileSearch.DeleteAllFilesDocumentsByDatasetId(DatasetId, cancellationToken);
            Log.Add($"Deleted {count} Azure Search dataset document(s).");
            count = await DatasetStorage.DeleteDatasetDocuments(DatasetId, cancellationToken);
            Log.Add($"Deleted {count} dataset document(s).");
        }

        private async Task<Domain> FindDomain(string domainId, CancellationToken cancellationToken)
        {
            var domains = await DatasetStorage.GetAllDomains(cancellationToken);
            var domain = domains
                .FirstOrDefault(d => string.Compare(d.Id, domainId, StringComparison.InvariantCultureIgnoreCase) == 0);
            if (domain == null)
            {
                throw new InvalidOperationException($"Domain id, \"{domainId}\", was not found.");
            }

            return domain;
        }

        private async Task CreateDatasetDocument(
            DatasetNomination nomination,
            DatasetImportProperties storage,
            (ICollection<string> fileTypes, int fileCount, long fileSize, string containerUri) fileDetails,
            CancellationToken cancellationToken)
        {
            Log.Add("Adding dataset document.");

            var eligibleDatasetOwners = await DatasetStorage.GetEligibleDatasetOwners(cancellationToken);

            var domain = await FindDomain(nomination.DomainId, cancellationToken);

            Log.Add($"nomination.NominationLicenseType {nomination.NominationLicenseType.ToString()}");

            if (nomination.NominationLicenseType == NominationLicenseType.HtmlText ||
                 nomination.NominationLicenseType == NominationLicenseType.InputFile)
            {
                var licenseSource = nomination.NominationLicenseType == NominationLicenseType.HtmlText
                    ? "Text"
                    : "File";

                Log.Add($"Source nomination contains new license from {licenseSource}. Creating license...");
                
                var license = await DatasetStorage.CreateLicense(nomination);

                if (license != null)
                {
                    Log.Add($"Created new license: {license.Name}.");
                    nomination.LicenseId = license.Id;
                    nomination.LicenseName = license.Name;
                }
            }

            var dataset = new Dataset
            {
                Id = DatasetId,
                Name = nomination.Name,
                Description = nomination.Description,
                DatasetUrl = nomination.DatasetUrl,
                ProjectUrl = nomination.ProjectUrl,
                FileTypes = fileDetails.fileTypes.ToList(),
                Tags = nomination.Tags.ToList(),
                FileCount = fileDetails.fileCount,
                Domain = domain.Name,
                DomainId = domain.Id,
                LicenseId = nomination.LicenseId,
                LicenseName = nomination.LicenseName,
                Version = nomination.Version,
                Published = nomination.Published,
                Size = fileDetails.fileSize,
                GzipFileSize = 0,
                ZipFileSize = 0,
                IsDownloadAllowed = nomination.IsDownloadAllowed,
                IsCompressedAvailable = false,
                Created = DateTime.UtcNow,
                CreatedByUserEmail = nomination.CreatedByUserEmail,
                CreatedByUserName = nomination.CreatedByUserName,
                DigitalObjectIdentifier = nomination.DigitalObjectIdentifier,
                DatasetOwners = GetDatasetOwners(nomination, eligibleDatasetOwners).ToList()
            };

            var containerDetails = new DatasetItemContainerDetails
            {
                DatasetId = DatasetId,
                Account = storage.AccountName,
                Container = storage.ContainerName,
                Uri = fileDetails.containerUri,
            };

            await DatasetStorage.CreateDatasetRecord(dataset, containerDetails);

            await DatasetStorage.CreateDatasetStorageDetailsRecord(containerDetails);
        }

        private IEnumerable<DatasetOwner> GetDatasetOwners(DatasetNomination nomination, ICollection<Regex> eligibleDatasetOwners)
        {
            var email = nomination.ContactInfo?.Trim();
            bool hasContactInfo =
                Utils.IsValidEmail(email) &&
                eligibleDatasetOwners.Any(regex => regex.IsMatch(email));

            if (hasContactInfo)
            {
                yield return new DatasetOwner
                {
                    Name = nomination.ContactName?.Trim(),
                    Email = email,
                };
            }
        }
    }
}
