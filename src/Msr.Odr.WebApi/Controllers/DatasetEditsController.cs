// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

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
using Msr.Odr.Services.Batch;

namespace Msr.Odr.Api.Controllers
{
    /// <summary>
    /// Provides access to the datasets edits by dataset owners.
    /// </summary>
    [Route(RouteConstants.DatasetEditsControllerRoute)]
    public class DatasetEditsController : Controller
    {
        private DatasetEditStorageService Storage { get; }
        private ValidationService ValidationService { get; }
        private ApplicationJobs AppJobs { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DatasetEditsController" /> class.
        /// </summary>
        /// <param name="storage">The dataset storage service.</param>
        /// <param name="currentUserService">The current user service.</param>
        public DatasetEditsController(
            DatasetEditStorageService storage,
            ValidationService validationService,
            ApplicationJobs appJobs)
        {
            this.Storage = storage;
            this.ValidationService = validationService;
            this.AppJobs = appJobs;
        }

        /// <summary>
        /// Gets the dataset edit by its identifier.
        /// </summary>
        /// <param name="id">The dataset identifier.</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The requested dataset edit.</returns>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("{id:guid}")]
        [SwaggerOperation(OperationId = "DatasetEdits_GetById")]
        [Produces(contentType: "application/json")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(DatasetEditViewModel), Description = "Returns the dataset details")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, Description = "Dataset not found")]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized, Description = "Access to dataset edit prohibited.")]
        [DatasetOwnerExceptionFilterAttribute]
        public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var datasetEdit = await Storage.GetDatasetEditById(id, User, token);
            if(datasetEdit == null)
            {
                return NotFound();
            }

            return Ok(datasetEdit.ToDatasetEditViewModel());
        }

        /// <summary>
        /// Gets the dataset edit by its identifier.
        /// </summary>
        /// <param name="id">The dataset identifier.</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The requested dataset edit.</returns>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPut("{id:guid}")]
        [SwaggerOperation(OperationId = "DatasetEdits_UpdateDataset")]
        [Consumes("application/json", "multipart/form-data")]
        [Produces(contentType: "application/json")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(DatasetEditViewModel), Description = "Returns the dataset details")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, Description = "Dataset not found")]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized, Description = "Access to dataset edit prohibited.")]
        [DatasetOwnerExceptionFilterAttribute]
        public async Task<IActionResult> UpdateDataset([FromRoute] Guid id, [FromForm] DatasetEditViewModel update, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if(id != update.Id)
            {
                return BadRequest();
            }

            var datasetEdit = update.ToDatasetEditStorageItem();

            var validationResult = ValidationService.IsDatasetEditValidForUpdate(datasetEdit);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult);
            }

            datasetEdit = await Storage.UpdateDataset(id, User, datasetEdit, token);
            return Ok(datasetEdit.ToDatasetEditViewModel());
        }

        /// <summary>
        /// Initiate changes to dataset content.
        /// </summary>
        /// <param name="id">The dataset identifier.</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>True</returns>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPut("{id:guid}/update-content")]
        [SwaggerOperation(OperationId = "DatasetEdits_ContentChanges")]
        [Produces(contentType: "application/json")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(DatasetEditViewModel), Description = "Returns the dataset details")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, Description = "Dataset not found")]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized, Description = "Access to dataset edit prohibited.")]
        [DatasetOwnerExceptionFilterAttribute]
        public async Task<IActionResult> InitiateDatasetContentChanges([FromRoute] Guid id, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            var datasetEdit = await Storage.InitiateDatasetContentEdit(id, User, token);
            return Ok(datasetEdit.ToDatasetEditViewModel());
        }

        /// <summary>
        /// Commit changes to dataset.
        /// </summary>
        /// <param name="id">The dataset identifier.</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>True</returns>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPut("{id:guid}/commit")]
        [SwaggerOperation(OperationId = "DatasetEdits_CommitChanges")]
        [Produces(contentType: "application/json")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(bool), Description = "Returns the dataset details")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, Description = "Dataset not found")]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized, Description = "Access to dataset edit prohibited.")]
        [DatasetOwnerExceptionFilterAttribute]
        public async Task<IActionResult> CommitChanges([FromRoute] Guid id, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            var result = await Storage.PublishUpdatedDataset(id, User, token);
            if(result.ShouldQueueBatchOperation)
            {
                await AppJobs.StartDatasetImportJob(result.DatasetId, result.DatasetName);
            }
            return Ok(result.IsPublished);
        }

        /// <summary>
        /// Cancel changes to dataset.
        /// </summary>
        /// <param name="id">The dataset identifier.</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>True</returns>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpDelete("{id:guid}")]
        [SwaggerOperation(OperationId = "DatasetEdits_CancelChanges")]
        [Produces(contentType: "application/json")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(bool), Description = "Returns the dataset details")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, Description = "Dataset not found")]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized, Description = "Access to dataset edit prohibited.")]
        [DatasetOwnerExceptionFilterAttribute]
        public async Task<IActionResult> CancelChanges([FromRoute] Guid id, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            var result = await Storage.CancelDatasetChanges(id, User, token);
            return Ok(result);
        }

        /// <summary>
        /// Gets a read-only SAS token for the original dataset content.
        /// </summary>
        /// <param name="id">The dataset identifier.</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The SAS token.</returns>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("{id:guid}/original-sas-token")]
        [SwaggerOperation(OperationId = "DatasetEdits_GetOriginalSasToken")]
        [Produces(contentType: "application/json")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(SasTokenViewModel), Description = "Returns the SAS token for read-only access to the original dataset contents.")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, Description = "Dataset not found")]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized, Description = "Access to dataset edit prohibited.")]
        [DatasetOwnerExceptionFilterAttribute]
        public async Task<IActionResult> GetOriginalContentSasToken([FromRoute] Guid id, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            return Ok(new SasTokenViewModel
            {
                Token = await Storage.GetReadOnlySasTokenForOriginalDatasetContent(id, User, token)
            });
        }

        /// <summary>
        /// Gets a read-write SAS token for the dataset content to be updated.
        /// </summary>
        /// <param name="id">The dataset identifier.</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The SAS token.</returns>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("{id:guid}/update-sas-token")]
        [SwaggerOperation(OperationId = "DatasetEdits_GetUpdateSasToken")]
        [Produces(contentType: "application/json")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(SasTokenViewModel), Description = "Returns the SAS token for read-write access to the dataset contents to be updated.")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, Description = "Dataset not found")]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized, Description = "Access to dataset edit prohibited.")]
        [DatasetOwnerExceptionFilterAttribute]
        public async Task<IActionResult> GetUpdatedContentSasToken([FromRoute] Guid id, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            return Ok(new SasTokenViewModel
            {
                Token = await Storage.GetReadWriteSasTokenForUpdatedDatasetContent(id, User, token)
            });
        }
    }
}
