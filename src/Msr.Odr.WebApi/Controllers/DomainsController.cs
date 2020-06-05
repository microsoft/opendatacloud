// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Msr.Odr.Api.Services;
using Msr.Odr.Model;
using Swashbuckle.AspNetCore.SwaggerGen;
using Msr.Odr.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace Msr.Odr.Api.Controllers
{
    /// <summary>
    /// Provides access to the available tags.
    /// </summary>
    [Route(RouteConstants.DomainsControllerRoute)]
	public class DomainsController : Controller
	{
        /// <summary>
        /// Initializes a new instance of the <see cref="TagsController"/> class.
        /// </summary>
        /// <param name="index">The index service.</param>
        /// <param name="storage">The storage service.</param>
        public DomainsController(DatasetStorageService storage)
        {
            this.Storage = storage;
        }

        /// <summary>
        /// Gets the dataset storage repository.
        /// </summary>
        /// <value>The dataset storage repository.</value>
        private DatasetStorageService Storage
        {
            get;
        }

        /// <summary>
        /// Gets all domains.
        /// </summary>
        /// <returns>The collection of available domains.</returns>
        [HttpGet("")]
		[SwaggerOperation(OperationId = "Domains_Get")]
		[Produces(contentType: "application/json")]
		[SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(IEnumerable<Domain>), Description = "Returns the available domain classifications")]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var results = await this.Storage.GetAllDomains(cancellationToken).ConfigureAwait(false);

            if (results == null || results.Count() == 0)
            {
                return this.NoContent();
            }

            return this.Ok(results);
        }

        /// <summary>
        /// Gets only domains currently in use by datasets.
        /// </summary>
        /// <returns>The collection of domains currently in use by a dataset.</returns>
        [HttpGet("active")]
        [SwaggerOperation(OperationId = "Domains_GetInUse")]
        [Produces(contentType: "application/json")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(IEnumerable<Domain>), Description = "Returns the available domain classifications")]
        public async Task<IActionResult> GetDomainsInUseByDatasets(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var domains = await this.Storage.GetDomainsInUseByDatasets(cancellationToken)
                                    .ConfigureAwait(false);
            
            if (domains == null)
            {
                return this.NoContent();
            }

            // remove the Other category
            var results = domains.Where(d => d.Id != "Other").ToList();

            if (!results.Any())
            {
                return this.NoContent();
            }

            return this.Ok(results);
        }
    }
}
