// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using FluentValidation;

namespace Msr.Odr.Services.Configuration
{
    /// <summary>
    /// Configuration information about the B2C Identity Provider
    /// </summary>
    public class IdentityProviderConfiguration
    {
        /// <summary>
        /// Id of Tenant.
        /// </summary>
        public string Tenant { get; set; }

        /// <summary>
        /// Id of Audience (also called "Client Id").
        /// </summary>
        public string Audience { get; set; }

        /// <summary>
        /// Name of policy.
        /// </summary>
        public string Policy { get; set; }

        public static void Validate(IdentityProviderConfiguration configuration)
        {
            var validator = new IdentityProviderConfigurationValidator();
            validator.ValidateAndThrow(configuration);
        }
    }
}
