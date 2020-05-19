using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;
using FluentValidation;

namespace Msr.Odr.Services.Configuration
{
    public class StorageConfiguration
    {
        public StorageConfiguration()
        {
            Accounts = new StorageAccountsConfiguration();
        }

        public StorageAccountsConfiguration Accounts { get; }

        public static void Validate(StorageConfiguration configuration)
        {
            var validator = new StorageConfigurationValidator();
            validator.ValidateAndThrow(configuration);
        }
    }
}
