// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Msr.Odr.Services;
using Msr.Odr.Model;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.Annotations;
using Msr.Odr.WebApi.Services;
using Msr.Odr.WebApi.ViewModels;

namespace Msr.Odr.Api.Controllers
{
    /// <summary>
    /// Provides access to the datasets.
    /// </summary>
    [Route(RouteConstants.UserDataControllerRoute)]
    public class UserDataController : Controller
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DatasetsController" /> class.
        /// </summary>
        /// <param name="index">The search indexes.</param>
        /// <param name="storage">The storage repository.</param>
        public UserDataController(UserDataStorageService storage, CurrentUserService currentUserService)
        {
            this.Storage = storage;
            this.CurrentUserService = currentUserService;
        }

        private UserDataStorageService Storage { get; }
        private CurrentUserService CurrentUserService { get; }

        /// <summary>
        /// Gets the current user's details (if applicable).
        /// </summary>
        /// <returns>The current user details.</returns>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [AllowAnonymous]
        [HttpGet("current")]
        [SwaggerOperation(OperationId = "User_CurrentUser")]
        [Produces(contentType: "application/json", Type = typeof(CurrentUserDetails))]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(Uri), Description = "Accepted the license associated with the dataset")]
        public async Task<IActionResult> GetCurrentUserDetails(CancellationToken cancellationToken)
        {
            var currentUser = await CurrentUserService.GetCurrentUserDetails(User, cancellationToken);
            return Ok(currentUser);
        }

        /// <summary>
        /// Accepts the specified license on the specified dataset
        /// </summary>
        /// <param name="id">The dataset identifier.</param>
        /// <param name="reason">The reason the license is accepted.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The results of the request</returns>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("accept-license/{id:guid}")]
        [SwaggerOperation(OperationId = "User_AcceptLicense")]
        [Consumes(contentType: "application/json")]
        [Produces(contentType: "application/json")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(Uri), Description = "Accepted the license associated with the dataset")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, Description = "Dataset not found")]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized, Description = "The user is not properly authenticated")]
        public async Task<IActionResult> AcceptLicense([FromRoute] Guid id, [FromBody] AcceptLicense acceptLicense, CancellationToken cancellationToken)
        {
            var currentLicenseId = await this.Storage.AcceptLicenseAsync(id, this.User, acceptLicense.Reason, cancellationToken).ConfigureAwait(false);
            if (currentLicenseId == null)
            {
                return this.NotFound();
            }

            return this.Ok(currentLicenseId);
        }

        /// <summary>
        /// Gets the license status for the specified dataset
        /// </summary>
        /// <param name="datasetId">The dataset identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The results of the request</returns>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("get-license/{datasetId:guid}")]
        [SwaggerOperation(OperationId = "User_GetLicense")]
        [Produces(contentType: "application/json")]
        [SwaggerResponse((int)HttpStatusCode.OK, Description = "Gets the license acceptance status for the dataset")]
        public async Task<IActionResult> GetLicense([FromRoute] Guid datasetId, CancellationToken cancellationToken)
        {
            var isLicenseAccepted = await this.Storage.GetLicenseStatusAsync(datasetId, this.User, cancellationToken).ConfigureAwait(false);
            return Ok(isLicenseAccepted);
        }
    }
}