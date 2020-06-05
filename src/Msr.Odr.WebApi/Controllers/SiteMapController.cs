// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Msr.Odr.Api.Attributes;
using Msr.Odr.Api.Services;
using Msr.Odr.Model;
using Swashbuckle.AspNetCore.SwaggerGen;
using Msr.Odr.Services;
using Swashbuckle.AspNetCore.Annotations;
using Msr.Odr.WebApi.ViewModels;
using Msr.Odr.WebApi.Services;
using System.Xml;
using Microsoft.Extensions.Options;
using Msr.Odr.Services.Configuration;
using Microsoft.Extensions.Caching.Memory;

namespace Msr.Odr.Api.Controllers
{
    /// <summary>
    /// Generates site map for application
    /// </summary>
    [Route("site-map")]
    public class SiteMapController : Controller
    {
        private DatasetStorageService DatasetStorageService { get; }
        private IOptions<WebServerConfiguration> WebServer { get; }
        private IMemoryCache MemCache { get; }
        private static readonly string SiteMapKey = "site-map-entries";

        public SiteMapController(
            DatasetStorageService storage,
            IOptions<WebServerConfiguration> webServer,
            IMemoryCache memCache)
        {
            this.DatasetStorageService = storage;
            this.WebServer = webServer;
            this.MemCache = memCache;
        }

        [HttpGet("")]
        public async Task GetSiteMap(CancellationToken cancellationToken)
		{
            cancellationToken.ThrowIfCancellationRequested();

            var allDatasetEntries = await MemCache.GetOrCreateAsync(SiteMapKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
                return await this.DatasetStorageService.GetAllDatasetsForSiteMap(cancellationToken).ConfigureAwait(false);
            });

            var uriBuilder = new UriBuilder(WebServer.Value.SiteMap);

            var settings = new XmlWriterSettings();
            settings.Async = true;
            settings.CloseOutput = false;

            this.Response.StatusCode = (int)HttpStatusCode.OK;
            this.Response.ContentType = "text/xml";
            using (var xml = XmlWriter.Create(this.Response.Body, settings))
            {
                async Task WriteEntry(string path, DateTime? modified = null)
                {
                    uriBuilder.Path = path;
                    await xml.WriteStartElementAsync(null, "url", null);
                    await xml.WriteElementStringAsync(null, "loc", null, uriBuilder.Uri.ToString());
                    if (modified != null)
                    {
                        await xml.WriteElementStringAsync(null, "lastmod", null, modified?.ToString("yyyy-MM-dd"));
                    }
                    await xml.WriteEndElementAsync();
                }

                await xml.WriteStartDocumentAsync();
                await xml.WriteStartElementAsync(null, "urlset", "http://www.sitemaps.org/schemas/sitemap/0.9");
                await WriteEntry("/");
                await WriteEntry("/categories");
                await WriteEntry("/about");
                await WriteEntry("/faq");
                foreach (var entry in allDatasetEntries)
                {
                    await WriteEntry($"/datasets/{entry.DatasetId}", entry.Modified);
                }
                await xml.WriteEndElementAsync();
                await xml.WriteEndDocumentAsync();
                await xml.FlushAsync();
            }
        }
    }
}
