// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Msr.Odr.Model;
using Msr.Odr.Model.UserData;
using Xunit;

namespace Msr.Odr.Services.Test.Validation
{
    public class DatasetNominationUpdateValidationTests
    {
        [Theory]
        [InlineData("Name")]
        [InlineData("Description")]
        [InlineData("Project Page URL")]
        [InlineData("Dataset URL")]
        [InlineData("Contact Name")]
        [InlineData("Contact Info")]
        public void IsDatasetNominationValidForUpdate_RequiredFieldMissing_ExpectError(string propertyName)
        {
            IsMessageFoundForUpdate($"{propertyName} is required.").Should().BeTrue();
        }

        [Theory]
        [InlineData("Name", Constraints.LongName)]
        [InlineData("Description", Constraints.MedDescription)]
        public void IsDatasetNominationValidForUpdate_RequiredFieldMaxLengthExceeded_ExpectError(string propertyName, int fieldLength)
        {
            var nomination = ValidationTestsHelpers.GenerateNominationWithAllMaxLengthFieldsExceeded();
            IsMessageFoundForUpdate($"{propertyName} must be between 1 and {fieldLength} characters.", nomination).Should().BeTrue();
        }

        [Theory]
        [InlineData("Project Page URL", Constraints.UrlLength)]
        [InlineData("Dataset URL", Constraints.UrlLength)]
        [InlineData("Version", Constraints.VersionLength)]
        [InlineData("Contact Name", Constraints.ContactNameLength)]
        [InlineData("Contact Info", Constraints.ContactInfoLength)]
        public void IsDatasetNominationValidForUpdate_NonRequiredFieldMaxLengthExceeded_ExpectError(string propertyName, int fieldLength)
        {
            var nomination = ValidationTestsHelpers.GenerateNominationWithAllMaxLengthFieldsExceeded();
            IsMessageFoundForUpdate($"{propertyName} must be maximum of {fieldLength} characters.", nomination).Should().BeTrue();
        }

        [Fact]
        public void IsDatasetNominationValidForUpdate_MaximumTagsExceeded_ExpectError()
        {
            var nomination = new DatasetNomination
            {
                Tags = new List<string>(Enumerable.Repeat("*", 11))
            };
            IsMessageFoundForUpdate($"Maximum of {Constraints.MaxTags} tags allowed.", nomination).Should().BeTrue();
        }

        [Fact]
        public void IsDatasetNominationValidForUpdate_InvalidDatasetUrl_ExpectError()
        {
            var nomination = new DatasetNomination
            {
                DatasetUrl = "sdfsfsdfsdfsd"
            };
            IsMessageFoundForUpdate("Not a valid URL.", nomination).Should().BeTrue();
        }

        [Fact]
        public void IsDatasetNominationValidForUpdate_InvalidProjectUrl_ExpectError()
        {
            var nomination = new DatasetNomination
            {
                DatasetUrl = "sdfsfsdfsdfsd"
            };
            IsMessageFoundForUpdate("Not a valid URL.", nomination).Should().BeTrue();
        }

        [Theory]
        [InlineData(NominationLicenseType.HtmlText, true)]
        [InlineData(NominationLicenseType.InputFile, true)]
        [InlineData(NominationLicenseType.Standard, false)]
        [InlineData(NominationLicenseType.Unknown, false)]
        public void IsDatasetNominationValidForUpdate_MissingOtherLicenseName(NominationLicenseType licenseType, bool expectError)
        {
            var nomination = new DatasetNomination
            {
                NominationLicenseType = licenseType,
                OtherLicenseName = String.Empty
            };
            IsMessageFoundForUpdate("License Name is required.", nomination).Should().Be(expectError);
        }

        [Theory]
        [InlineData(NominationLicenseType.HtmlText, true)]
        [InlineData(NominationLicenseType.InputFile, true)]
        [InlineData(NominationLicenseType.Standard, false)]
        [InlineData(NominationLicenseType.Unknown, false)]
        public void IsDatasetNominationValidForUpdate_OtherLicenseNameTooLong(NominationLicenseType licenseType, bool expectError)
        {
            var nomination = new DatasetNomination
            {
                NominationLicenseType = licenseType,
                OtherLicenseName = new string('*', Constraints.MaxLicenseNameLength + 1)
            };
            IsMessageFoundForUpdate($"License Name must be maximum of {Constraints.MaxLicenseNameLength} characters.", nomination).Should().Be(expectError);
        }

        [Theory]
        [InlineData(NominationLicenseType.HtmlText, true)]
        [InlineData(NominationLicenseType.InputFile, true)]
        [InlineData(NominationLicenseType.Standard, false)]
        [InlineData(NominationLicenseType.Unknown, false)]
        public void IsDatasetNominationValidForUpdate_OtherLicenseAdditionalUrlTooLong(NominationLicenseType licenseType, bool expectError)
        {
            var nomination = new DatasetNomination
            {
                NominationLicenseType = licenseType,
                OtherLicenseAdditionalInfoUrl = new string('*', Constraints.UrlLength + 1)
            };
            IsMessageFoundForUpdate($"License Additional Info URL must be maximum of {Constraints.UrlLength} characters.", nomination).Should().Be(expectError);
        }

        [Theory]
        [InlineData(NominationLicenseType.HtmlText, false)]
        [InlineData(NominationLicenseType.InputFile, true)]
        [InlineData(NominationLicenseType.Standard, false)]
        [InlineData(NominationLicenseType.Unknown, false)]
        public void IsDatasetNominationValidForUpdate_OtherLicenseFileNameRequired(NominationLicenseType licenseType, bool expectError)
        {
            var nomination = new DatasetNomination
            {
                NominationLicenseType = licenseType,
                OtherLicenseFileName = string.Empty
            };
            IsMessageFoundForUpdate("License File Name is required.", nomination).Should().Be(expectError);
        }

        [Theory]
        [InlineData(NominationLicenseType.HtmlText, false)]
        [InlineData(NominationLicenseType.InputFile, true)]
        [InlineData(NominationLicenseType.Standard, false)]
        [InlineData(NominationLicenseType.Unknown, false)]
        public void IsDatasetNominationValidForUpdate_OtherLicenseFileNameTooLong(NominationLicenseType licenseType, bool expectError)
        {
            var nomination = new DatasetNomination
            {
                NominationLicenseType = licenseType,
                OtherLicenseFileName = new string('*', Constraints.MaxFileNameLength + 1)
            };
            IsMessageFoundForUpdate($"License File Name must be maximum of {Constraints.MaxFileNameLength} characters.", nomination).Should().Be(expectError);
        }

        [Theory]
        [InlineData(NominationLicenseType.HtmlText, true)]
        [InlineData(NominationLicenseType.InputFile, false)]
        [InlineData(NominationLicenseType.Standard, false)]
        [InlineData(NominationLicenseType.Unknown, false)]
        public void IsDatasetNominationValidForUpdate_OtherLicenseContentRequired(NominationLicenseType licenseType, bool expectError)
        {
            var nomination = new DatasetNomination
            {
                NominationLicenseType = licenseType,
                OtherLicenseContentHtml = string.Empty
            };
            IsMessageFoundForUpdate("License Content is required.", nomination).Should().Be(expectError);
        }

        [Theory]
        [InlineData(NominationLicenseType.HtmlText, true)]
        [InlineData(NominationLicenseType.InputFile, false)]
        [InlineData(NominationLicenseType.Standard, false)]
        [InlineData(NominationLicenseType.Unknown, false)]
        public void IsDatasetNominationValidForUpdate_OtherLicenseContentTooLong(NominationLicenseType licenseType, bool expectError)
        {
            var nomination = new DatasetNomination
            {
                NominationLicenseType = licenseType,
                OtherLicenseContentHtml = new string('*', Constraints.OtherLicenseContentHtml + 1)
            };
            IsMessageFoundForUpdate($"License Content must be maximum of {Constraints.OtherLicenseContentHtml} characters.", nomination).Should().Be(expectError);
        }

        private bool IsMessageFoundForUpdate(string expectedMessage, DatasetNomination nomination = null)
        {
            var service = new ValidationService();  
            nomination = nomination ?? new DatasetNomination();
            var result = service.IsDatasetNominationValidForUpdate(nomination);
            return result.Errors.Count(e => e.ErrorMessage.Equals(expectedMessage)).Equals(1);
        }
    }
}
