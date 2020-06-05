// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using FluentAssertions;
using Msr.Odr.Model.UserData;
using Xunit;

namespace Msr.Odr.Services.Tests.Models
{
    public class DatasetNominationTests
    {
        [Fact]
        public void ConstructorFromStorageItem_LicenseTypeStandard_SetsLicensePropertiesCorrectly()
        {
            var storageItem = CreateNominationStorageItemForLicensePropertyTests(NominationLicenseType.Standard);
            var nomination = storageItem.ToDatasetNomination();
            nomination.NominationLicenseType.Should().BeEquivalentTo(NominationLicenseType.Standard);
            nomination.OtherLicenseAdditionalInfoUrl.Should().BeNullOrEmpty();
            nomination.OtherLicenseContentHtml.Should().BeNullOrEmpty();
            nomination.OtherLicenseFileName.Should().BeNullOrEmpty();
            nomination.OtherLicenseName.Should().BeNullOrEmpty();
            nomination.LicenseId.Should().Be(storageItem.LicenseId);
        }

        [Fact]
        public void ConstructorFromStorageItem_LicenseTypeHtmlText_SetsLicensePropertiesCorrectly()
        {
            var storageItem = CreateNominationStorageItemForLicensePropertyTests(NominationLicenseType.HtmlText);
            var nomination = storageItem.ToDatasetNomination();
            nomination.NominationLicenseType.Should().BeEquivalentTo(NominationLicenseType.HtmlText);
            nomination.OtherLicenseAdditionalInfoUrl.Should().BeEquivalentTo("OtherLicenseAdditionalInfoUrl");
            nomination.OtherLicenseContentHtml.Should().BeEquivalentTo("OtherLicenseContentHtml");
            nomination.OtherLicenseFileName.Should().BeNullOrEmpty();
            nomination.OtherLicenseName.Should().BeEquivalentTo("OtherLicenseName");
            nomination.LicenseId.Should().Be(default(Guid));
        }

        [Fact]
        public void ConstructorFromStorageItem_LicenseTypeInputFile_SetsLicensePropertiesCorrectly()
        {
            var storageItem = CreateNominationStorageItemForLicensePropertyTests(NominationLicenseType.InputFile);
            var nomination = storageItem.ToDatasetNomination();
            nomination.NominationLicenseType.Should().BeEquivalentTo(NominationLicenseType.InputFile);
            nomination.OtherLicenseAdditionalInfoUrl.Should().BeEquivalentTo("OtherLicenseAdditionalInfoUrl");
            nomination.OtherLicenseContentHtml.Should().BeNullOrEmpty();
            nomination.OtherLicenseFileName.Should().BeEquivalentTo("OtherLicenseFileName");
            nomination.OtherLicenseName.Should().BeEquivalentTo("OtherLicenseName");
            nomination.LicenseId.Should().Be(default(Guid));
        }

        private DatasetNominationStorageItem CreateNominationStorageItemForLicensePropertyTests(NominationLicenseType licenseType)
        {
            var licenseId = new Guid();
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes("OtherLicenseContentHtml");
            var contentHtml = Convert.ToBase64String(plainTextBytes);
            
            var storageItem = new DatasetNominationStorageItem()
            {
                NominationLicenseType = licenseType,
                LicenseId = licenseId,
                OtherLicenseContentHtml = contentHtml,
                OtherLicenseFileName = "OtherLicenseFileName",
                OtherLicenseAdditionalInfoUrl = "OtherLicenseAdditionalInfoUrl",
                OtherLicenseName = "OtherLicenseName"
            };
            return storageItem;
        }
    }
}
