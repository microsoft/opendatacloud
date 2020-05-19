using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Msr.Odr.Services.Configuration;
using Xunit;

namespace Msr.Odr.Services.Test.Configuration
{
    public class WebServerConfigurationTests
    {
        [Fact]
        public void ProvidesWebServerUrl()
        {
            var config = BuildWebServerConfiguration();
            config.URL.Should().Be("web-server-url");
        }

        [Fact]
        public void ProvidesAzureImportUrl()
        {
            var config = BuildWebServerConfiguration();
            config.AzureImportURL.Should().Be("azure-import");
        }

        [Fact]
        public void PassesValidation()
        {
            var config = BuildWebServerConfiguration();
            WebServerConfiguration.Validate(config);
        }

        private WebServerConfiguration BuildWebServerConfiguration()
        {
            var config = BuildConfig();
            var batch = new WebServerConfiguration();
            config.GetSection("webserver").Bind(batch);
            return batch;
        }

        private IConfiguration BuildConfig()
        {
            var initialData = new Dictionary<string, string>
            {
                {"WebServer:URL", "web-server-url"},
                {"WebServer:AzureImportURL", "azure-import"},
                {"WebServer:SiteMap", "https://test-site.com"},
            };

            var builder = new ConfigurationBuilder();
            builder.AddInMemoryCollection(initialData);
            return builder.Build();
        }
    }
}
