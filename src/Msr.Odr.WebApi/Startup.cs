// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.AzureAppServices;
using Msr.Odr.Api.Configuration;
using Msr.Odr.Api.Services;
using Msr.Odr.Model.Configuration;
using Msr.Odr.Services;
using Msr.Odr.Services.Batch;
using Msr.Odr.Services.Configuration;
using Msr.Odr.WebApi.Services;
using Newtonsoft.Json.Converters;

namespace Msr.Odr.Api
{
    /// <summary>
    /// Defines the startup and bootstrapping of the instance.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class. Initiates template mapping for all Deploy to Azure Options.
        /// </summary>
        /// <param name="env">The environment.</param>
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("template-mapping.json", optional: false, reloadOnChange: true)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            var config = builder.Build();

            string keyVaultUrl = config["keyVaultUrl"];
            if (string.IsNullOrWhiteSpace(keyVaultUrl))
            {
                throw new InvalidOperationException("Could not find keyVaultUrl configuration setting.");
            }

            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            var keyVaultCallback = new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback);
            var keyVaultClient = new KeyVaultClient(keyVaultCallback);
            builder.AddAzureKeyVault(keyVaultUrl, keyVaultClient, new DefaultKeyVaultSecretManager());

            try
            {
                this.Configuration = builder.Build();
            }
            catch (Exception ex)
            {
				new TelemetryClient().TrackException(ex);
				new LoggerFactory().CreateLogger("Exception").LogError("Exception '{0}' while building configuration: {1}", ex.GetType().Name, ex.Message);
                this.Configuration = config;
            }
            
			this.UseSwagger = string.Compare(this.Configuration["Application:EnableSwagger"], "true", StringComparison.OrdinalIgnoreCase) == 0;
            this.EnableExceptionDetails = string.Compare(this.Configuration["Application:EnableExceptionDetails"], "true", StringComparison.OrdinalIgnoreCase) == 0;
        }

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        /// <value>The configuration.</value>
        public IConfigurationRoot Configuration { get; }

        /// <summary>
        /// Gets a value indicating whether to use Swagger
        /// </summary>
        /// <value><c>true</c> if Swagger should be enabled; otherwise, <c>false</c>.</value>
        private bool UseSwagger { get; }

		/// <summary>
        /// Gets a value indicating whether detailed exceptions should be returned to the client.
        /// </summary>
        private bool EnableExceptionDetails { get; }

        /// <summary>
        /// Adds the runtime services to the instance.
        /// </summary>
        /// <param name="services">The services.</param>
        public void ConfigureServices(IServiceCollection services)
		{
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IConfiguration>(this.Configuration);
            services.AddSingleton<ITelemetryInitializer, TelemetryUserAndSessionContext>();
            services.AddApplicationInsightsTelemetry();

            services.AddOptions();
            services.Configure<IdentityProviderConfiguration>(options =>
            {
                Configuration.GetSection("azuread").Bind(options);
                IdentityProviderConfiguration.Validate(options);
            });
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
		    services.Configure<WebServerConfiguration>(options =>
		    {
		        Configuration.GetSection("webserver").Bind(options);
		        WebServerConfiguration.Validate(options);
		    });
            services.Configure<BatchConfiguration>((config) =>
            {
                Configuration.GetSection("batch").Bind(config);
                BatchConfiguration.Validate(config);
            });

            var armTemplatesMap = Configuration.GetSection("armTemplates").Get<ArmTemplatesMap>();
            services.AddSingleton(armTemplatesMap);
            var staticAssetsMap = Configuration.GetSection("staticAssets").Get<StaticAssetsMap>();
            services.AddSingleton(staticAssetsMap);

            services.AddMemoryCache();

			services.AddCors(options =>
			{
				options.AddPolicy("DefaultPolicy",
					builder => builder.AllowAnyOrigin()
					.WithMethods("GET", "POST", "PUT", "DELETE")
					.AllowAnyHeader()
					.AllowCredentials());
			});

            services.AddResponseCompression(opt =>
            {
                opt.EnableForHttps = true;
            });

			services.AddMvc()
				.AddMvcOptions(options =>
                {
                    options.Filters.Add(new TelemetryExceptionFilter(new LoggerFactory()));
                })
				.AddJsonOptions(options =>
				{
					options.SerializerSettings.Converters.Add(new StringEnumConverter
					{
						CamelCaseText = true
					});
				});

            services.AddScoped<DatasetSearchService>();
            services.AddScoped<FileSearchService>();
            services.AddScoped<DatasetStorageService>();
            services.AddScoped<DatasetOwnersService>();
            services.AddScoped<DatasetEditStorageService>();
            services.AddScoped<UserDataStorageService>();
            services.AddScoped<FileStorageService>();
            services.AddScoped<LicenseStorageService>();
		    services.AddScoped<ValidationService>();
            services.AddScoped<SasTokenService>();
            services.AddScoped<GenerateFilePreview>();
            services.AddScoped<CurrentUserService>();
            services.AddScoped<ApplicationJobs>();

            if (this.UseSwagger)
            {
                services.UseSwagger(this.Configuration);
            }

            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    var tenantName = Configuration["AzureAD:Tenant"].Split('.').First();
                    var policyName = Configuration["AzureAD:Policy"];
                    var audience = Configuration["AzureAD:Audience"];
                    options.MetadataAddress = $"https://{tenantName}.b2clogin.com/{tenantName}.onmicrosoft.com/{policyName}/v2.0/.well-known/openid-configuration";
                    options.Audience = audience;
                    options.Events = new JwtBearerEvents
                    {
                        OnAuthenticationFailed = context =>
                        {
                            var ctx = context;
                            return Task.FromResult(0);
                        },
                    };
                });
        }

        /// <summary>
        /// Configures the application's HTTP request pipeline.
        /// </summary>
        /// <param name="app">The application.</param>
        /// <param name="env">The env.</param>
        /// <param name="loggerFactory">The logger factory.</param>
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
            
            loggerFactory.AddAzureWebAppDiagnostics(
			  new AzureAppServicesDiagnosticsSettings
			  {
				  OutputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss zzz} [{Level}] {RequestId}-{SourceContext}: {Message}{NewLine}{Exception}"
			  }
			);

            if (this.EnableExceptionDetails)
            {
                app.ConfigureErrorHandling(loggerFactory);
            }

            if (!string.IsNullOrEmpty(this.Configuration["AzureAd:Tenant"]))
            {
                app.UseAuthentication();
            }
            else
            {
                loggerFactory.CreateLogger<Startup>().LogError("B2C connection details are missing. Authentication disabled");
            }

            app.UseCors("DefaultPolicy");

            app.UseResponseCompression();

			app.UseMvc();

            if (this.UseSwagger)
            {
                app.ConfigureSwagger();
            }
        }
    }
}