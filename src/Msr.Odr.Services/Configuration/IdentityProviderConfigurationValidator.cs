// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using FluentValidation;

namespace Msr.Odr.Services.Configuration
{
    public class IdentityProviderConfigurationValidator : AbstractValidator<IdentityProviderConfiguration>
    {
        public IdentityProviderConfigurationValidator()
        {
            RuleFor(options => options.Tenant).NotEmpty();
            RuleFor(options => options.Audience).NotEmpty();
            RuleFor(options => options.Policy).NotEmpty();
        }
    }
}
