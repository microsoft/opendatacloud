using System.Linq;
using FluentValidation;

namespace Msr.Odr.Model.Validators
{
    public class DatasetUpdateValidator : AbstractValidator<Dataset>
    {
        public DatasetUpdateValidator()
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

            RuleFor(f => f.DatasetUrl)
                .NotEmpty()
                .WithMessage("Dataset URL is required.")
                .WithErrorCode("required")
                .Must(url => ValidatorHelpers.BeValidUrl(url, true))
                .WithMessage("Not a valid URL.")
                .WithErrorCode("url")
                .MaximumLength(Constraints.UrlLength)
                .WithMessage($"Dataset URL must be maximum of {Constraints.UrlLength} characters.")
                .WithErrorCode("maxlength");

            RuleFor(f => f.ProjectUrl)
                .NotEmpty()
                .WithMessage("Project Page URL is required.")
                .WithErrorCode("required")
                .Must(url => ValidatorHelpers.BeValidUrl(url, true))
                .WithMessage("Not a valid URL.")
                .WithErrorCode("url")
                .MaximumLength(Constraints.UrlLength)
                .WithMessage($"Project Page URL must be maximum of {Constraints.UrlLength} characters.")
                .WithErrorCode("maxlength");

            RuleFor(f => f.FileTypes)
                .Must(types => types.Count() <= Constraints.MaxFileTypes)
                .WithMessage($"Maximum of {Constraints.MaxFileTypes} file types allowed.")
                .WithErrorCode("filetypes");

            RuleFor(f => f.Tags)
                .Must(tags => tags.Count() <= Constraints.MaxTags)
                .WithMessage($"Maximum of {Constraints.MaxTags} tags allowed.")
                .WithErrorCode("tags");

            RuleFor(n => n.Version)
                .NotEmpty()
                .WithMessage("Version is required.")
                .WithErrorCode("required")
                .MaximumLength(Constraints.VersionLength)
                .WithMessage($"Version must be maximum of {Constraints.VersionLength} characters.")
                .WithErrorCode("maxlength");

            RuleFor(n => n.Domain)
                .NotEmpty()
                .WithMessage("Domain is required.")
                .WithErrorCode("required");

            RuleFor(n => n.Published)
                .NotEmpty()
                .WithMessage("Published Date is required.")
                .WithErrorCode("required");

            RuleFor(n => n.LicenseId)
                .NotEmpty()
                .WithMessage("License is required.")
                .WithErrorCode("required")
                .OverridePropertyName("License");

            RuleFor(n => n.LicenseName)
                .NotEmpty()
                .WithMessage("License Name is required.")
                .WithErrorCode("required")
                .OverridePropertyName("License");

            RuleFor(n => n.DigitalObjectIdentifier)
                .MaximumLength(Constraints.DigitalObjectIdentifierLength)
                .WithMessage($"Digital Object Identifier must be no more than {Constraints.DigitalObjectIdentifierLength} characters.")
                .WithErrorCode("maxlength");
        }
    }
}