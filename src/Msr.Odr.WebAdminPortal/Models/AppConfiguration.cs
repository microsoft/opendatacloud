using Microsoft.Extensions.Configuration;

namespace Msr.Odr.WebAdminPortal.Models
{
    public class AppConfiguration
    {
        public AppConfiguration(IConfiguration config)
        {
            AzureAD = new AppConfigurationAzureADConfig(config);
        }
        public AppConfigurationAzureADConfig AzureAD { get; }
    }

    public class AppConfigurationAzureADConfig
    {
        public AppConfigurationAzureADConfig(IConfiguration config)
        {
            Tenant = config["AzureAD:Tenant"];
            Audience = config["AzureAD:Audience"];
            Policy = config["AzureAD:Policy"];
        }

        public string Tenant { get; }
        public string Audience { get; }
        public string Policy { get; }
    }
}
