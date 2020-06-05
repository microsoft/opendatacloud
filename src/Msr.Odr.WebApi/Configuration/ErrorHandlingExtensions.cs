// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Msr.Odr.Api.Configuration
{
    /// <summary>
    /// Extensions for error handling configuration
    /// </summary>
    public static class ErrorHandlingExtensions
    {
        /// <summary>
        /// Configures the error handling.
        /// </summary>
        /// <param name="app">The application builder instance.</param>
        /// <param name="loggerFactory">The logging factory</param>
        /// <returns>The application builder instance</returns>
        public static IApplicationBuilder ConfigureErrorHandling(this IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
			// Configure the HTTP request to be buffered to allow multiple reads of the stream.
            app.Use(async (context, next) => {
                var request = context.Request;
                var body = request.Body;
                if (!body.CanSeek)
                {
                    var fileStream = new Microsoft.AspNetCore.WebUtilities.FileBufferingReadStream(
                        body,
                        1024 * 30,
                        null,
                         Environment.GetEnvironmentVariable("ASPNETCORE_TEMP") ?? Path.GetTempPath());
                    request.Body = fileStream;
                    request.HttpContext.Response.RegisterForDispose(fileStream);
                }

                await next();
            });

            app.UseExceptionHandler(options =>
            {
                options.Run(
                async context =>
                {
                    var ex = context.Features.Get<IExceptionHandlerFeature>();

                    new TelemetryClient().TrackException(ex.Error);
                    loggerFactory.CreateLogger("Exception").LogError("Uncaught Exception '{0}': {1}", ex.Error.GetType().Name, ex.Error.Message);

                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    context.Response.ContentType = "application/json";
                  
                    var form = new Dictionary<string, string>();
                    string body = null;
                    try
                    {
                        form = context.Request.Form.ToDictionary(x => x.Key, x => x.Value.ToString());
                    }
                    catch (Exception)
                    {
						// This wasn't a form post, so we may have an exception we need to ignore.
                    }

                    try
                    {
						// Read the content of the body
                        if (context.Request.Body.CanSeek && context.Request.Body.CanRead)
                        {
                            var stream = context.Request.Body;
                            stream.Position = 0;
                            using (StreamReader reader = new StreamReader(stream, System.Text.Encoding.UTF8, true, 16 * 1024, leaveOpen: true))
                            {
                                body = reader.ReadToEnd();
                            }

                            stream.Position = 0;
                        }
                    }
                    catch (Exception)
                    {
						// If the body cannot be read, we won't return that data.
                    }

                    if (ex != null)
                    {
						// Serialize the error details to the client
                        var json = JsonConvert.SerializeObject(new
                        {
							Code = "UncaughtException",
                            Exception = ex.GetType().FullName,
                            Message = ex.Error.Message,
                            Stack = ex.Error.StackTrace,
                            Query = context.Request.QueryString.Value,
                            Form = form,
                            Body = body
                        });
                        await context.Response.WriteAsync(json).ConfigureAwait(false);
                    }
                });
            }
            );

            return app;
        }
    }
}
