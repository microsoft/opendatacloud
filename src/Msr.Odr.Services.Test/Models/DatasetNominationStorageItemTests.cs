using System;
using FluentAssertions;
using Msr.Odr.Model;
using Msr.Odr.Model.UserData;
using Xunit;

namespace Msr.Odr.Services.Tests.Models
{
    public class DatasetNominationStorageItemTests
    {
        [Fact]
        public void ConstructorFromNomination_LicenseTypeStandard_SetsLicensePropertiesCorrectly()
        {
            var nomination = CreateNominationForLicensePropertyTests(NominationLicenseType.Standard);
            var storageItem = nomination.ToDatasetNominationStorageItem();
            storageItem.NominationLicenseType.Should().BeEquivalentTo(NominationLicenseType.Standard);
            storageItem.OtherLicenseAdditionalInfoUrl.Should().BeNullOrEmpty();
            storageItem.OtherLicenseContentHtml.Should().BeNullOrEmpty();
            storageItem.OtherLicenseFileName.Should().BeNullOrEmpty();
            storageItem.OtherLicenseName.Should().BeNullOrEmpty();
            storageItem.LicenseId.Should().Be(nomination.LicenseId);
        }

        [Fact]
        public void ConstructorFromNomination_LicenseTypeHtmlText_SetsLicensePropertiesCorrectly()
        {
            var nomination = CreateNominationForLicensePropertyTests(NominationLicenseType.HtmlText);
            var storageItem = nomination.ToDatasetNominationStorageItem();
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes("OtherLicenseContentHtml");
            var encodedContent = Convert.ToBase64String(plainTextBytes);
            storageItem.NominationLicenseType.Should().BeEquivalentTo(NominationLicenseType.HtmlText);
            storageItem.OtherLicenseAdditionalInfoUrl.Should().BeEquivalentTo("OtherLicenseAdditionalInfoUrl");
            storageItem.OtherLicenseContentHtml.Should().BeEquivalentTo(encodedContent);
            storageItem.OtherLicenseFileName.Should().BeNullOrEmpty();
            storageItem.OtherLicenseName.Should().BeEquivalentTo("OtherLicenseName");
            storageItem.LicenseId.Should().Be(default(Guid));
        }

        [Fact]
        public void ConstructorFromNomination_LicenseTypeInputFile_SetsLicensePropertiesCorrectly()
        {
            var nomination = CreateNominationForLicensePropertyTests(NominationLicenseType.InputFile);
            var storageItem = nomination.ToDatasetNominationStorageItem();
            storageItem.NominationLicenseType.Should().BeEquivalentTo(NominationLicenseType.InputFile);
            storageItem.OtherLicenseAdditionalInfoUrl.Should().BeEquivalentTo("OtherLicenseAdditionalInfoUrl");
            storageItem.OtherLicenseContentHtml.Should().BeNullOrEmpty();
            storageItem.OtherLicenseFileName.Should().BeEquivalentTo("OtherLicenseFileName");
            storageItem.OtherLicenseName.Should().BeEquivalentTo("OtherLicenseName");
            storageItem.LicenseId.Should().Be(default(Guid));
        }

        private DatasetNomination CreateNominationForLicensePropertyTests(NominationLicenseType licenseType)
        {
            var licenseId = Guid.NewGuid();
            var isOtherLicense = licenseType == NominationLicenseType.HtmlText ||
                                 licenseType == NominationLicenseType.InputFile;

            var nomination = new DatasetNomination()
            {
                NominationLicenseType = licenseType,
                LicenseId = isOtherLicense ? default(Guid) : licenseId,
                OtherLicenseContentHtml = licenseType == NominationLicenseType.HtmlText ? "OtherLicenseContentHtml" : null,
                OtherLicenseFileName = licenseType == NominationLicenseType.InputFile ? "OtherLicenseFileName" : null,
                OtherLicenseAdditionalInfoUrl = "OtherLicenseAdditionalInfoUrl",
                OtherLicenseName = "OtherLicenseName"
            };
            return nomination;
        }
    }
}
