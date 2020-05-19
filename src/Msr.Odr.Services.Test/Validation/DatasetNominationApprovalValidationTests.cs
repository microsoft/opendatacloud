using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Msr.Odr.Model;
using Xunit;

namespace Msr.Odr.Services.Test.Validation
{
    public class DatasetNominationApprovalValidationTests
    {
        [Theory]
        [InlineData("Name")]
        [InlineData("Description")]
        [InlineData("Project Page URL")]
        [InlineData("Dataset URL")]
        [InlineData("Version")]
        [InlineData("License")]
        [InlineData("Domain")]
        [InlineData("Published Date")]
        [InlineData("Contact Name")]
        [InlineData("Contact Info")]
        public void IsDatasetNominationValidForApproval_RequiredFieldMissing_ExpectError(string propertyName)
        {
            IsMessageFoundForApproval($"{propertyName} is required.").Should().BeTrue();
        }

        [Theory]
        [InlineData("Name", Constraints.LongName)]
        [InlineData("Description", Constraints.MedDescription)]
        public void IsDatasetNominationValidForApproval_RequiredFieldMaxLengthExceeded_ExpectError(string propertyName, int fieldLength)
        {
            var nomination = ValidationTestsHelpers.GenerateNominationWithAllMaxLengthFieldsExceeded();
            IsMessageFoundForApproval($"{propertyName} must be between 1 and {fieldLength} characters.", nomination).Should().BeTrue();
        }

        [Theory]
        [InlineData("Dataset URL", Constraints.UrlLength)]
        [InlineData("Project Page URL", Constraints.UrlLength)]
        [InlineData("Version", Constraints.VersionLength)]
        [InlineData("Contact Name", Constraints.ContactNameLength)]
        [InlineData("Contact Info", Constraints.ContactInfoLength)]
        public void IsDatasetNominationValidForApproval_NonRequiredFieldMaxLengthExceeded_ExpectError(string propertyName, int fieldLength)
        {
            var nomination = ValidationTestsHelpers.GenerateNominationWithAllMaxLengthFieldsExceeded();
            IsMessageFoundForApproval($"{propertyName} must be maximum of {fieldLength} characters.", nomination).Should().BeTrue();
        }

        [Fact]
        public void IsDatasetNominationValidForApproval_MaximumTagsExceeded_ExpectError()
        {
            var nomination = new DatasetNomination
            {
                Tags = new List<string>(Enumerable.Repeat("*", 11))
            };
            IsMessageFoundForApproval($"Maximum of {Constraints.MaxTags} tags allowed.", nomination).Should().BeTrue();
        }

        [Fact]
        public void IsDatasetNominationValidForApproval_InvalidUrl_ExpectError()
        {
            var nomination = new DatasetNomination
            {
                DatasetUrl = "Not a URL"
            };
            IsMessageFoundForApproval("Not a valid URL.", nomination).Should().BeTrue();
        }

        private bool IsMessageFoundForApproval(string expectedMessage, DatasetNomination nomination = null)
        {
            var service = new ValidationService();
            nomination = nomination ?? new DatasetNomination();
            var result = service.IsDatasetNominationValidForApproval(nomination);
            return result.Errors.Count(e => e.ErrorMessage.Equals(expectedMessage)).Equals(1);
        }

    }
}
