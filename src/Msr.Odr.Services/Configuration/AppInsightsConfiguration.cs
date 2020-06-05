// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using FluentValidation;

namespace Msr.Odr.Services.Configuration
{
    // filter(timestamp gt now() sub duration'P30D' and startswith(pageView/url, '/datasets/'))/groupBy(pageView/url)/aggregate($count as countForUrl)

    /// <summary>
    /// The AppInsights API options
    /// </summary>
    public class AppInsightsConfiguration
    {
        /// <summary>
        /// Gets or sets the ApplicationId for AppInsights.
        /// </summary>
        public string ApplicationId { get; set; }

        /// <summary>
        /// Gets or sets the Azure AppInsights API Key.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Gets the URI for accessing the AppInsights API.
        /// </summary>
        public Uri Uri => new Uri($"https://api.applicationinsights.io/v1/apps/{ApplicationId}");

        public static void Validate(AppInsightsConfiguration configuration)
        {
            var validator = new AppInsightsConfigurationValidator();
            validator.ValidateAndThrow(configuration);
        }
    }
}
