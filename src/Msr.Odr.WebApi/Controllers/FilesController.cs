using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Msr.Odr.Api.Attributes;
using Msr.Odr.Api.Services;
using Msr.Odr.Model;
using Msr.Odr.Services;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Msr.Odr.Api.Controllers
{
    /// <summary>
    /// Provides access to files within the dataset.
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.Controller" />
    [Route(RouteConstants.DatasetControllerRoute)]
    public class FilesController : Controller
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FilesController" /> class.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="storage">The storage.</param>
        public FilesController(
            FileSearchService index,
            FileStorageService storage,
            GenerateFilePreview generateFilePreview)
        {
            Index = index;
            Storage = storage;
            GenerateFilePreview = generateFilePreview;
        }

        /// <summary>
        /// Gets the File search index repository.
        /// </summary>
        /// <value>The File repository.</value>
        private FileSearchService Index
        {
            get;
        }

        /// <summary>
        /// Gets the File storage repository.
        /// </summary>
        /// <value>The File storage repository.</value>
        private FileStorageService Storage
        {
            get;
        }

        public GenerateFilePreview GenerateFilePreview { get; }

        /// <summary>
        /// Gets the files associated with the specified dataset.
        /// </summary>
        /// <param name="datasetId">The dataset identifier.</param>
        /// <param name="page">The page to retrieve.</param>
        /// <param name="folder">The parent folder.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The collection of files in the specified page</returns>
        [HttpGet("{datasetId:guid}/files")]
        [SwaggerOperation(OperationId = "Files_Get")]
        [Produces(contentType: "application/json")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(PagedResult<FileEntry>), Description = "Returns the dataset content in the specified folder")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, Description = "Dataset or folder not found")]
        public async Task<IActionResult> Get([FromRoute] Guid datasetId, [FromQuery] int page, [FromQuery] string folder, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return this.Ok(await this.Index.GetAsync(datasetId, folder, Math.Max(0, page-1), cancellationToken).ConfigureAwait(false));
        }

        /// <summary>
        /// Gets the file by its identifier.
        /// </summary>
        /// <param name="datasetId">The dataset identifier.</param>
        /// <param name="id">The file identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The file details</returns>
        [HttpGet("{datasetId:guid}/files/{id:guid}")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(FileEntry), Description = "Returns the details of the specified file")]
        [SwaggerOperation(OperationId = "Files_GetById")]
        [Produces(contentType: "application/json")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, Description = "File not found")]
        [ResourceNotFoundExceptionFilter]
        public async Task<IActionResult> GetById([FromRoute] Guid datasetId, [FromRoute] Guid id, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var result = await this.Storage.GetByIdAsync(datasetId, id, cancellationToken).ConfigureAwait(false);
            if (result == null)
            {
                return this.NotFound();
            }

            return this.Ok(result);
        }

        /// <summary>
        /// Gets the preview for the specified file.
        /// </summary>
        /// <param name="datasetId">The dataset identifier.</param>
        /// <param name="id">The file identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The preview data for the file</returns>
        [HttpGet("{datasetId:guid}/files/{id:guid}/preview", Name = "GetPreview")]
        [SwaggerOperation(OperationId = "Files_GetPreview")]
        [Produces("application/octet-stream", "text/plain")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(Stream), Description = "Returns the file preview data")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, Description = "File not found or no preview available")]
        public async Task<IActionResult> GetPreview([FromRoute] Guid datasetId, [FromRoute] Guid id, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var result = await GenerateFilePreview.GeneratePreview(datasetId, id, cancellationToken);
            if (result == null)
            {
                return NotFound();
            }

            return Content(result, "text/plain");
        }

        /// <summary>
        /// Gets the direct download URL for the file
        /// </summary>
        /// <param name="datasetId">The dataset identifier.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The URL</returns>
		[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("{datasetId:guid}/files/{id:guid}/url")]
        [SwaggerOperation(OperationId = "Files_GetUrl")]
        [Produces("application/json")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(Uri), Description = "Returns the URL for directly downloading the file")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, Description = "File not found")]
        [ResourceNotFoundExceptionFilter]
        public async Task<IActionResult> GetDownloadUrl([FromRoute] Guid datasetId, [FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var response = await this.Storage.GetDownloadUriAsync(datasetId, id, cancellationToken).ConfigureAwait(false);
            if (response == null)
            {
                return this.NotFound();
            }

            return this.Ok(response);
        }
    }
}
