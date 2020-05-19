using FluentValidation;

namespace Msr.Odr.Services.Configuration
{
    public class BatchConfigurationValidator : AbstractValidator<BatchConfiguration>
    {
        public BatchConfigurationValidator()
        {
            RuleFor(options => options.Key).NotEmpty();
            RuleFor(options => options.Account).NotEmpty();
            RuleFor(options => options.Url).NotEmpty();
            RuleFor(options => options.StorageName).NotEmpty();
            RuleFor(options => options.StorageKey).NotEmpty();
        }
    }
}
