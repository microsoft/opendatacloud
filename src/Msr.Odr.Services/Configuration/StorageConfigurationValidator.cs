using FluentValidation;

namespace Msr.Odr.Services.Configuration
{
    public class StorageConfigurationValidator : AbstractValidator<StorageConfiguration>
    {
        public StorageConfigurationValidator()
        {
            RuleFor(options => options.Accounts).Must(list => list.Count > 0).WithMessage("No storage accounts are specified.");
        }
    }
}
