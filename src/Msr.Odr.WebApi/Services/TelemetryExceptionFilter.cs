// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace Msr.Odr.Api.Services
{
	/// <summary>
	/// Telemetry exception handling
	/// </summary>
	public class TelemetryExceptionFilter : IExceptionFilter
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="TelemetryExceptionFilter"/> class.
		/// </summary>
		/// <param name="logger">The logger.</param>
		/// <exception cref="ArgumentNullException">logger</exception>
		public TelemetryExceptionFilter(ILoggerFactory logger)
		{
			if (logger == null)
			{
				throw new ArgumentNullException(nameof(logger));
			}

			this.Logger = logger.CreateLogger("Exceptions");
		}

		/// <summary>
		/// Gets the logger.
		/// </summary>
		/// <value>The logger.</value>
		private ILogger Logger { get;  }

		/// <summary>
		/// Gets the telemetry client
		/// </summary>
		/// <value>The telemetry.</value>
		private TelemetryClient Telemetry { get; }  = new TelemetryClient();

		/// <summary>
		/// Called when an exception occurs
		/// </summary>
		/// <param name="context">The context.</param>
		public void OnException(ExceptionContext context)
		{
			if (context != null && context.Exception != null)
			{ 	
				this.Telemetry.TrackException(context.Exception);
				this.Logger.LogError(context.Exception.Message);
			}
		}
	}

}
