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
    [Route(RouteConstants.FeedbackControllerRoute)]
    public class FeedbackController : Controller
    {
        public FeedbackController(UserDataStorageService storage)
        {
            Storage = storage;
        }

        private UserDataStorageService Storage { get; }

        /// <summary>
        /// Submits a feedback (general issue).
        /// </summary>
        /// <param name="id">The dataset identifier.</param>
        /// <param name="reason">The reason the license is accepted.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The results of the request</returns>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [SwaggerOperation(OperationId = "Feedback_Add")]
        [Consumes(contentType: "application/json")]
        [Produces(contentType: "application/json")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(bool), Description = "Issue was accepted.")]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized, Description = "The user is not properly authenticated")]
        [ValidateModel]
        public async Task<IActionResult> SubmitNomination([FromBody] GeneralIssue issue, CancellationToken cancellationToken)
        {
            await Storage.SubmitGeneralIssue(User, issue, cancellationToken);
            return Ok(true);
        }
	}
}
