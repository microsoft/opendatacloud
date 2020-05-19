using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Options;
using Msr.Odr.Model;
using Msr.Odr.Services.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Msr.Odr.WebAdminPortal.Services
{
    /// <summary>
    /// Azure AppInsights API Service
    /// </summary>
    public class AppInsightsService
    {
        private HttpClient AppInsightsClient { get; }
        private AppInsightsConfiguration Configuration { get; }

        public AppInsightsService(IOptions<AppInsightsConfiguration> options)
        {
            this.Configuration = options.Value;
            this.AppInsightsClient = new HttpClient();
        }

        public async Task<List<ReportItem>> GetPageViewsPerDataset(
            List<DatasetDigestItem> datasets,
            CancellationToken cancellationToken,
            int maxNumberOfDatasets = 10)
        {
            var result = await ExecuteAppInsightsQuery(PageViewsByDatasetQuery, cancellationToken);
            return TransformReportItems(result, rows =>
            {
                return rows
                    .Select(data => (
                        Id: (string)data.ElementAt(0),
                        Count: (int)data.ElementAt(1)
                    ))
                    .Select(t => new ReportItem
                    {
                        Name = FindDatasetName(datasets, t.Id),
                        Count = t.Count
                    })
                    .Where(item => item.Name != null)
                    .Take(maxNumberOfDatasets)
                    .ToList();
            });
        }

        public async Task<List<ReportItem>> GetPageViewsByRegion(
            CancellationToken cancellationToken,
            int maxNumberOfRegions = 10)
        {
            var result = await ExecuteAppInsightsQuery(PageViewsByRegionQuery, cancellationToken);
            return TransformReportItems(result, rows =>
            {
                return rows
                    .Select(data => new ReportItem
                    {
                        Name = (string)data.ElementAt(0),
                        Count = (int)data.ElementAt(1)
                    })
                    .Take(maxNumberOfRegions)
                    .ToList();
            });
        }

        public async Task<List<ReportItem>> GetPageViewsByDate(CancellationToken cancellationToken)
        {
            var result = await ExecuteAppInsightsQuery(DatasetPageViewsByDateQuery, cancellationToken);
            return TransformReportItems(result, rows =>
            {
                return rows
                    .Select(data => new ReportItem
                    {
                        Name = DateTime.Parse((string)data.ElementAt(0)).ToString("yyyy-MM-dd"),
                        Count = (int)data.ElementAt(1),
                    })
                    .ToList();
            });
        }

        public async Task<List<ReportItem>> GetSearchesByDomain(
            CancellationToken cancellationToken,
            int maxNumberOfDomains = 10)
        {
            var result = await ExecuteAppInsightsQuery(SearchByDomainsQuery, cancellationToken);
            return TransformReportItems(result, rows =>
            {
                return rows
                    .Select(data => new ReportItem
                    {
                        Name = (string)data.ElementAt(0),
                        Count = (int)data.ElementAt(1)
                    })
                    .Take(maxNumberOfDomains)
                    .ToList();
            });
        }

        public async Task<List<ReportItem>> GetSearchesBySearchTerm(
            CancellationToken cancellationToken,
            int maxNumberOfTerms = 20)
        {
            var result = await ExecuteAppInsightsQuery(SearchByTermsQuery, cancellationToken);
            return TransformReportItems(result, rows =>
            {
                return rows
                    .Select(data => new ReportItem
                    {
                        Name = (string)data.ElementAt(0),
                        Count = (int)data.ElementAt(1)
                    })
                    .Take(maxNumberOfTerms)
                    .ToList();
            });
        }

        public async Task<List<ReportItem>> GetAzureDeploymentsPerDataset(
            List<DatasetDigestItem> datasets,
            CancellationToken cancellationToken,
            int maxNumberOfDatasets = 10)
        {
            var result = await ExecuteAppInsightsQuery(DatasetDeploymentsQuery, cancellationToken);
            return TransformReportItems(result, rows =>
            {
                return rows
                    .Select(data => (
                        Id: (string)data.ElementAt(0),
                        Count: (int)data.ElementAt(1)
                    ))
                    .Select(t => new ReportItem
                    {
                        Name = FindDatasetName(datasets, t.Id),
                        Count = t.Count
                    })
                    .Where(item => item.Name != null)
                    .Take(maxNumberOfDatasets)
                    .ToList();
            });
        }

        private string FindDatasetName(List<DatasetDigestItem> datasets, string id)
        {
            return datasets
                .Where(ds => string.Equals(ds.DatasetId.ToString(), id, StringComparison.InvariantCultureIgnoreCase))
                .Select(ds => ds.Name)
                .FirstOrDefault();
        }

        private List<ReportItem> TransformReportItems(JToken result, Func<IEnumerable<JArray>, List<ReportItem>> transform)
        {
            var rows = (JArray)result.SelectToken("tables[0].rows");
            if (rows == null)
            {
                return null;
            }

            return transform(rows.Cast<JArray>());
        }

        private async Task<JToken> ExecuteAppInsightsQuery(string query, CancellationToken cancellationToken)
        {
            var uriBuilder = new UriBuilder($"{Configuration.Uri}/query");
            var queryBuilder = new QueryBuilder();
            queryBuilder.Add("query", query.Trim().Replace('\n', ' '));
            uriBuilder.Query = queryBuilder.ToString();

            var request = new HttpRequestMessage(HttpMethod.Get, uriBuilder.Uri);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Add("x-api-key", this.Configuration.Key);
            var response = await AppInsightsClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                throw new ApplicationException($"Request failed to AppInsights API with error: {response.ReasonPhrase}\n{uriBuilder.Query}");
            }

            var json = await response.Content.ReadAsStringAsync();
            return JObject.Parse(json);
        }

        private const string PageViewsByDatasetQuery = @"
pageViews
| where timestamp > ago(90d) and url startswith '/datasets/'
| extend datasetId = tostring(split(url, '/')[2])
| summarize viewCount = count() by datasetId
| order by viewCount desc
";

        private const string PageViewsByRegionQuery = @"
pageViews
| where timestamp > ago(90d) and url startswith '/datasets/'
| summarize countForRegion = count() by client_CountryOrRegion
| order by countForRegion desc
";

        private const string DatasetPageViewsByDateQuery = @"
pageViews
| where timestamp > ago(90d) and url startswith '/datasets/'
| project viewDate = startofday(timestamp)
| summarize countViews = count() by viewDate
| order by viewDate asc
";

        private const string SearchByDomainsQuery = @"
requests
| where timestamp > ago(90d)
| extend parsed_url = parseurl(url)
| project domain = toupper(tostring(parsed_url['Query Parameters']['domain']))
| where isnotempty(domain)
| summarize domainCount = count() by domain
| order by domainCount desc
";

        private const string SearchByTermsQuery = @"
requests
| where timestamp > ago(90d)
| extend parsed_url = parseurl(url)
| project searchTerm = tostring(parsed_url['Query Parameters']['term'])
| where isnotempty(searchTerm)
| summarize searchTermCount = count() by searchTerm
| order by searchTermCount desc
";

        private const string DatasetDeploymentsQuery = @"
requests
| where timestamp > ago(90d) and operation_Name == 'GET AzureDeploy/GetTemplate [datasetId/deploymentId]'
| extend datasetId = tostring(split(url, '/')[4])
| summarize datasetCount = count() by datasetId
| order by datasetCount desc
";
    }
}
