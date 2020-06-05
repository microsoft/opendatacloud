// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Msr.Odr.Api.Attributes;
using Msr.Odr.Api.Services;
using Msr.Odr.Model.FileSystem;
using Msr.Odr.Services;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Msr.Odr.Api.Controllers
{
    /// <summary>
    /// Provides access to the available file types from the datasets.
    /// </summary>
    [Route(RouteConstants.FileTypesControllerRoute)]
    public class FileTypesController : Controller
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileTypesController"/> class.
        /// </summary>
        /// <param name="index">The index service.</param>
        /// <param name="storage">The storage service.</param>
        public FileTypesController(DatasetSearchService index)
        {
            this.Index = index;
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
        /// Gets the available file extensions
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The extensions for the files in all datasets</returns>
        [HttpGet("available")]
        [SwaggerOperation(OperationId = "FileTypes_GetAvailable")]
        [Produces(contentType: "application/json")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(IEnumerable<string>), Description = "Returns the available file types across all datasets")]
        public async Task<IActionResult> GetAvailableFileTypes(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var results = await this.Index.GetFileTypes(cancellationToken).ConfigureAwait(false);

			if (results == null || !results.Any())
            {
                return this.NoContent();
            }

            return this.Ok(results);
        }

        /// <summary>
        /// Gets the previewable file extensions
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The extensions for the files in all datasets</returns>
        [HttpGet("preview")]
        [SwaggerOperation(OperationId = "FileTypes_GetPreview")]
        [Produces(contentType: "application/json")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(IEnumerable<string>), Description = "Returns the previewable file types")]
        public IActionResult GetPreviewableFileTypes(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Ok(PreviewFileTypes.List);
        }
    }
}
