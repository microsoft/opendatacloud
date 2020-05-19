using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Msr.Odr.Api.Attributes;
using Msr.Odr.Api.Services;
using Msr.Odr.Model;
using Swashbuckle.AspNetCore.SwaggerGen;
using Msr.Odr.Services;
using Swashbuckle.AspNetCore.Annotations;
using Msr.Odr.WebApi.ViewModels;
using Msr.Odr.WebApi.Services;

namespace Msr.Odr.Api.Controllers
{
    /// <summary>
    /// Provides access to the datasets.
    /// </summary>
    [Route(RouteConstants.DatasetControllerRoute)]
    public class DatasetsController : Controller
    {
        private DatasetSearchService DataSearchService { get; }
        private DatasetStorageService DatasetStorageService { get; }
        private CurrentUserService CurrentUserService { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DatasetsController" /> class.
        /// </summary>
        /// <param name="index">The search indexes.</param>
        /// <param name="storage">The storage repository.</param>
        public DatasetsController(DatasetSearchService index, DatasetStorageService storage, CurrentUserService currentUserService)
        {
            this.DataSearchService = index;
            this.DatasetStorageService = storage;
            this.CurrentUserService = currentUserService;
        }

        /// <summary>
        /// Gets the available datasets
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The datasets</returns>
        [HttpGet("")]
        [SwaggerOperation(OperationId = "Dataset_Get")]
        [Produces(contentType: "application/json")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(PagedResult<DatasetViewModel>), Description = "Returns the available datasets")]
        public async Task<IActionResult> Get([FromQuery] int page, CancellationToken cancellationToken)
		{
            cancellationToken.ThrowIfCancellationRequested();
            var result = await this.DataSearchService.GetAsync(Math.Max(0, page - 1), cancellationToken).ConfigureAwait(false);
            return Ok(new PagedResult<DatasetViewModel>
            {
                RecordCount = result.RecordCount,
                PageCount = result.PageCount,
                Value = result.Value.Select(d => d.ToDatasetViewModel())
            });
		}

        /// <summary>
        /// Gets the dataset by its identifier.
        /// </summary>
        /// <param name="id">The dataset identifier.</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The requested dataset.</returns>
        [HttpGet("{id:guid}")]
        [SwaggerOperation(OperationId = "Dataset_GetById")]
        [Produces(contentType: "application/json")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(DatasetViewModel), Description = "Returns the dataset")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, Description = "Dataset not found")]
		[ResourceNotFoundExceptionFilter]
        public async Task <IActionResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var dataset = await this.DatasetStorageService.GetByIdAsync(id, this.User, cancellationToken).ConfigureAwait(false);
			if (dataset == null)
            {
                return this.NotFound();
            }

            bool isCurrentUserDatasetOwner = CurrentUserService.IsCurrentUserDatasetOwner(dataset, User);
            return this.Ok(dataset.ToDatasetViewModel(d =>
            {
                d.IsCurrentUserOwner = isCurrentUserDatasetOwner;
            }));
        }

        /// <summary>
        /// Gets the download URI for the complete dataset
        /// </summary>
        /// <param name="id">The dataset identifier.</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The URI.</returns>
		[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("{id:guid}/url")]
        [SwaggerOperation(OperationId = "Dataset_GetDownloadUrl")]
        [Produces(contentType: "application/json")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(Uri), Description = "Returns the download URI")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, Description = "Dataset not found")]
        [ResourceNotFoundExceptionFilter]
        public async Task<IActionResult> GetDownloadUrl([FromRoute] Guid id, CancellationToken cancellationToken)
		{
            var response = await this.DatasetStorageService.GetDownloadUriAsync(id, cancellationToken).ConfigureAwait(false);
			if (response == null)
            {
                return this.NotFound();
            }

            return this.Ok(response);
        }


        /// <summary>
        /// Gets the zip url for the dataset.
        /// </summary>
        /// <param name="datasetId">The dataset identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The zip file url</returns>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("{datasetId:guid}/zip", Name = "GetZipUrl")]
        [SwaggerOperation(OperationId = "Files_GetZipUrl")]
        [Produces(contentType: "application/json")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(Uri), Description = "Returns the download URI for the zip file")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, Description = "Zip not found")]
        [ResourceNotFoundExceptionFilter]
        public async Task<IActionResult> GetZipUrl([FromRoute] Guid datasetId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var result = await this.DatasetStorageService.GetZipUriAsync(datasetId, cancellationToken).ConfigureAwait(false);
            if (result == null)
            {
                return this.NotFound();
            }

            return this.Ok(result);
        }

        /// <summary>
        /// Gets the gzip for the dataset.
        /// </summary>
        /// <param name="datasetId">The dataset identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The gzip file url</returns>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("{datasetId:guid}/gzip", Name = "GetGzipUrl")]
        [SwaggerOperation(OperationId = "Files_GetGzipUrl")]
        [Produces(contentType: "application/json")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(Uri), Description = "Returns the download URI for the gzip file")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, Description = "Gzip not found")]
        [ResourceNotFoundExceptionFilter]
        public async Task<IActionResult> GetGzipUrl([FromRoute] Guid datasetId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var result = await this.DatasetStorageService.GetGzipUriAsync(datasetId, cancellationToken).ConfigureAwait(false);
            if (result == null)
            {
                return this.NotFound();
            }

            return this.Ok(result);
        }

        /// <summary>
        /// Searches the dataset using the specified criteria.
        /// </summary>
        /// <param name="criteria">The criteria.</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>Dataset search results.</returns>
        [HttpPost("search")]
		[SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(PagedResult<DatasetViewModel>), Description = "Returns datasets which match the specified criteria")]
		[SwaggerResponse((int)HttpStatusCode.BadRequest, Type = typeof(Dictionary<string, string[]>), Description = "The request was invalid, improperly formatted, or contained bad data")]
		[SwaggerOperation(OperationId = "Datasets_Search")]
		[Consumes(contentType: "application/json")]
		[Produces(contentType: "application/json")]
		[ValidateModel]
		public async Task<IActionResult> Search([FromBody]DatasetSearch criteria, CancellationToken cancellationToken)
		{
            criteria.Page = Math.Max(0, criteria.Page - 1);
            var response = await this.DataSearchService.SearchAsync(criteria, cancellationToken)
				.ConfigureAwait(false);
            if (response == null)
            {
                return this.NoContent();
            }

            return this.Ok(new PagedResult<DatasetViewModel>
            {
                RecordCount = response.RecordCount,
                PageCount = response.PageCount,
                Value = response.Value.Select(d => d.ToDatasetViewModel()).ToList()
            });
        }
        
        /// <summary>
        /// Searches for a specific dataset name.
        /// </summary>
        /// <param name="name">The dataset name to search for.</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>Dataset search results.</returns>
        [HttpGet("name-search")]
		[SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(ICollection<DatasetNameSearchResult>), Description = "Returns datasets which match the name")]
		[SwaggerResponse((int)HttpStatusCode.BadRequest, Type = typeof(Dictionary<string, string[]>), Description = "The request was invalid, improperly formatted, or contained bad data")]
		[SwaggerOperation(OperationId = "Datasets_NameSearch")]
		[Produces(contentType: "application/json")]
		public async Task<IActionResult> NameSearch([FromQuery]string name, CancellationToken cancellationToken)
        {
            var criteria = new DatasetSearch
            {
                Page = 0,
                SortOrder = SortOrder.Name,
                Terms = name,
            };
            var response = await this.DataSearchService.SearchAsync(criteria, cancellationToken).ConfigureAwait(false);
            if (response == null)
            {
                return this.NoContent();
            }

            var results = response.Value
                .Select(ds => new DatasetNameSearchResult
                {
                    Id = ds.Id,
                    Name = ds.Name,
                })
                .ToList();

            return this.Ok(results);
        }


        /// <summary>
        /// Gets the dataset by its identifier.
        /// </summary>
        /// <param name="id">The dataset identifier.</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The requested dataset.</returns>
        [HttpGet("featured/{quantity}")]
        [SwaggerOperation(OperationId = "Dataset_GetFeaturedDatasets")]
        [Produces(contentType: "application/json")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(DatasetViewModel), Description = "Returns the featured datasets")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, Description = "No Featured Datasets found")]
        [ResourceNotFoundExceptionFilter]
        public async Task<IActionResult> GetFeaturedDatasets([FromRoute] int quantity, CancellationToken cancellationToken)
        {
            var response = await this.DataSearchService.GetFeaturedDatasetsAsync(quantity, cancellationToken).ConfigureAwait(false);
            
            if (response == null)
            {
                return this.NoContent();
            }

            return this.Ok(response.Select(d => d.ToDatasetViewModel()).ToList());
        }
    }
}
