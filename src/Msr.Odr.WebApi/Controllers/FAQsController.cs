using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Msr.Odr.Api.Services;
using Msr.Odr.Services;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Msr.Odr.Api.Controllers
{
	/// <summary>
	/// Provides access to the available tags.
	/// </summary>
	[Route(RouteConstants.FAQsControllerRoute)]
	public class FAQsController : Controller
	{
        /// <summary>
        /// Initializes a new instance of the <see cref="TagsController"/> class.
        /// </summary>
        /// <param name="index">The index service.</param>
        /// <param name="storage">The storage service.</param>
        public FAQsController(DatasetStorageService storage)
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
        /// Gets this instance.
        /// </summary>
        /// <returns>The collection of FAQs (in display order).</returns>
        [HttpGet("")]
		[SwaggerOperation(OperationId = "FAQs_Get")]
		[Produces(contentType: "application/json")]
		[SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(IEnumerable<string>), Description = "Returns the list of FAQs in display order")]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var results = await this.Storage.GetFAQs(cancellationToken).ConfigureAwait(false);

            if (results == null || results.Count() == 0)
            {
                return this.NoContent();
            }

            return this.Ok(results);
        }
    }
}
