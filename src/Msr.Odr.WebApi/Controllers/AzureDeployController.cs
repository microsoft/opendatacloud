// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Msr.Odr.Api.Services;
using Msr.Odr.Model;
using Msr.Odr.Services;
using Msr.Odr.Model.Configuration;
using Msr.Odr.Services.Configuration;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.Annotations;

namespace Msr.Odr.Api.Controllers
{
    /// <summary>
    /// Provides access to data for deploying datasets to Azure.
    /// </summary>
    [Route(RouteConstants.AzureDeployControllerRoute)]
    public class AzureDeployController : Controller
    {
        public AzureDeployController(
            UserDataStorageService userStorage,
            DatasetStorageService datasetStorage,
            IOptions<WebServerConfiguration> webServerOptions)
        {
            this.UserStorage = userStorage;
            this.DatasetStorage = datasetStorage;
            this.WebServerConfiguration = webServerOptions.Value;
        }

        private UserDataStorageService UserStorage { get; }
        private DatasetStorageService DatasetStorage { get; }
        private WebServerConfiguration WebServerConfiguration { get; }

        /// <summary>
        /// Accepts the specified license on the specified dataset
        /// </summary>
        /// <param name="id">The dataset identifier.</param>
        /// <param name="reason">The reason the license is accepted.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The results of the request</returns>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("create")]
        [SwaggerOperation(OperationId = "AzureDeploy_Create")]
        [Consumes(contentType: "application/json")]
        [Produces(contentType: "application/json")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(Uri), Description = "Deployment was created.")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, Description = "Dataset not found")]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized, Description = "The user is not properly authenticated")]
        public async Task<IActionResult> CreateDeployment([FromBody] DeploymentCreation deployment, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var storageUri = await this.DatasetStorage.GetDownloadUriAsync(deployment.DatasetId, cancellationToken).ConfigureAwait(false);
            if(storageUri == null)
            {
                return this.NotFound();
            }

            var deploymentId = await this.UserStorage.CreateDeployment(deployment, storageUri, User, cancellationToken).ConfigureAwait(false);
            if (deploymentId == null)
            {
                return this.NotFound();
            }

            var templateUrl = $"{WebServerConfiguration.URL}azure-deploy/{deployment.DatasetId}/{deploymentId}/azuredeploy.json";
            var importUrl = WebServerConfiguration.AzureImportURL;
            return Json($"{importUrl}{Uri.EscapeDataString(templateUrl)}");
        }

        /// <summary>
        /// Gets the files associated with the specified dataset.
        /// </summary>
        /// <param name="datasetId">The identifier for the dataset.</param>
        /// <param name="deploymentId">The identifier for the deployment.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The collection of files in the specified page</returns>
        [HttpGet("assets/{name}")]
        [SwaggerOperation(OperationId = "Azdeploy_GetAsset")]
        [SwaggerResponse((int)HttpStatusCode.OK, Description = "Returns the deployment asset.")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, Description = "Deployment asset not found")]
        public async Task<IActionResult> GetAsset(
            [FromRoute] string name,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var asset = await UserStorage.GenerateDeploymentAsset(name, cancellationToken);
            if(asset == null)
            {
                return NotFound();
            }
            return Content(asset.Content, asset.MimeType);
        }

        /// <summary>
        /// Gets the files associated with the specified dataset.
        /// </summary>
        /// <param name="datasetId">The identifier for the dataset.</param>
        /// <param name="deploymentId">The identifier for the deployment.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The collection of files in the specified page</returns>
        [HttpGet("{datasetId:guid}/{deploymentId:guid}/azuredeploy.json")]
        [SwaggerOperation(OperationId = "Azdeploy_GetTemplate")]
        [Produces(contentType: "application/json")]
        [SwaggerResponse((int)HttpStatusCode.OK, Description = "Returns the deployment ARM template.")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, Description = "Deployment instance not found")]
        public async Task<IActionResult> GetTemplate(
            [FromRoute] Guid datasetId,
            [FromRoute] Guid deploymentId,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var content = await UserStorage.GenerateDeploymentTemplate(datasetId, deploymentId, cancellationToken);
            if (content == null)
            {
                return NotFound();
            }
            return Content(content, "application/json");
        }
    }
}
