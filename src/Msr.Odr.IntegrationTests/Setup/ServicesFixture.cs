using System;
using System.Threading.Tasks;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Auth;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Msr.Odr.Services;
using Msr.Odr.Services.Configuration;
using Xunit;

namespace Msr.Odr.IntegrationTests.Setup
{
    public class ServicesFixture
    {
        public const string Name = "Injected Services";
        public static string KeyVaultUrl { get; set; }

        private readonly AsyncLazy<DocumentClient> cosmosDbClient;
        private readonly AsyncLazy<CloudBlobClient> blobClient;

        public ServicesFixture()
        {
            cosmosDbClient = new AsyncLazy<DocumentClient>(async () =>
            {
                return await CreateCosmosClient();
            });
            blobClient = new AsyncLazy<CloudBlobClient>(() =>
            {
                return CreateBlobClient();
            });
        }

        private static Lazy<IServiceProvider> LazyServiceProvider { get; } =
            new Lazy<IServiceProvider>(InitializeServices);

        private static IServiceProvider InitializeServices()
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddEnvironmentVariables();

            var interimConfig = builder.Build();
            KeyVaultUrl = interimConfig["keyVaultUrl"];

            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
            builder.AddAzureKeyVault(KeyVaultUrl, keyVaultClient, new DefaultKeyVaultSecretManager());

            var configuration = builder.Build();

            var services = new ServiceCollection();

            services.AddSingleton<IConfiguration>(configuration);
            services.AddOptions()
              .AddOptions<CosmosConfiguration>()
              .Bind(configuration.GetSection("documents"));
            services.AddOptions()
              .AddOptions<StorageConfiguration>()
              .Bind(configuration.GetSection("storage"));
            services.AddOptions()
              .AddOptions<SearchConfiguration>()
              .Bind(configuration.GetSection("search"));

            services.AddMemoryCache();
            services.AddScoped<SasTokenService>();
            services.AddScoped<DatasetOwnersService>();
            services.AddScoped<DatasetStorageService>();
            services.AddScoped<DatasetSearchService>();
            services.AddScoped<DatasetEditStorageService>();

            return services.BuildServiceProvider();
        }

        public T GetService<T>() where T : class
        {
            return LazyServiceProvider.Value.GetService<T>();
        }

        public T CreateInstance<T>() where T : class
        {
            return ActivatorUtilities.CreateInstance<T>(LazyServiceProvider.Value);
        }

        public async Task<DocumentClient> GetCosmosClient()
        {
            return await cosmosDbClient.Value;
        }

        public async Task<CloudBlobClient> GetBlobClient()
        {
            return await blobClient.Value;
        }

        private async Task<DocumentClient> CreateCosmosClient()
        {
            var config = GetService<IOptions<CosmosConfiguration>>().Value;
            var client = new DocumentClient(config.Uri, config.Key, new ConnectionPolicy
            {
                ConnectionMode = ConnectionMode.Direct,
                ConnectionProtocol = Protocol.Tcp
            });
            await client.OpenAsync();
            return client;
        }

        private Task<CloudBlobClient> CreateBlobClient()
        {
            var storageConfig = GetService<IOptions<StorageConfiguration>>();
            var account = storageConfig.Value.Accounts.DefaultStorageAccount;
            var storageKey = storageConfig.Value.Accounts[account];
            var creds = new StorageCredentials(account, storageKey);
            var storageAcct = new CloudStorageAccount(creds, useHttps: true);
            return Task.FromResult(storageAcct.CreateCloudBlobClient());
        }
    }

    [CollectionDefinition(ServicesFixture.Name)]
    public class ServicesFixtureCollection : ICollectionFixture<ServicesFixture>
    {
    }
}
