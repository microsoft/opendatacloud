using FluentValidation;

namespace Msr.Odr.Services.Configuration
{
    public class WebServerConfiguration
    {
        /// <summary>
        /// URL of web server (for callbacks)
        /// </summary>
        public string URL { get; set; }

        /// <summary>
        /// URL used to import ARM Template into Azure account.
        /// </summary>
        public string AzureImportURL { get; set; }

        /// <summary>
        /// URL used for site map
        /// </summary>
        public string SiteMap { get; set; }

        public static void Validate(WebServerConfiguration configuration)
        {
            var validator = new WebServerConfigurationValidator();
            validator.ValidateAndThrow(configuration);
        }
    }
}
