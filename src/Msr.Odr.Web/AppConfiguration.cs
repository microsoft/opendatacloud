using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Msr.Odr.Web
{
    public class AppConfiguration
    {
        public AppConfiguration(IConfiguration config)
        {
            ApiBaseUrl = config["ApiBaseUrl"];
            SiteMap = config["SiteMap"];
            AzureAD = new AppConfigurationAzureADConfig(config);
        }
        public string ApiBaseUrl { get; }
        public string SiteMap { get; }
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
