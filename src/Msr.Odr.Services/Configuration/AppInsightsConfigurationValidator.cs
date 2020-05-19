using FluentValidation;

namespace Msr.Odr.Services.Configuration
{
    public class AppInsightsConfigurationValidator : AbstractValidator<AppInsightsConfiguration>
    {
        public AppInsightsConfigurationValidator()
        {
            RuleFor(options => options.ApplicationId).NotEmpty();
            RuleFor(options => options.Key).NotEmpty();
        }
    }
}
