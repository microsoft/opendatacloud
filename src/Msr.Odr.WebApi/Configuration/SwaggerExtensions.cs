// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.PlatformAbstractions;
using Swashbuckle.AspNetCore.Swagger;

namespace Msr.Odr.Api.Configuration
{
    /// <summary>
    /// Extensions to configure Swagger/Swashbuckle
    /// </summary>
    public static class SwaggerExtensions
    {
        /// <summary>
        /// Configures the services collection to enable Swagger support
        /// </summary>
        /// <param name="services">The services collection.</param>
        /// <param name="configuration">The configuration.</param>
        /// <returns>The services collection.</returns>
        public static IServiceCollection UseSwagger(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("ODRv1", new Info
                {
                    Version = "v1",
                    Title = "MSR ODR API v1",
                    Description = "Open Data Repository supporting API",
                    TermsOfService = "None",
                });

                var documentationFile = Path.Combine(PlatformServices.Default.Application.ApplicationBasePath,
                    Path.ChangeExtension(typeof(Startup).GetTypeInfo().Assembly.GetName().Name, "xml"));
                if (File.Exists(documentationFile))
                {
                    options.IncludeXmlComments(documentationFile);
                }

                options.DescribeAllEnumsAsStrings();
                options.DescribeStringEnumsInCamelCase();

                options.AddSecurityDefinition("oauth2", new OAuth2Scheme
                {
                    Type = "oauth2",
                    Description = "Requires authentication using Microsoft B2C AAD",
                    AuthorizationUrl = string.Format(@"https://{0}.b2clogin.com/{1}.onmicrosoft.com/oauth2/v2.0/authorize?p={2}&client_id={3}", configuration["AzureAD:Tenant"], configuration["AzureAD:Tenant"], configuration["AzureAD:Policy"], configuration["AzureAD:Audience"]),
                    //AuthorizationUrl = string.Format(@"https://login.microsoftonline.com/{0}/oauth2/v2.0/authorize", configuration["AzureAD:Tenant"]), //configuration["AzureAD:Policy"]),
                    //Scopes = new Dictionary<string, string>
                        //{
                           // { "user_impersonation", "Access datasets" }
                       // }
                });
                // Assign scope requirements to operations based on AuthorizeAttribute
                options.OperationFilter<SecurityRequirementsOperationFilter>();
            });

            return services;
        }

        /// <summary>
        /// Configures Swagger as part of the application pipeline.
        /// </summary>
        /// <param name="app">The application builder instance.</param>
        /// <returns>The application builder instance.</returns>
        public static IApplicationBuilder ConfigureSwagger(this IApplicationBuilder app)
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/ODRv1/swagger.json", "MSR ODR API v1");
                //options.ConfigureOAuth2(string.Empty, string.Empty, string.Empty, string.Empty);
                options.OAuthClientId("");
                options.OAuthClientSecret("");
                options.OAuthRealm("");
                options.OAuthAppName("");
                options.OAuthUseBasicAuthenticationWithAccessCodeGrant();
            });

            return app;
        }
    }
}
