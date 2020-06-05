// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Msr.Odr.Model;

namespace Msr.Odr.Services.Test.Validation
{
    public static class ValidationTestsHelpers
    {
        public static DatasetNomination GenerateNominationWithAllMaxLengthFieldsExceeded()
        {
            var nomination = new DatasetNomination
            {
                Name = new string('*', Constraints.LongName + 1),
                Description = new string('*', Constraints.MedDescription + 1),
                DatasetUrl = new string('*', Constraints.UrlLength + 1),
                ProjectUrl = new string('*', Constraints.UrlLength + 1),
                Version = new string('*', Constraints.VersionLength + 1),
                Domain = new string('*', Constraints.ShortName + 1),
                ContactName = new string('*', Constraints.ContactNameLength + 1),
                ContactInfo = new string('*', Constraints.ContactInfoLength + 1)
            };
            return nomination;
        }
    }
}
