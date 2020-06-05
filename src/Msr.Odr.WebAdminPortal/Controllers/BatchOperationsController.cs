// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Msr.Odr.Services;
using Msr.Odr.Services.Batch;
using Msr.Odr.WebAdminPortal.Users;

namespace Msr.Odr.WebAdminPortal.Controllers
{
    [Route("api/batch")]
    [Authorize(Policy = PolicyNames.MustBeInAdminGroup)]
    public class BatchOperationsController : Controller
    {
        public ApplicationJobs ApplicationJobs { get; }

        public BatchOperationsController(ApplicationJobs applicationJobs)
        {
            ApplicationJobs = applicationJobs;
        }

        [HttpGet("operations")]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
		{
		    cancellationToken.ThrowIfCancellationRequested();
		    var batchOperations = await ApplicationJobs.GetLatestTasks(cancellationToken).ConfigureAwait(false);
            return Json(batchOperations);
        }

        [HttpGet("operations/{taskId}/output")]
        public async Task<IActionResult> GetTaskOutput([FromRoute] string taskId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var batchOutput = await ApplicationJobs.GetTaskOutput(taskId, cancellationToken).ConfigureAwait(false);
            return Json(batchOutput);
        }
	}
}
