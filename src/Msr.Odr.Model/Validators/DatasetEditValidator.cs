// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Linq;
using FluentValidation;
using Msr.Odr.Model.UserData;

namespace Msr.Odr.Model.Validators
{
    public class DatasetEditValidator : AbstractValidator<DatasetEditStorageItem>
    {
        public DatasetEditValidator()
        {
            RuleFor(n => n.Name)
                .NotEmpty()
                .WithMessage("Name is required.")
                .WithErrorCode("required")
                .MaximumLength(Constraints.LongName)
                .WithMessage($"Name must be between 1 and {Constraints.LongName} characters.")
                .WithErrorCode("maxlength"); 

            RuleFor(n => n.Description)
                .NotEmpty()
                .WithMessage("Description is required.")
                .WithErrorCode("required")
                .MaximumLength(Constraints.MedDescription)
                .WithMessage($"Description must be between 1 and {Constraints.MedDescription} characters.")
                .WithErrorCode("maxlength");

            RuleFor(f => f.SourceUri)
                .NotEmpty()
                .WithMessage("Dataset URL is required.")
                .WithErrorCode("required")
                .Must(url => ValidatorHelpers.BeValidUrl(url, true))
                .WithMessage("Not a valid URL.")
                .WithErrorCode("url")
                .MaximumLength(Constraints.UrlLength)
                .WithMessage($"Dataset URL must be maximum of {Constraints.UrlLength} characters.")
                .WithErrorCode("maxlength");

            RuleFor(f => f.ProjectUri)
                .NotEmpty()
                .WithMessage("Project Page URL is required.")
                .WithErrorCode("required")
                .Must(url => ValidatorHelpers.BeValidUrl(url, true))
                .WithMessage("Not a valid URL.")
                .WithErrorCode("url")
                .MaximumLength(Constraints.UrlLength)
                .WithMessage($"Project Page URL must be maximum of {Constraints.UrlLength} characters.")
                .WithErrorCode("maxlength");

            RuleFor(f => f.Tags)
                .Must(tags => tags.Count() <= Constraints.MaxTags)
                .WithMessage($"Maximum of {Constraints.MaxTags} tags allowed.")
                .WithErrorCode("tags");

            RuleFor(n => n.Version)
                .MaximumLength(Constraints.VersionLength)
                .WithMessage($"Version must be maximum of {Constraints.VersionLength} characters.")
                .WithErrorCode("maxlength");

            RuleFor(n => n.ContactName)
                .NotEmpty()
                .WithMessage("Contact Name is required.")
                .WithErrorCode("required")
                .MaximumLength(Constraints.ContactNameLength)
                .WithMessage($"Contact Name must be maximum of {Constraints.ContactNameLength} characters.")
                .WithErrorCode("maxlength");

            RuleFor(n => n.ContactInfo)
                .NotEmpty()
                .WithMessage("Contact Info is required.")
                .WithErrorCode("required")
                .MaximumLength(Constraints.ContactInfoLength)
                .WithMessage($"Contact Info must be maximum of {Constraints.ContactInfoLength} characters.")
                .WithErrorCode("maxlength");

            RuleFor(n => n.DigitalObjectIdentifier)
                .MaximumLength(Constraints.DigitalObjectIdentifierLength)
                .WithMessage($"Digital Object Identifier must be no more than {Constraints.DigitalObjectIdentifierLength} characters.")
                .WithErrorCode("maxlength");

            RunRulesForNonStandardLicense();

            RunRulesForLicenseTypeHtmlContent();

            RunRulesForLicenseTypeInputFile();
        }

        private void RunRulesForLicenseTypeInputFile()
        {
            RuleFor(n => n.OtherLicenseFileName)
                .NotEmpty()
                .WithMessage("License File Name is required.")
                .WithErrorCode("required")
                .MaximumLength(Constraints.MaxFileNameLength)
                .WithMessage($"License File Name must be maximum of {Constraints.MaxFileNameLength} characters.")
                .WithErrorCode("maxlength")
                .When(n => n.NominationLicenseType == NominationLicenseType.InputFile)
                .OverridePropertyName("LicenseFileName");
        }

        private void RunRulesForLicenseTypeHtmlContent()
        {
            RuleFor(n => n.OtherLicenseContentHtml)
                .NotEmpty()
                .WithMessage("License Content is required.")
                .WithErrorCode("required")
                .MaximumLength(Constraints.OtherLicenseContentHtml)
                .WithMessage($"License Content must be maximum of {Constraints.OtherLicenseContentHtml} characters.")
                .WithErrorCode("maxlength")
                .When(n => n.NominationLicenseType == NominationLicenseType.HtmlText)
                .OverridePropertyName("LicenseContent");
        }

        private void RunRulesForNonStandardLicense()
        {
            RuleFor(n => n.OtherLicenseAdditionalInfoUrl)
                .Must(url => ValidatorHelpers.BeValidUrl(url, true))
                .When(n => !string.IsNullOrEmpty(n.OtherLicenseAdditionalInfoUrl) && 
                            (n.NominationLicenseType == NominationLicenseType.HtmlText ||
                             n.NominationLicenseType == NominationLicenseType.InputFile))
                .WithMessage("Not a valid URL.")
                .WithErrorCode("url")
                .MaximumLength(Constraints.UrlLength)
                .WithMessage($"License Additional Info URL must be maximum of {Constraints.UrlLength} characters.")
                .WithErrorCode("maxlength")
                .When(n => !string.IsNullOrEmpty(n.OtherLicenseAdditionalInfoUrl) && 
                            (n.NominationLicenseType == NominationLicenseType.HtmlText ||
                             n.NominationLicenseType == NominationLicenseType.InputFile))
                .OverridePropertyName("LicenseUrl");

            RuleFor(n => n.OtherLicenseName)
                .NotEmpty()
                .WithMessage("License Name is required.")
                .WithErrorCode("required")
                .MaximumLength(Constraints.MaxLicenseNameLength)
                .WithMessage($"License Name must be maximum of {Constraints.MaxLicenseNameLength} characters.")
                .WithErrorCode("maxlength")
                .When(n => n.NominationLicenseType == NominationLicenseType.HtmlText ||
                           n.NominationLicenseType == NominationLicenseType.InputFile)
                .OverridePropertyName("LicenseName");
        }
    }
}
