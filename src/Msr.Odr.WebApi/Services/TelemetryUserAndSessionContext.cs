using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace Msr.Odr.Api.Services
{
    /// <summary>
    /// Set user/session context from HTTP headers passed from client
    /// </summary>
    /// <remarks>
    /// The default telemetry handlers pick up the context information from
    /// cookies, but the Web application runs on a different endpoint than
    /// the Web API application, so HTTP headers have to be sent instead.
    /// </remarks>
    public class TelemetryUserAndSessionContext : ITelemetryInitializer
    {
        private const string UserHttpHeaderName = "ai_user";
        private const string SessionHttpHeaderName = "ai_session";

        private IHttpContextAccessor HttpContextAccessor { get; }

        public TelemetryUserAndSessionContext(IHttpContextAccessor httpContextAccessor)
        {
            HttpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }


        public void Initialize(ITelemetry telemetry)
        {
            var context = HttpContextAccessor.HttpContext;
            if (context != null)
            {
                lock (context)
                {
                    var request = context.Features.Get<RequestTelemetry>();
                    if (request != null)
                    {
                        OnInitializeTelemetry(context, request, telemetry);
                    }
                }
                // System.Diagnostics.Debug.WriteLine($"USER/SESSION: {telemetry.Context.User.Id}/{telemetry.Context.Session.Id}");
            }
        }

        private void OnInitializeTelemetry(
            HttpContext context,
            RequestTelemetry requestTelemetry,
            ITelemetry telemetry)
        {
            SetUser(context, requestTelemetry, telemetry);
            SetSession(context, requestTelemetry, telemetry);
        }

        private void SetUser(
            HttpContext context,
            RequestTelemetry requestTelemetry,
            ITelemetry telemetry)
        {
            if (string.IsNullOrEmpty(telemetry.Context.User.Id))
            {
                if (string.IsNullOrEmpty(requestTelemetry.Context.User.Id))
                {
                    requestTelemetry.Context.User.Id = GetHeaderValue(context, UserHttpHeaderName);
                }

                if (!string.IsNullOrEmpty(requestTelemetry.Context.User.Id))
                {
                    telemetry.Context.User.Id = requestTelemetry.Context.User.Id;
                }
            }
        }

        private void SetSession(
            HttpContext context,
            RequestTelemetry requestTelemetry,
            ITelemetry telemetry)
        {
            if (string.IsNullOrEmpty(telemetry.Context.Session.Id))
            {
                if (string.IsNullOrEmpty(requestTelemetry.Context.Session.Id))
                {
                    requestTelemetry.Context.Session.Id = GetHeaderValue(context, SessionHttpHeaderName);
                }

                if (!string.IsNullOrEmpty(requestTelemetry.Context.Session.Id))
                {
                    telemetry.Context.Session.Id = requestTelemetry.Context.Session.Id;
                }
            }
        }

        private string GetHeaderValue(HttpContext context, string headerName)
        {
            var headerValue = context.Request.Headers.TryGetValue(headerName, out var values) ? values.First() : null;
            return string.IsNullOrWhiteSpace(headerValue) ? null : headerValue.Split('|').First();
        }
    }
}
