// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using FluentValidation.Results;
using Msr.Odr.Model;
using Msr.Odr.Model.UserData;
using Msr.Odr.Model.Validators;

namespace Msr.Odr.Services
{
    public class ValidationService
    {
        public ValidationResult IsDatasetNominationValidForApproval(DatasetNomination nomination)
        {
            var validator = new DatasetNominationApprovalValidator();
            var result = validator.Validate(nomination);
            return result;
        }

        public ValidationResult IsDatasetNominationValidForUpdate(DatasetNomination nomination)
        {
            var validator = new DatasetNominationUpdateValidator();
            var result = validator.Validate(nomination);
            return result;
        }

        public ValidationResult IsDatasetNominationValidForCreate(DatasetNomination nomination)
        {
            var validator = new DatasetNominationUpdateValidator();
            var result = validator.Validate(nomination);
            return result;
        }

        public ValidationResult IsDatasetValidForUpdate(Dataset dataset)
        {
            var validator = new DatasetUpdateValidator();
            var result = validator.Validate(dataset);
            return result;
        }

        public ValidationResult IsDatasetEditValidForUpdate(DatasetEditStorageItem dataset)
        {
            var validator = new DatasetEditValidator();
            var result = validator.Validate(dataset);
            return result;
        }
    }
}
