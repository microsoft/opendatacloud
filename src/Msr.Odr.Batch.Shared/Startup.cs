// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Msr.Odr.Model.Configuration;
using Msr.Odr.Services;
using Msr.Odr.Services.Batch;
using Msr.Odr.Services.Configuration;

namespace Msr.Odr.Batch.Shared
{
    public class Startup
    {
        public Startup()
        {
            var builder = new ConfigurationBuilder()
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        public IServiceProvider ServiceProvider { get; private set; }

        public void ConfigureServices()
        {
            var services = new ServiceCollection();

            services.AddMemoryCache();

            services.AddOptions();
            services.Configure<SearchConfiguration>(options =>
            {
                Configuration.GetSection("search").Bind(options);
                SearchConfiguration.Validate(options);
            });
            services.Configure<CosmosConfiguration>(options =>
            {
                Configuration.GetSection("documents").Bind(options);
                CosmosConfiguration.Validate(options);
            });
            services.Configure<StorageConfiguration>((config) =>
            {
                Configuration.GetSection("storage").Bind(config);
                StorageConfiguration.Validate(config);

            });
            services.Configure<BatchConfiguration>((config) =>
            {
                Configuration.GetSection("batch").Bind(config);
                BatchConfiguration.Validate(config);
            });

            var armTemplatesMap = new ArmTemplatesMap();
            services.AddSingleton(armTemplatesMap);
            var staticAssetsMap = new StaticAssetsMap();
            services.AddSingleton(staticAssetsMap);

            services.AddSingleton<ApplicationJobs>();
            services.AddSingleton<DatasetStorageService>();
            services.AddSingleton<DatasetOwnersService>();
            services.AddSingleton<DatasetEditStorageService>();
            services.AddSingleton<UserDataStorageService>();
            services.AddSingleton<SasTokenService>();
            services.AddSingleton<ValidationService>();
            services.AddSingleton<DatasetSearchService>();
            services.AddSingleton<FileSearchService>();

            ServiceProvider = services.BuildServiceProvider();
        }
    }
}
