using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Msr.Odr.Api.Services;
using Swashbuckle.AspNetCore.SwaggerGen;
using Msr.Odr.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace Msr.Odr.Api.Controllers
{
	/// <summary>
	/// Provides access to the available tags.
	/// </summary>
	[Route(RouteConstants.TagsControllerRoute)]
	public class TagsController : Controller
	{
        /// <summary>
        /// Initializes a new instance of the <see cref="TagsController"/> class.
        /// </summary>
        /// <param name="index">The index service.</param>
        /// <param name="storage">The storage service.</param>
        public TagsController(DatasetSearchService index, DatasetStorageService storage)
        {
            this.Index = index;
            this.Storage = storage;
        }

        /// <summary>
        /// Gets the dataset search index repository.
        /// </summary>
        /// <value>The dataset repository.</value>
        private DatasetSearchService Index
        {
            get;
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
        /// <returns>The collection of available tags.</returns>
        [HttpGet("")]
		[SwaggerOperation(OperationId = "Tags_Get")]
		[Produces(contentType: "application/json")]
		[SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(IEnumerable<string>), Description = "Returns the available tags across all datasets")]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var results = await this.Index.GetTags(cancellationToken).ConfigureAwait(false);

            if (results == null || results.Count() == 0)
            {
                return this.NoContent();
            }

            return this.Ok(results);
        }
    }
}
