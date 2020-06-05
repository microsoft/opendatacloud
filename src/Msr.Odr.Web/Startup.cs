// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Policy;

namespace Msr.Odr.Web
{
    public class Startup
    {
        private HttpClient httpClient = new HttpClient();

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            var config = builder.Build();

            try
            {
                this.Configuration = builder.Build();
            }
            catch (Exception ex)
            {
                new LoggerFactory().CreateLogger("Exception").LogError("Exception '{0}' while building configuration: {1}", ex.GetType().Name, ex.Message);
                this.Configuration = config;
            }

            this.EnableExceptionDetails = string.Compare(this.Configuration["Application:EnableExceptionDetails"], "true", StringComparison.OrdinalIgnoreCase) == 0;
        }

        public IConfigurationRoot Configuration { get; }

        /// <summary>
        /// Gets a value indicating whether detailed exceptions should be returned to the client.
        /// </summary>
        private bool EnableExceptionDetails { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddApplicationInsightsTelemetry();
            services.AddRouting();

            // In production, the Angular files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ui-app";
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment() || env.IsStaging())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseDeveloperExceptionPage();
                //app.UseExceptionHandler("/Home/Error");
            }

            var appConfig = new AppConfiguration(Configuration);
            var currentConfig = new Lazy<string>(() =>
            {
                return JsonConvert.SerializeObject(appConfig, new JsonSerializerSettings
                {
                    Formatting = Formatting.None,
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                });
            });

            app.UseRouter(r =>
            {
                r.MapGet("assets/app-config.json", async (ctx) =>
                {
                    ctx.Response.ContentType = "application/json";
                    await ctx.Response.WriteAsync(currentConfig.Value);
                });

                r.MapGet("robots.txt", async (ctx) =>
                {
                    var siteMapUrl = new UriBuilder(appConfig.SiteMap)
                    {
                        Path = "sitemap.xml"
                    }.Uri;
                    ctx.Response.ContentType = "text/plain";
                    var text = string.Join("\n", new []
                    {
                        "User-agent: *",
                        "Allow: /",
                        "",
                        $"Sitemap: {siteMapUrl}",
                        "",
                    });
                    await ctx.Response.WriteAsync(text);
                });

                r.MapGet("sitemap.xml", async (ctx) =>
                {
                    var siteMapUrl = new UriBuilder(appConfig.ApiBaseUrl)
                    {
                        Path = "site-map"
                    }.Uri;
                    using (var siteMapResponse = await httpClient.GetAsync(siteMapUrl, HttpCompletionOption.ResponseHeadersRead))
                    using (var readStream = await siteMapResponse.Content.ReadAsStreamAsync())
                    {
                        ctx.Response.StatusCode = (int)siteMapResponse.StatusCode;
                        siteMapResponse.Headers
                            .Concat(siteMapResponse.Content.Headers)
                            .Where(header => !string.Equals(header.Key, "transfer-encoding", StringComparison.InvariantCultureIgnoreCase))
                            .ToList()
                            .ForEach(header =>
                            {
                                ctx.Response.Headers[header.Key] = header.Value.ToArray();
                            });
                        await readStream.CopyToAsync(ctx.Response.Body);
                    }
                });
            });

            var options = new RewriteOptions();
            if (!(env.IsDevelopment() || env.IsStaging()))
            {
                string domainName = Configuration["Application:CanonicalDomain"];
                if (!String.IsNullOrWhiteSpace(domainName))
                {
                    options.Add(new DomainRewriteRule(domainName));
                }
            }

            options
                .AddRewrite("^$", "index.html", skipRemainingRules: true)
                .AddRewrite("^search($|/.*$)", "index.html", skipRemainingRules: true)
                .AddRewrite("^auth($|/.*$)", "index.html", skipRemainingRules: true)
                .AddRewrite("^contact($|/.*$)", "index.html", skipRemainingRules: true)
                .AddRewrite("^datasets($|/.*$)", "index.html", skipRemainingRules: true)
                .AddRewrite("^categories($|/.*$)", "index.html", skipRemainingRules: true)
                .AddRewrite("^login($|/.*$)", "index.html", skipRemainingRules: true)
                .AddRewrite("^faq($|/.*$)", "index.html", skipRemainingRules: true)
                .AddRewrite("^issue($|/.*$)", "index.html", skipRemainingRules: true)
                .AddRewrite("^feedback($|/.*$)", "index.html", skipRemainingRules: true)
                .AddRewrite("^nominate($|/.*$)", "index.html", skipRemainingRules: true)
                .AddRewrite("^about($|/.*$)", "index.html", skipRemainingRules: true)
                .AddRewrite("^privacy-policy($|/.*$)", "index.html", skipRemainingRules: true)
                .AddRewrite("^usage-terms($|/.*$)", "index.html", skipRemainingRules: true)
                .AddRewrite("^google[a-z0-9]+\\.html$", "assets/$0", skipRemainingRules: true);
            app.UseRewriter(options);
            app.UseStaticFiles();
            app.UseSpaStaticFiles();

            app.UseSpa(spa =>
            {
                // To learn more about options for serving an Angular SPA from ASP.NET Core,
                // see https://go.microsoft.com/fwlink/?linkid=864501

                spa.Options.SourcePath = "../odr-ui/projects/odr-ui-web";

                if (env.IsDevelopment())
                {
                    // spa.UseAngularCliServer(npmScript: "start");
                    spa.UseProxyToSpaDevelopmentServer("http://localhost:4200");
                }
            });
        }
    }
}
