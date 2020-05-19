using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Msr.Odr.Model;
using Msr.Odr.Services;
using Msr.Odr.WebAdminPortal.Users;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Msr.Odr.WebAdminPortal.Controllers
{
    [Route("api/datasets")]
    [Authorize(Policy = PolicyNames.MustBeInAdminGroup)]
    public class DatasetsController : Controller
    {
        public DatasetsController(
            DatasetSearchService datasetSearch,
            DatasetStorageService datasetStorage,
            ValidationService validationService)
        {
            ValidationService = validationService;
            DatasetSearch = datasetSearch;
            DatasetStorage = datasetStorage;
        }

        private DatasetSearchService DatasetSearch { get; }
        private ValidationService ValidationService { get; }
        private DatasetStorageService DatasetStorage { get; }

        [HttpGet("")]
        public async Task<IActionResult> GetDatasetList([FromQuery] int page, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var result = await DatasetSearch.GetAsync(Math.Max(0, page - 1), cancellationToken).ConfigureAwait(false);
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var dataset = await DatasetStorage.GetByIdAsync(id, User, cancellationToken).ConfigureAwait(false);
            if (dataset == null)
            {
                return NotFound();
            }

            return Ok(dataset);
        }

        /// <summary>
        /// Updates a dataset.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="dataset"></param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The dataset id.</returns>
        [HttpPut("{id:guid}")]
        [SwaggerOperation(OperationId = "Datasets_Update")]
        [Consumes(contentType: "application/json")]
        [Produces(contentType: "application/json")]
        public async Task<IActionResult> Update(
            [FromRoute] Guid id,
            [FromBody] Dataset dataset,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (id != dataset.Id)
            {
                throw new InvalidOperationException("Dataset id is not valid.");
            }

            var validationResult = ValidationService.IsDatasetValidForUpdate(dataset);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult);
            }

            var datasetId = await DatasetStorage.UpdateDatasetAsync(this.User, dataset, cancellationToken).ConfigureAwait(false);
            if (datasetId.HasValue)
            {
                return this.Ok(datasetId);
            }

            return this.NotFound();
        }


        /// <summary>
        /// Deletes a dataset.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The dataset id.</returns>
        [HttpDelete("{id:guid}")]
        [SwaggerOperation(OperationId = "Datasets_Delete")]
        [Consumes(contentType: "application/json")]
        [Produces(contentType: "application/json")]
        public async Task<IActionResult> Delete(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (id == default(Guid))
            {
                throw new InvalidOperationException("Dataset id is not valid.");
            }
            
            var success = await DatasetStorage.DeleteDatasetAsync(this.User, id, cancellationToken).ConfigureAwait(false);
            if (success)
            {
                return this.Ok();
            }

            return this.NotFound();
        }
    }
}
