using FluentValidation;

namespace Msr.Odr.Services.Configuration
{
    public class WebServerConfigurationValidator : AbstractValidator<WebServerConfiguration>
    {
        public WebServerConfigurationValidator()
        {
            RuleFor(options => options.URL).NotEmpty();
            RuleFor(options => options.AzureImportURL).NotEmpty();
            RuleFor(options => options.SiteMap).NotEmpty();
        }
    }
}
