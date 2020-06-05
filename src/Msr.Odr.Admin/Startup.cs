// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;

namespace Msr.Odr.Admin
{
    public static class Startup
    {
        private static IConfiguration configInstance;

        public static IConfiguration Configuration
        {
            get
            {
                if(configInstance == null)
                {
                    throw new InvalidOperationException("Configuration not available yet.");
                }

                return configInstance;
            }
        }

        public static string KeyVaultUrl => Configuration["keyVaultUrl"];

        public static void Initialize()
        {
            string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "development";
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile($"appsettings.{environment}.json", optional: true)
                .AddEnvironmentVariables();

            var initialConfig = builder.Build();

            var keyVaultUrl = initialConfig["keyVaultUrl"];
            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
            builder.AddAzureKeyVault(keyVaultUrl, keyVaultClient, new DefaultKeyVaultSecretManager());

            configInstance = builder.Build();
        }
    }
}
