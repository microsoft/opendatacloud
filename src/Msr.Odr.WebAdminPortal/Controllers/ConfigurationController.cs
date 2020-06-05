// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Msr.Odr.WebAdminPortal.Models;

namespace Msr.Odr.WebAdminPortal.Controllers
{
    [Route("api")]
    public class ConfigurationController : Controller
    {
        public IConfiguration Config { get; }

        public ConfigurationController(IConfiguration config)
        {
            this.Config = config;
        }

        [HttpGet("configuration")]
        public IActionResult GetConfiguration(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Json(new AppConfiguration(Config));
        }
    }
}
