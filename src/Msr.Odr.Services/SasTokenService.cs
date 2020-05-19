using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Auth;
using Microsoft.Azure.Storage.Blob;
using Msr.Odr.Model;
using Msr.Odr.Model.Configuration;
using Msr.Odr.Services.Configuration;

namespace Msr.Odr.Services
{
    /// <summary>
    /// Service providing the ability to create SAS tokens
    /// </summary>
    public class SasTokenService
    {
        const string DatasetUpdateAccessPolicyName = "DatasetOwnerUpdatePolicy";
        const string NextChars = "23456789abcdefghijklmnopqrstuvwxyz";
        const int ContainerNameMinLength = 3;
        const int ContainerNameMaxLength = 63;

        /// <summary>
        /// Initializes a new instance of the <see cref="SasTokenService" /> class.
        /// </summary>
        /// <param name="options">The storage account settings.</param>
        public SasTokenService(IOptions<StorageConfiguration> options)
        {
            Accounts = options.Value.Accounts;
        }

        /// <summary>
        /// Gets the accounts.
        /// </summary>
        /// <value>The configuration.</value>
        private StorageAccountsConfiguration Accounts { get; }

        /// <summary>
        /// The default storage account name.
        /// </summary>
        /// <returns>The storage account name.</returns>
        public string DefaultDatasetStorageAccount()
        {
            return Accounts.DefaultStorageAccount;
        }

        /// <summary>
        /// Get the media link Uri for the storage container.
        /// </summary>
        /// <param name="accountName">The name of the account.</param>
        /// <param name="containerName">The name of the container.</param>
        /// <returns>The media link.</returns>
        public string GetContainerMediaLink(string accountName, string containerName)
        {
            if (accountName is null)
            {
                throw new ArgumentNullException(nameof(accountName));
            }

            if (containerName is null)
            {
                throw new ArgumentNullException(nameof(containerName));
            }

            var blobClient = CreateBlobClient(accountName);
            var container = blobClient.GetContainerReference(containerName);
            return container.Uri.ToString();
        }

        /// <summary>
        /// Convert a dataset name into a valid Azure storage container name.
        /// </summary>
        /// <param name="name">The name of the dataset.</param>
        /// <param name="nextCharIndex">Next index to try for the container name.</param>
        /// <returns>The container name.</returns>
        public string ContainerNameFromDatasetName(string name, params string[] suffixes)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            // Attempting to find a unique container name
            int maxLength = ContainerNameMaxLength - suffixes.Aggregate(0, (length, suffix) => length + suffix.Length + 1);

            // Build the container name from the dataset name using Azure's container name constraints.
            var sb = new StringBuilder();
            foreach(char ch in name)
            {
                var isValidChar =
                    (ch >= 'A' && ch <= 'Z') ||
                    (ch >= 'a' && ch <= 'z') ||
                    (ch >= '0' && ch <= '9');
                if(isValidChar)
                {
                    sb.Append(Char.ToLower(ch));
                }
            }
            if(sb.Length < ContainerNameMinLength)
            {
                sb.Append("dataset");
            }
            else if(sb.Length > maxLength)
            {
                sb.Length = maxLength;
            }

            foreach(var suffix in suffixes)
            {
                sb.Append("-");
                sb.Append(suffix);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Attempts to find a unique container name for the dataset name.
        /// </summary>
        /// <param name="storage">The dataset storage details (will be updated).</param>
        /// <returns>Awaitable Task.</returns>
        public async Task FindUniqueDatasetContainerName(DatasetStorage storage)
        {
            await GetUniqueDatasetContainerName(storage);
        }

        /// <summary>
        /// Attempts to find a unique container name for the dataset update.
        /// </summary>
        /// <param name="storage">The dataset storage details (will be updated).</param>
        /// <returns>Awaitable Task.</returns>
        public async Task FindUniqueDatasetUpdateContainerName(DatasetStorage storage)
        {
            var suffix = $"u{DateTime.UtcNow.ToString("yyyyMMdd")}";
            await GetUniqueDatasetContainerName(storage, suffix);
        }

        /// <summary>
        /// Generate SAS token for uploading the dataset contents.
        /// </summary>
        /// <param name="accountName">The name of the account.</param>
        /// <param name="containerName">The name of the container within the storage account.</param>
        /// <returns>The SAS token URI.</returns>
        public async Task<Uri> GenerateSasTokenForUpdatingDatasetContainer(string accountName, string containerName)
        {
            if (containerName == null)
            {
                throw new ArgumentNullException(nameof(containerName));
            }

            var blobClient = CreateBlobClient(accountName);
            var container = blobClient.GetContainerReference(containerName);
            var accessPolicy = new SharedAccessBlobPolicy
            {
                Permissions =
                    SharedAccessBlobPermissions.Read |
                    SharedAccessBlobPermissions.Write |
                    SharedAccessBlobPermissions.Delete |
                    SharedAccessBlobPermissions.List |
                    SharedAccessBlobPermissions.Add |
                    SharedAccessBlobPermissions.Create
            };
            var tokenPolicy = new SharedAccessBlobPolicy
            {
                SharedAccessStartTime = DateTimeOffset.UtcNow.AddMinutes(-5),
                SharedAccessExpiryTime = DateTimeOffset.UtcNow.AddDays(7),
                Permissions = SharedAccessBlobPermissions.None
            };

            var permissions = await container.GetPermissionsAsync();
            permissions.SharedAccessPolicies[DatasetUpdateAccessPolicyName] = accessPolicy;
            await container.SetPermissionsAsync(permissions);

            var sasToken = container.GetSharedAccessSignature(tokenPolicy, DatasetUpdateAccessPolicyName);
            return new Uri(container.Uri + sasToken);
        }


        /// <summary>
        /// Disable SAS token for uploading the dataset contents.
        /// </summary>
        /// <param name="accountName">The name of the account.</param>
        /// <param name="containerName">The name of the container within the storage account.</param>
        /// <returns>The SAS token URI.</returns>
        public async Task DisableSasTokenForUpdatingDatasetContainer(string accountName, string containerName)
        {
            if (containerName == null)
            {
                throw new ArgumentNullException(nameof(containerName));
            }

            var blobClient = CreateBlobClient(accountName);
            var container = blobClient.GetContainerReference(containerName);

            var permissions = await container.GetPermissionsAsync();
            if(permissions.SharedAccessPolicies.ContainsKey(DatasetUpdateAccessPolicyName))
            {
                permissions.SharedAccessPolicies.Remove(DatasetUpdateAccessPolicyName);
            }
            await container.SetPermissionsAsync(permissions);
        }

        /// <summary>
        /// Create the container for the dataset.
        /// </summary>
        /// <param name="storage">The details of the dataset.</param>
        /// <returns>The url to the dataset container.</returns>
        public async Task<string> CreateDatasetContainer(DatasetStorage storage)
        {
            if (string.IsNullOrWhiteSpace(storage?.ContainerName))
            {
                throw new ArgumentNullException(nameof(storage.ContainerName));
            }

            var blobClient = CreateBlobClient(storage);
            var container = blobClient.GetContainerReference(storage.ContainerName);
            await container.CreateIfNotExistsAsync();

            return container.StorageUri.PrimaryUri.ToString();
        }

        /// <summary>
        /// Delete the container for the dataset.
        /// </summary>
        /// <param name="storage">The details of the dataset.</param>
        /// <returns>True if dataset container was deleted.</returns>
        public async Task<bool> DeleteDatasetContainer(DatasetStorage storage)
        {
            if (string.IsNullOrWhiteSpace(storage?.ContainerName))
            {
                throw new ArgumentNullException(nameof(storage.ContainerName));
            }

            var blobClient = CreateBlobClient(storage);
            var container = blobClient.GetContainerReference(storage.ContainerName);
            return await container.DeleteIfExistsAsync();
        }

        /// <summary>
        /// Creates the container sas token.
        /// </summary>
        /// <param name="accountName">Name of the account.</param>
        /// <param name="containerName">Name of the container.</param>
        /// <returns>The SAS URI to the container.</returns>
        public Uri CreateContainerSasToken(string accountName, string containerName)
        {
            if (containerName == null)
            {
                throw new ArgumentNullException(nameof(containerName));
            }

            var blobClient = CreateBlobClient(accountName);
            var container = blobClient.GetContainerReference(containerName);
            var policy = CreateContainerReadAccessPolicy();
            var sasToken = container.GetSharedAccessSignature(policy);
            return new Uri(container.Uri + sasToken);
        }

        /// <summary>
        /// Creates the file SAS token.
        /// </summary>
        /// <param name="accountName">Name of the account.</param>
        /// <param name="containerName">Name of the container.</param>
        /// <param name="blobName">Name of the blob file.</param>
        /// <returns>The SAS URI to the file.</returns>
        public Uri CreateFileSasToken(string accountName, string containerName, string blobName)
        {
            if (containerName == null)
            {
                throw new ArgumentNullException(nameof(containerName));
            }

            if (blobName is null)
            {
                throw new ArgumentNullException(nameof(blobName));
            }

            var blobClient = CreateBlobClient(accountName);
            var container = blobClient.GetContainerReference(containerName);
            var blob = container.GetBlockBlobReference(blobName);
            var policy = CreateFileReadAccessPolicy();
            var sasToken = blob.GetSharedAccessSignature(policy);
            return new Uri(blob.Uri + sasToken);
        }

        private CloudBlobClient CreateBlobClient(DatasetStorage datasetStorage)
        {
            if (datasetStorage is null)
            {
                throw new ArgumentNullException(nameof(datasetStorage));
            }

            return CreateBlobClient(datasetStorage.AccountName);
        }

        private CloudBlobClient CreateBlobClient(string accountName)
        {
            if (accountName == null)
            {
                throw new ArgumentNullException(nameof(accountName));
            }

            var account = accountName.ToLowerInvariant();
            var storageKey = Accounts[account];
            if (string.IsNullOrEmpty(storageKey))
            {
                throw new ArgumentException($"Could not find storage key for account \"{accountName}\"");
            }

            var storageAcct = new CloudStorageAccount(new StorageCredentials(account, storageKey), useHttps: true);
            return storageAcct.CreateCloudBlobClient();
        }

        private async Task GetUniqueDatasetContainerName(DatasetStorage storage, params string[] suffixes)
        {
            var blobClient = CreateBlobClient(storage);
            int nextCharOffs = 0;
            var containerName = ContainerNameFromDatasetName(storage.DatasetName, suffixes);
            while (true)
            {
                var container = blobClient.GetContainerReference(containerName);
                bool exists = await container.ExistsAsync();
                if (!exists)
                {
                    storage.ContainerName = containerName;
                    break;
                }

                if (nextCharOffs >= NextChars.Length)
                {
                    throw new InvalidOperationException("No more characters available to build valid container name.");
                }

                string suffix = NextChars.Substring(nextCharOffs++, 1);
                string[] allSuffixes = suffixes
                    .Concat(Enumerable.Repeat(suffix, 1))
                    .ToArray();
                containerName = ContainerNameFromDatasetName(storage.DatasetName, allSuffixes);
            }
        }

        // For reading entire container
        private static SharedAccessBlobPolicy CreateContainerReadAccessPolicy()
        {
            return new SharedAccessBlobPolicy
            {
                SharedAccessStartTime = DateTimeOffset.UtcNow.AddMinutes(-5),
                SharedAccessExpiryTime = DateTimeOffset.UtcNow.AddDays(30),
                Permissions = SharedAccessBlobPermissions.Read | SharedAccessBlobPermissions.List,
            };
        }

        // For reading single file
        private static SharedAccessBlobPolicy CreateFileReadAccessPolicy()
        {
            return new SharedAccessBlobPolicy
            {
                SharedAccessStartTime = DateTimeOffset.UtcNow.AddMinutes(-5),
                SharedAccessExpiryTime = DateTimeOffset.UtcNow.AddDays(30),
                Permissions = SharedAccessBlobPermissions.Read,
            };
        }
    }
}
