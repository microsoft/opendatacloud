using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Msr.Odr.Services;
using Msr.Odr.WebAdminPortal.Users;

namespace Msr.Odr.WebAdminPortal.Controllers
{
    [Route("api")]
    [Authorize(Policy = PolicyNames.MustBeInAdminGroup)]
    public class DomainsController : Controller
    {
        private const int CacheTimeInSeconds = 15 * 60;

        public DomainsController(DatasetStorageService datasetStorage, DatasetSearchService datasetSearch)
        {
            DatasetStorage = datasetStorage;
            DatasetSearch = datasetSearch;
        }

        private DatasetStorageService DatasetStorage { get; }
        private DatasetSearchService DatasetSearch { get; }

        [HttpGet("domains")]
		[ResponseCache(Duration = CacheTimeInSeconds)]
        public async Task<IActionResult> GetDomains(CancellationToken cancellationToken)
		{
		    cancellationToken.ThrowIfCancellationRequested();
		    var results = await DatasetStorage.GetAllDomains(cancellationToken).ConfigureAwait(false);
		    return Json(results);
        }

        [HttpGet("tags")]
		[ResponseCache(Duration = CacheTimeInSeconds)]
        public async Task<IActionResult> GetTags(CancellationToken cancellationToken)
		{
		    cancellationToken.ThrowIfCancellationRequested();
		    var results = await DatasetSearch.GetTags(cancellationToken).ConfigureAwait(false);
		    return Json(results);
        }

        [HttpGet("filetypes")]
        [ResponseCache(Duration = CacheTimeInSeconds)]
        public async Task<IActionResult> GetFileTypes(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var results = await DatasetSearch.GetFileTypes(cancellationToken).ConfigureAwait(false);
            return Json(results);
        }
    }
}
