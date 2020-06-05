// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using FluentValidation;

namespace Msr.Odr.Services.Configuration
{
    public class SearchConfigurationValidator : AbstractValidator<SearchConfiguration>
    {
        public SearchConfigurationValidator()
        {
            RuleFor(options => options.Account).NotEmpty();
            RuleFor(options => options.Key).NotEmpty();
            RuleFor(options => options.DatasetIndex).NotEmpty();
            RuleFor(options => options.FileIndex).NotEmpty();
            RuleFor(options => options.NominationsIndex).NotEmpty();
        }
    }
}
