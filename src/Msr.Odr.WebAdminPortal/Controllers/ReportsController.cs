// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Msr.Odr.Model;
using Msr.Odr.Services;
using Msr.Odr.WebAdminPortal.Services;
using Msr.Odr.WebAdminPortal.Users;

namespace Msr.Odr.WebAdminPortal.Controllers
{
    [Route("api/reports")]
    [Authorize(Policy = PolicyNames.MustBeInAdminGroup)]
    [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any)]
    public class ReportsController : Controller
    {
        private DatasetStorageService _datasetStorage;
        private AppInsightsService _appInsightsService;
        private List<DatasetDigestItem> _datasetDigestItems = new List<DatasetDigestItem>();

        public ReportsController(
            DatasetStorageService datasetStorage,
            AppInsightsService appInsightsService)
        {
            _datasetStorage = datasetStorage;
            _appInsightsService = appInsightsService;
        }

        [HttpGet("datasets/domain")]
        public async Task<IActionResult> GetDatasetStatsByDomain(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var result = await _datasetStorage.GetCountOfDatasetsByDomain(cancellationToken).ConfigureAwait(false);
            return Ok(result);
        }

        [HttpGet("datasets/license")]
        public async Task<IActionResult> GetDatasetStatsByLicense(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var result = await _datasetStorage.GetCountOfDatasetsByLicense(cancellationToken).ConfigureAwait(false);
            return Ok(result);
        }

        [HttpGet("datasets/page-views")]
        public async Task<IActionResult> GetPageViewsByDataset(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!_datasetDigestItems.Any())
            {
                _datasetDigestItems = await _datasetStorage.GetDatasetDigestItems(cancellationToken);
            }

            var result = await _appInsightsService.GetPageViewsPerDataset(_datasetDigestItems, cancellationToken).ConfigureAwait(false);
            return Ok(result);
        }


        [HttpGet("views-by-region")]
        public async Task<IActionResult> GetViewsByRegion(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var result = await _appInsightsService.GetPageViewsByRegion(cancellationToken).ConfigureAwait(false);
            return Ok(result);
        }

        [HttpGet("views-by-date")]
        public async Task<IActionResult> GetViewsByDate(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var result = await _appInsightsService.GetPageViewsByDate(cancellationToken).ConfigureAwait(false);
            return Ok(result);
        }

        [HttpGet("searches-by-domain")]
        public async Task<IActionResult> GetSearchesByDomain(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var result = await _appInsightsService.GetSearchesByDomain(cancellationToken).ConfigureAwait(false);
            return Ok(result);
        }

        [HttpGet("searches-by-term")]
        public async Task<IActionResult> GetSearchesByTerm(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var result = await _appInsightsService.GetSearchesBySearchTerm(cancellationToken).ConfigureAwait(false);
            return Ok(result);
        }

        [HttpGet("datasets/azure-deployments")]
        public async Task<IActionResult> GetAzureDeploymentsByDataset(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!_datasetDigestItems.Any())
            {
                _datasetDigestItems = await _datasetStorage.GetDatasetDigestItems(cancellationToken);
            }
            var result = await _appInsightsService.GetAzureDeploymentsPerDataset(_datasetDigestItems, cancellationToken).ConfigureAwait(false);
            return Ok(result);
        }
    }
}