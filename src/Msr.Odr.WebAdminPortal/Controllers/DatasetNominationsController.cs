// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Msr.Odr.Model;
using Msr.Odr.Model.UserData;
using Msr.Odr.Services;
using Msr.Odr.Services.Batch;
using Msr.Odr.WebAdminPortal.Users;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Msr.Odr.WebAdminPortal.Controllers
{
    [Route("api/dataset-nominations")]
    [Authorize(Policy = PolicyNames.MustBeInAdminGroup)]
    public class DatasetNominationsController : Controller
    {
        public DatasetNominationsController(
            UserDataStorageService userDataStorage,
            UserDataSearchService userDataSearch,
            ValidationService validationService,
            SasTokenService sasTokenService,
            ApplicationJobs appJobs)
        {
            UserDataStorage = userDataStorage;
            UserDataSearch = userDataSearch;
            ValidationService = validationService;
            SasTokenService = sasTokenService;
            AppJobs = appJobs;
        }

        private UserDataStorageService UserDataStorage { get; }
        private UserDataSearchService UserDataSearch { get; }
        private ValidationService ValidationService { get; }
        private SasTokenService SasTokenService { get; }
        private ApplicationJobs AppJobs { get; }

        [HttpGet("")]
        public async Task<IActionResult> GetNominationsList([FromQuery] int page, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var result = await UserDataSearch.GetNominations(page - 1, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Gets the nominated dataset by its identifier.
        /// </summary>
        /// <param name="id">The dataset identifier.</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The requested dataset.</returns>
        [HttpGet("{id:guid}")]
        [Produces(contentType: "application/json")]
        public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var result = await UserDataStorage.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
            if (result == null)
            {
                return this.NotFound();
            }

            return this.Ok(result);
        }

        /// <summary>
        /// Gets the nominated dataset by its identifier.
        /// </summary>
        /// <param name="id">The dataset identifier.</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The requested dataset.</returns>
        [HttpGet("{id:guid}/other-license-file")]
        [Produces("application/json")]
        public async Task<IActionResult> GetOtherLicenseFile([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var otherLicenseFile = await UserDataStorage.GetOtherLicenseFileAsync(id, cancellationToken);
            if (otherLicenseFile == null)
            {
                return NotFound();
            }

            return File(otherLicenseFile.Content, otherLicenseFile.ContentType, otherLicenseFile.FileName);
        }

        /// <summary>
        /// Rejects a nominated dataset.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="nomination"></param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The updated nomination.</returns>
        [HttpPut("{id:guid}/reject")]
        [SwaggerOperation(OperationId = "Nominations_Reject")]
        [Consumes(contentType: "application/json")]
        [Produces(contentType: "application/json")]
        public async Task<IActionResult> Reject(
            [FromRoute] Guid id,
            [FromBody] DatasetNomination nomination,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (id != nomination.Id)
            {
                throw new InvalidOperationException("Nomination id is not valid.");
            }

            // intentionally no validation for Rejecting a nomination

            bool found = await UserDataStorage.RejectNominationAsync(id, this.User, cancellationToken).ConfigureAwait(false);
            if (!found)
            {
                return this.NotFound();
            }

            return Ok();
        }

        /// <summary>
        /// approves a nominated dataset.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="nomination"></param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The updated nomination.</returns>
        [HttpPut("{id:guid}/approve")]
        [SwaggerOperation(OperationId = "Nominations_Approve")]
        [Consumes("application/json", "multipart/form-data")]
        [Produces("application/json")]
        public async Task<IActionResult> Approve(
            [FromRoute] Guid id,
            [FromForm] DatasetNomination nomination,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (id != nomination.Id)
            {
                throw new InvalidOperationException("Nomination id is not valid.");
            }

            var validationResult = ValidationService.IsDatasetNominationValidForApproval(nomination);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult);
            }

            var nominationId = await UserDataStorage.UpdateDatasetNominationAsync(this.User, nomination, cancellationToken).ConfigureAwait(false);
            if (nominationId.HasValue)
            {
                bool found = await UserDataStorage.ApproveNominationAsync(nomination.Id, this.User, cancellationToken).ConfigureAwait(false);
                if (found)
                {
                    return Ok();
                }
            }

            return NotFound();
        }

        /// <summary>
        /// Updates a nominated dataset.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="nomination"></param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>nomination id.</returns>
        [HttpPut("{id:guid}")]
        [SwaggerOperation(OperationId = "Nominations_Update")]
        [Consumes("application/json", "multipart/form-data")]
        [Produces("application/json")]
        public async Task<IActionResult> Update(
            [FromRoute] Guid id,
            [FromForm] DatasetNomination nomination,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (id != nomination.Id)
            {
                throw new InvalidOperationException("Nomination id is not valid.");
            }

            var validationResult = ValidationService.IsDatasetNominationValidForUpdate(nomination);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult);
            }

            var nominationId = await UserDataStorage.UpdateDatasetNominationAsync(this.User, nomination, cancellationToken).ConfigureAwait(false);
            if (nominationId.HasValue)
            {
                return this.Ok(nominationId);
            }

            return this.NotFound();
        }

        /// <summary>
        /// creates an Approved nomination.
        /// </summary>
        /// <param name="nomination"></param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The created nomination id.</returns>
        [HttpPost()]
        [SwaggerOperation(OperationId = "Nominations_Create")]
        [Consumes("application/json", "multipart/form-data")]
        [Produces("application/json")]
        public async Task<IActionResult> CreateApproved(
            [FromForm] DatasetNomination nomination,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var validationResult = ValidationService.IsDatasetNominationValidForApproval(nomination);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult);
            }

            nomination.NominationStatus = NominationStatus.Approved;

            var nominationId = await UserDataStorage.CreateDatasetNominationAsync(this.User, nomination, cancellationToken).ConfigureAwait(false);
            if (nominationId.HasValue)
            {
                return this.Ok(nominationId);
            }

            return this.NotFound();
        }

        /// <summary>
        /// Searches the dataset nominations using the specified criteria.
        /// </summary>
        /// <param name="criteria">The criteria.</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>Dataset nomination search results.</returns>
        [HttpPost("search")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(PagedResult<DatasetNomination>), Description = "Returns datasets which match the specified criteria")]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, Type = typeof(Dictionary<string, string[]>), Description = "The request was invalid, improperly formatted, or contained bad data")]
        [Consumes(contentType: "application/json")]
        [Produces(contentType: "application/json")]
        public async Task<IActionResult> Search([FromBody]NominationSearch criteria, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            criteria.Page = Math.Max(0, criteria.Page - 1);
            var response = await UserDataSearch.SearchNominations(criteria, cancellationToken).ConfigureAwait(false);
            return Ok(response);
        }

        /// <summary>
        /// Searches the dataset nominations using the specified criteria.
        /// </summary>
        /// <param name="criteria">The criteria.</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>Dataset nomination search results.</returns>
        [HttpGet("{id:guid}/storage")]
        public async Task<IActionResult> DatasetContainer([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var nomination = await UserDataStorage.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
            if (nomination == null)
            {
                return NotFound();
            }

            var storage = new DatasetStorage
            {
                Id = nomination.Id,
                AccountName = SasTokenService.DefaultDatasetStorageAccount(),
                DatasetName = nomination.Name,
            };
            await SasTokenService.FindUniqueDatasetContainerName(storage).ConfigureAwait(false);
            return Ok(storage);
        }

        /// <summary>
        /// Creates storage container for approved dataset.
        /// </summary>
        /// <param name="id">The nomination identifier.</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The nomination status.</returns>
        [HttpPost("{id:guid}/create-storage")]
        public async Task<IActionResult> CreateStorage(
            [FromRoute] Guid id,
            [FromBody] DatasetStorage storage,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (id != storage.Id)
            {
                throw new InvalidOperationException("Nomination id is not valid.");
            }

            var status = await UserDataStorage.CreateDatasetStorageAsync(storage, this.User, cancellationToken).ConfigureAwait(false);
            if (status == null)
            {
                return NotFound();
            }

            return Ok();
        }

        /// <summary>
        /// Imports dataset from storage account.
        /// </summary>
        /// <param name="id">The nomination identifier.</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns></returns>
        [HttpPost("{id:guid}/import-from-storage")]
        public async Task<IActionResult> ImportDatasetFromStorage(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var (found, name) = await UserDataStorage.UpdateNominationStatusAndReturnName(id, NominationStatus.Importing, User, cancellationToken);
            if (!found)
            {
                return NotFound();
            }

            await AppJobs.StartDatasetImportJob(id, name);

            return Ok();
        }

        //[HttpGet("refresh-html-license")]
        //public async Task<IActionResult> AddAttachmentToLicense(CancellationToken cancellationToken)
        //{
        //    var id = Guid.Parse("7230b4b1-912d-400e-be58-f84e0512985e");
        //    var licenseId = Guid.Parse("332854c6-95b4-4396-b126-5ec88da916f8");
            
        //    cancellationToken.ThrowIfCancellationRequested();
        //    var nomination = await UserDataStorage.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
        //    var licenseSelfLink = //this.LicenseService.GetLicenseSelfLink(licenseId, cancellationToken);
        //        "dbs/mrJtAA==/colls/mrJtAOth8+U=/docs/mrJtAOth8+V+TwYAAAAACA==/";

        //    await UserDataStorage.RefreshLicenseAttachment(nomination, licenseSelfLink);

        //    return Ok();
        //}
    }
}