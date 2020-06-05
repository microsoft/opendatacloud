// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Azure.Documents;
using Microsoft.Extensions.Logging;
using Msr.Odr.Services;

namespace Msr.Odr.Api.Attributes
{
    /// <summary>
    /// Handles dataset owner exceptions from services gracefully.
    /// </summary>
    /// <seealso cref="ExceptionFilterAttribute" />
    public class DatasetOwnerExceptionFilterAttribute : ExceptionFilterAttribute
    {
        public override Task OnExceptionAsync(ExceptionContext context)
        {
            if (context.Exception is DatasetOwnerException)
            {
                var logger = (ILoggerFactory)context.HttpContext.RequestServices.GetService(typeof(ILoggerFactory));
                var log = logger?.CreateLogger<DatasetOwnerExceptionFilterAttribute>();
                if (log != null)
                {
                    log.LogError(context.Exception.Message);
                }

                new TelemetryClient().TrackException(context.Exception);

                context.Exception = null;
                context.Result = new ForbidResult();
            }

            return base.OnExceptionAsync(context);
        }
    }
}
