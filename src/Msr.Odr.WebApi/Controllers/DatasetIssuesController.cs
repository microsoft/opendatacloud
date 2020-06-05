// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

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
    /// Provides access to the datasets.
    /// </summary>
    [Route(RouteConstants.DatasetIssuesControllerRoute)]
    public class DatasetIssuesController : Controller
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DatasetIssuesController" /> class.
        /// </summary>
        /// <param name="index">The search indexes.</param>
        /// <param name="storage">The storage repository.</param>
        public DatasetIssuesController(UserDataStorageService storage)
        {
            this.Storage = storage;
        }

        /// <summary>
        /// Gets the user data storage repository.
        /// </summary>
        /// <value>The user data storage repository.</value>
        private UserDataStorageService Storage
        {
            get;
        }

        /// <summary>
        /// Submits a dataset issue.
        /// </summary>
        /// <param name="id">The dataset identifier.</param>
        /// <param name="reason">The reason the license is accepted.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The results of the request</returns>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [SwaggerOperation(OperationId = "DatasetIssues")]
        [Consumes(contentType: "application/json")]
        [Produces(contentType: "application/json")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(bool), Description = "Dataset issue was accepted.")]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized, Description = "The user is not properly authenticated")]
        [ValidateModel]
        public async Task<IActionResult> SubmitNomination([FromBody] DatasetIssue issue, CancellationToken cancellationToken)
        {
            await this.Storage.SubmitDatasetIssue(this.User, issue, cancellationToken);
            return this.Ok(true);
        }
	}
}
