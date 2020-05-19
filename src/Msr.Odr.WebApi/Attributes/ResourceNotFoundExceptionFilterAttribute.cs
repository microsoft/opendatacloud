using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Azure.Documents;
using Microsoft.Extensions.Logging;

namespace Msr.Odr.Api.Attributes
{
    /// <summary>
    /// Handles resource not found exceptions from Cosmos Db gracefully.
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.Filters.ExceptionFilterAttribute" />
    public class ResourceNotFoundExceptionFilterAttribute : ExceptionFilterAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceNotFoundExceptionFilterAttribute" /> class.
        /// </summary>
        public ResourceNotFoundExceptionFilterAttribute()
        {
        }

        /// <summary>
        /// Called when handling an exception asynchronouslu.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>The task result.</returns>
        public override Task OnExceptionAsync(ExceptionContext context)
        {
            if (context.Exception.GetType().Name == "NotFoundException")
            {
                var logger = (ILoggerFactory)context.HttpContext.RequestServices.GetService(typeof(ILoggerFactory));
                var log = logger?.CreateLogger<ResourceNotFoundExceptionFilterAttribute>();
                if (log != null)
                {
                    log.LogError(context.Exception.Message);
                }

                new TelemetryClient().TrackException(context.Exception);

                context.Exception = null;
                context.Result = new NotFoundResult();
            }

            return base.OnExceptionAsync(context);
        }
    }
}
