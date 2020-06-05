// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using FluentValidation;

namespace Msr.Odr.Model.Validators
{
    public class DatasetNominationApprovalValidator : DatasetNominationUpdateValidator
    {
        public DatasetNominationApprovalValidator()
        {
            // runs all of the standard rules in the UpdateValidator 
            //  and then these additional rules for Approval of a Nomination

            RuleFor(n => n.Published)
                .NotEmpty()
                .WithMessage("Published Date is required.")
                .WithErrorCode("required");

            RuleFor(n => n.Version)
                .NotEmpty()
                .WithMessage("Version is required.")
                .WithErrorCode("required");

            RuleFor(n => n.Domain)
                .NotEmpty()
                .WithMessage("Domain is required.")
                .WithErrorCode("required");

            RuleFor(n => n.LicenseId)
                .NotEmpty()
                .WithMessage("License is required.")
                .WithErrorCode("required")
                .OverridePropertyName("License");

            RuleFor(n => n.LicenseName)
                .NotEmpty()
                .WithMessage("License Name is required.")
                .WithErrorCode("required")
                .OverridePropertyName("License");
        }
    }
}
