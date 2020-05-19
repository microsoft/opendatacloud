using FluentValidation;

namespace Msr.Odr.Services.Configuration
{
    public class CosmosConfigurationValidator : AbstractValidator<CosmosConfiguration>
    {
        public CosmosConfigurationValidator()
        {
            RuleFor(options => options.Account).NotEmpty();
            RuleFor(options => options.Key).NotEmpty();
            RuleFor(options => options.Database).NotEmpty();
            RuleFor(options => options.DatasetCollection).NotEmpty();
            RuleFor(options => options.UserDataCollection).NotEmpty();
        }
    }
}
