using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Msr.Odr.WebAdminPortal.Services
{
    public static class AppExceptionHandler
    {
        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore,
        };

        public static void Configure(IApplicationBuilder options)
        {
            options.Run(
                async context =>
                {
                    var ex = context.Features.Get<IExceptionHandlerFeature>();
                    if (ex != null)
                    {
                        var code = HttpStatusCode.InternalServerError;
                        var result = JsonConvert.SerializeObject(ex, Formatting.None, Settings);
                        context.Response.ContentType = "application/json";
                        context.Response.StatusCode = (int)code;
                        await context.Response.WriteAsync(result).ConfigureAwait(false);
                    }
                });
        }
    }
}
