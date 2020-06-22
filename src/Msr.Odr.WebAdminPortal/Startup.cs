// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Msr.Odr.Model.Configuration;
using Msr.Odr.Services;
using Msr.Odr.Services.Batch;
using Msr.Odr.Services.Configuration;
using Msr.Odr.WebAdminPortal.Services;
using Msr.Odr.WebAdminPortal.Users;

namespace Msr.Odr.WebAdminPortal
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
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
                Configuration = builder.Build();
                CurrentEnvironment = env;
            }
            catch (Exception ex)
            {
                new TelemetryClient().TrackException(ex);
                new LoggerFactory().CreateLogger("Exception").LogError("Exception '{0}' while building configuration: {1}", ex.GetType().Name, ex.Message);
                Configuration = config;
            }
        }

        public IConfiguration Configuration { get; }

        private IHostingEnvironment CurrentEnvironment { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddApplicationInsightsTelemetry();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IConfiguration>(Configuration);

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
            services.Configure<BatchConfiguration>((config) =>
            {
                Configuration.GetSection("batch").Bind(config);
                BatchConfiguration.Validate(config);
            });
            services.Configure<AppInsightsConfiguration>((config) =>
            {
                Configuration.GetSection("applicationinsights").Bind(config);
                AppInsightsConfiguration.Validate(config);
            });

            var armTemplatesMap = new ArmTemplatesMap();
            services.AddSingleton(armTemplatesMap);
            var staticAssetsMap = new StaticAssetsMap();
            services.AddSingleton(staticAssetsMap);

            services.AddMemoryCache();




            //services
            //    .AddAuthentication(options =>
            //    {
            //        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            //        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            //    })
            //    .AddJwtBearer(options =>
            //    {
            //        options.Audience = Configuration["azureADConfig:clientId"];
            //        options.Authority = Configuration["azureADConfig:authority"];
            //        options.Events = new JwtBearerEvents
            //        {
            //            OnMessageReceived = FetchAuthTokenFromCookie,
            //            OnTokenValidated = async context =>
            //            {
            //                var principal = context.Principal;
            //                var ODREmailList = Configuration["ODRAdminList"].Split(";");
            //            }
            //        };
            //    });

            //services
            //    .AddAuthentication(options =>
            //    {
            //        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            //    })
            //    .AddJwtBearer(options =>
            //    {
            //        options.Audience = Configuration["azureADConfig:clientId"];
            //        options.Authority = Configuration["azureADConfig:authority"];

            //        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
            //        {
            //            ValidAudience = Configuration["azureADConfig:clientId"],
            //            ValidIssuer = $"https://login.microsoftonline.com/" + Configuration["azureADConfig:tenantId"] + "/v2.0"
            //        };

            //        options.Events = new JwtBearerEvents
            //        {
            //            OnMessageReceived = FetchAuthTokenFromCookie,
            //            //OnTokenValidated = async context =>
            //            //{
            //            //    var principal = context.Principal;

            //            //    if (Configuration.GetChildren().Any(item => item.Key == "ODRAdminList") && principal.Claims.Any(c => c.Type == "preferred_username"))
            //            //    {

            //            //        var ODRAdminsList = Configuration["ODRAdminList"].Split(";").ToList();
            //            //        var testSubject = principal.Claims.FirstOrDefault(c => c.Type == "preferred_username").Value;

            //            //        if (ODRAdminsList.Contains(testSubject))
            //            //        {

            //            //            var claims = new List<Claim>
            //            //            {
            //            //                new Claim(ClaimTypes.Role, "ODRAdmin")
            //            //            };
            //            //            var appIdentity = new ClaimsIdentity(claims);
            //            //            principal.AddIdentity(appIdentity);

            //            //        }
            //            //    }
            //            //}
            //        };
            //    });

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
                        OnMessageReceived = FetchAuthTokenFromCookie,
                        OnAuthenticationFailed = context =>
                        {
                            var ctx = context;
                            return Task.FromResult(0);
                        },
                    };
                });

            services.AddMvc();

            services
                .AddAuthorization(options =>
                {
                    options.AddPolicy(PolicyNames.MustBeInAdminGroup, policy =>
                    {
                        var authorizedAdminUsers = Configuration["AuthorizedAdminUsers"];
                        policy.Requirements.Add(new AzureActiveDirectoryGroupRequirement(authorizedAdminUsers));
                    });
                });

            services.AddSingleton<IAuthorizationHandler, AzureActiveDirectoryGroupHandler>();

            //if (CurrentEnvironment.IsDevelopment())
            //{
            //    services.AddSingleton<IAuthorizationHandler, DevelopmentOnlyNoAuthDirectoryGroupHandler>();
            //}
            //else
            //{
            //    services.AddSingleton<IAuthorizationHandler, AzureActiveDirectoryGroupHandler>();
            //}

            services.AddScoped<DatasetSearchService>();
            services.AddScoped<UserDataSearchService>();
            services.AddScoped<DatasetStorageService>();
            services.AddScoped<AppInsightsService>();
            services.AddScoped<UserDataStorageService>();
            services.AddScoped<LicenseStorageService>();
            services.AddScoped<SasTokenService>();
            services.AddScoped<ValidationService>();
            services.AddScoped<ApplicationJobs>();

            // In production, the Angular files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ui-app";
            });

            // Add Swagger generator
            if (Configuration.GetValue<bool>("Application:EnableSwagger"))
            {
                services.AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("v1", new Swashbuckle.AspNetCore.Swagger.Info
                    {
                        Title = "MSR ODR Admin API",
                        Version = "v1"
                    });
                });
            }
        }



        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            //app.UseMiddleware<WebApiErrorHandlerMiddleware>();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                //app.UseExceptionHandler("/Home/Error");
                app.UseExceptionHandler(AppExceptionHandler.Configure);
            }

            app.UseAuthentication();

            // Set up Swagger end point
            if (Configuration.GetValue<bool>("Application:EnableSwagger"))
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "MSR ODR Admin API (v1)");
                });
            }



            app.UseStaticFiles();
            app.UseSpaStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action=Index}/{id?}");
            });

            app.UseSpa(spa =>
            {
                // To learn more about options for serving an Angular SPA from ASP.NET Core,
                // see https://go.microsoft.com/fwlink/?linkid=864501

                spa.Options.SourcePath = "../odr-ui/projects/odr-ui-admin";

                if (env.IsDevelopment())
                {
                    // spa.UseAngularCliServer(npmScript: "start");
                    spa.UseProxyToSpaDevelopmentServer("http://localhost:4200");
                }
            });
        }

        private readonly PathString _assetsPathString = new PathString("/api/assets");
        private readonly PathString _otherLicenseFileDownloadPrefix = new PathString("/api/dataset-nominations");
        private readonly string _otherLicenseFileDownloadSuffix = "other-license-file";
        private readonly PathString _authTokenPathString = new PathString("/api/assets/auth-token");

        private Task FetchAuthTokenFromCookie(MessageReceivedContext ctx)
        {
            if (ctx.Request.Path.StartsWithSegments(_assetsPathString) || 
                IsOtherLicenseFileDownload(ctx.Request.Path))
            {
                if (!ctx.Request.Path.Equals(_authTokenPathString))
                {
                    var authToken = ctx.Request.Cookies[Constants.AssetsAuthTokenName];
                    if (authToken != null)
                    {
                        ctx.Token = authToken;
                    }
                }
            }
            return Task.CompletedTask;
        }

        private bool IsOtherLicenseFileDownload(PathString requestString)
        {
            return requestString.StartsWithSegments(_otherLicenseFileDownloadPrefix) &&
                   requestString.Value.EndsWith(_otherLicenseFileDownloadSuffix);
        }
    }
}
