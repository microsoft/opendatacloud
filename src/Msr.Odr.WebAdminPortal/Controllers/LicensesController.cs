// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Msr.Odr.Services;
using Msr.Odr.WebAdminPortal.Users;

namespace Msr.Odr.WebAdminPortal.Controllers
{
    [Route("api/licenses")]
    [Authorize(Policy = PolicyNames.MustBeInAdminGroup)]
    public class LicensesController : Controller
    {
        private const int CacheTimeInSeconds = 15 * 60;

        public LicensesController(LicenseStorageService service)
        {
            Licenses = service;
        }

        private LicenseStorageService Licenses { get; }

  //      [HttpGet("")]
		//[ResponseCache(Duration = CacheTimeInSeconds)]
  //      public async Task<IActionResult> Get(CancellationToken cancellationToken)
		//{
		//    cancellationToken.ThrowIfCancellationRequested();
  //          var licenses = await Licenses.GetAsync(cancellationToken).ConfigureAwait(false);
  //          return Json(licenses);
  //      }

        [HttpGet("standard")]
        [ResponseCache(Duration = CacheTimeInSeconds)]
        public async Task<IActionResult> GetStandard(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var licenses = await Licenses.GetStandardAsync(cancellationToken).ConfigureAwait(false);
            return Json(licenses);
        }

        /// <summary>
        /// Gets the license by its identifier.
        /// </summary>
        /// <param name="id">The license identifier.</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The requested dataset.</returns>
        [HttpGet("{id:guid}/view")]
        [AllowAnonymous]
        public async Task<IActionResult> GetLicenseFileForView([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var license = await Licenses.GetLicenseFileAsync(id, cancellationToken);
            if (license?.Content == null)
            {
                return NotFound();
            }

            return File(license.Content, license.ContentType);
        }
    }
}
