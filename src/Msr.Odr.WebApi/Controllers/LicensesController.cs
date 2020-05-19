using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Net;
using Msr.Odr.Api.Attributes;
using Msr.Odr.Api.Services;
using Msr.Odr.Services;
using System.Threading;
using Swashbuckle.AspNetCore.Annotations;

namespace Msr.Odr.Api.Controllers
{
    /// <summary>
    /// Provides access to available licenses
    /// </summary>
    [Route(RouteConstants.LicenseControllerRoute)]
    public class LicensesController : Controller
    {
        /// <summary>
        /// The cache time for response.
        /// </summary>
        private const int CacheTimeInSeconds = 15 * 60;

        /// <summary>
        /// Initializes a new instance of the <see cref="LicensesController" /> class.
        /// </summary>
        /// <param name="service">The license storage service</param>
        public LicensesController(LicenseStorageService service)
        {
            this.LicenseStorageService = service;
        }

        /// <summary>
        /// Gets the license storage service.
        /// </summary>
        /// <value>The license service.</value>
        private LicenseStorageService LicenseStorageService { get; }

  //      /// <summary>
  //      /// Gets the available licenses
  //      /// </summary>
  //      /// <param name="cancellationToken">The cancellation token.</param>
  //      /// <returns>The licenses</returns>
  //      [HttpGet("")]
		//[ResponseCache(Duration = CacheTimeInSeconds)]
  //      [SwaggerOperation(OperationId = "License_Get")]
  //      [Produces(contentType: "application/json")]
  //      [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(IEnumerable<License>), Description = "Returns the available licenses across all datasets")]
  //      public async Task<IActionResult> Get(CancellationToken cancellationToken)
		//{
  //          var licenses = await this.LicenseStorageService.GetAsync(cancellationToken).ConfigureAwait(false);
  //          return this.Json(licenses);
  //      }

        [HttpGet("standard")]
        [ResponseCache(Duration = CacheTimeInSeconds)]
        [SwaggerOperation(OperationId = "License_GetStandard")]
        [Produces(contentType: "application/json")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(IEnumerable<License>), Description = "Returns the available standard licenses across all datasets")]
        public async Task<IActionResult> GetStandard(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var licenses = await LicenseStorageService.GetStandardAsync(cancellationToken).ConfigureAwait(false);
            return Json(licenses);
        }

        /// <summary>
        /// Gets the license by its identifier.
        /// </summary>
        /// <param name="id">The identifier for the license.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The license details.</returns>
        [HttpGet("{id:guid}")]
        [ResponseCache(Duration = CacheTimeInSeconds)]
        [SwaggerOperation(OperationId = "License_GetById")]
        [Produces(contentType: "application/json")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(License), Description = "Returns the license by its identifier")]
        public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var result = await this.LicenseStorageService.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);

			if (result == null)
            {
                return this.NotFound();
            }

            return this.Json(result);
        }

        /// <summary>
        /// Gets the content by identifier.
        /// </summary>
        /// <param name="id">The identifier for the license.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The content of the license</returns>
        [HttpGet("{id:guid}/content")]
        [ResponseCache(Duration = CacheTimeInSeconds)]
        [SwaggerOperation(OperationId = "License_GetContentById")]
        [Produces(contentType: "application/json")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(string), Description = "Returns the available licenses across all datasets")]
        public async Task<IActionResult> GetContentById([FromRoute] Guid id, CancellationToken cancellationToken)
		{
            var result = await this.LicenseStorageService.GetContentAsync(id, cancellationToken).ConfigureAwait(false);

            if (result == null)
            {
                return this.NotFound();
            }

            return this.Json(result);
        }


        /// <summary>
        /// Gets the download license file by its identifier.
        /// </summary>
        /// <param name="id">The dataset identifier.</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The requested dataset.</returns>
        [HttpGet("{id:guid}/file")]
        [Produces("application/json")]
        public async Task<IActionResult> GetLicenseFile([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var license = await LicenseStorageService.GetLicenseFileAsync(id, cancellationToken);
            if (license?.Content == null)
            {
                return NotFound();
            }

            return File(license.Content, license.ContentType, license.FileName);
        }

        /// <summary>
        /// Gets the license by its identifier.
        /// </summary>
        /// <param name="id">The dataset identifier.</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The requested dataset.</returns>
        [HttpGet("{id:guid}/view")]
        public async Task<IActionResult> GetLicenseFileForView([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var license = await LicenseStorageService.GetLicenseFileAsync(id, cancellationToken);
            if (license?.Content == null)
            {
                return NotFound();
            }

            return File(license.Content, license.ContentType);
        }
    }
}
