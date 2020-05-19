using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;

namespace Msr.Odr.Api.Configuration
{
    /// <summary>
    /// Configuration for Application Insights telemetry.
    /// </summary>
    /// <seealso cref="Microsoft.ApplicationInsights.Extensibility.ITelemetryInitializer" />
    public class TelemetryInitializer : ITelemetryInitializer
    {
		/// <summary>
        /// Initializes the telemetry client
        /// </summary>
        /// <param name="telemetry">The telemetry instance</param>
        public void Initialize(ITelemetry telemetry)
        {
            telemetry.Context.GlobalProperties["tags"] = typeof(Startup).Namespace;
            telemetry.Context.Component.Version = this.GetType().GetTypeInfo().Assembly.GetName().Version.ToString();
        }
    }
}
