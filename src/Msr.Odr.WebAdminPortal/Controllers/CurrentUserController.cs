// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Msr.Odr.Services;
using Msr.Odr.WebAdminPortal.Users;

namespace Msr.Odr.WebAdminPortal.Controllers
{
    [Route("api")]
    [Authorize(Policy = PolicyNames.MustBeInAdminGroup)]
    public class CurrentUserController : Controller
    {
        [HttpGet("current-user")]
        public IActionResult GetCurrentUser(CancellationToken cancellationToken)
		{
		    cancellationToken.ThrowIfCancellationRequested();
            return Json(new CurrentUserInfo());
        }

        private class CurrentUserInfo
        {
            public bool Authorized { get; } = true;
        }
    }
}
