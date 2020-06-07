# Deployment - Data Initialization

Certain data needs to be initialized before the application can be used.

Note, these instructions use the [.NET Core 2.1 SDK](https://dotnet.microsoft.com/download/dotnet-core/2.1) and the [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/?view=azure-cli-latest).

1. On the local machine, navigate to the `src/Msr.Odr.Admin` directory.

1. Sign in to Azure using the [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/authenticate-azure-cli?view=azure-cli-latest).

1. If you've got multiple Azure subscriptions, make sure you [set the subscription](https://docs.microsoft.com/en-us/cli/azure/account?view=azure-cli-latest#az-account-set) to the one with the Azure resources.

1. Build the admin utility using this command:

    ```
    dotnet build
    ```

