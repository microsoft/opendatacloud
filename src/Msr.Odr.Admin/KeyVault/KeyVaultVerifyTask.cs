// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Msr.Odr.Admin.Commands.Options;

namespace Msr.Odr.Admin.KeyVault
{
    public class KeyVaultVerifyTask
    {
        public VaultOptions VaultOptions { get; }

        public KeyVaultVerifyTask(
            VaultOptions vaultOptions)
        {
            VaultOptions = vaultOptions;
        }

        public async Task<int> ExecuteAsync()
        {
            Console.WriteLine($"Verifying {VaultOptions.VaultUrl} Key Vault");

            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            var authCallback = new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback);
            var keyVaultClient = new KeyVaultClient(authCallback);

            var result = await keyVaultClient.GetSecretsAsync(VaultOptions.VaultUrl);
            while(true)
            {
                foreach(Microsoft.Azure.KeyVault.Models.SecretItem item in result)
                {
                    var secret = await keyVaultClient.GetSecretAsync(item.Id);
                    Console.WriteLine($"  - {item.Id}");
                    Console.WriteLine($"    {secret.Value}");
                }

                if(result.NextPageLink == null)
                {
                    break;
                }

                result = await keyVaultClient.GetSecretsNextAsync(result.NextPageLink);
            }

            return 0;
        }
    }
}
