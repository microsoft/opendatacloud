using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Msr.Odr.Api.Attributes;
using Msr.Odr.Model;
using Msr.Odr.Services;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Msr.Odr.Api.Controllers
{
    /// <summary>
    /// Provides access to the datasets.
    /// </summary>
    [Route(RouteConstants.DatasetNominationsControllerRoute)]
    public class DatasetNominationsController : Controller
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DatasetNominationsController" /> class.
        /// </summary>
        /// <param name="index">The search indexes.</param>
        /// <param name="storage">The storage repository.</param>
        public DatasetNominationsController(UserDataStorageService storage, ValidationService validationService)
        {
            this.Storage = storage;
            ValidationService = validationService;
        }

        /// <summary>
        /// Gets the user data storage repository.
        /// </summary>
        /// <value>The user data storage repository.</value>
        private UserDataStorageService Storage { get; }

        public ValidationService ValidationService { get; }

        /// <summary>
        /// Submits a dataset nomination.
        /// </summary>
        /// <param name="id">The dataset identifier.</param>
        /// <param name="reason">The reason the license is accepted.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The results of the request</returns>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [SwaggerOperation(OperationId = "DatasetNominations")]
        [Consumes("application/json", "multipart/form-data")]
        [Produces(contentType: "application/json")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(bool), Description = "Dataset nomination was accepted.")]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized, Description = "The user is not properly authenticated")]
        [ValidateModel]
        public async Task<IActionResult> CreateNomination(
            [FromForm] DatasetNomination nomination, 
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var validationResult = ValidationService.IsDatasetNominationValidForCreate(nomination);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult);
            }

            await this.Storage.CreateDatasetNominationAsync(this.User, nomination, cancellationToken);
            return this.Ok(true);
        }
	}
}
