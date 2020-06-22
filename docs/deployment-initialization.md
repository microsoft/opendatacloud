# Deployment - Data Initialization

Certain data needs to be initialized before the application can be used.

Note, these instructions use the [.NET Core 2.1 SDK](https://dotnet.microsoft.com/download/dotnet-core/2.1) and the [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/?view=azure-cli-latest).

1. On the local machine, navigate to the `src/Msr.Odr.Admin` directory.

1. Sign in to Azure using the [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/authenticate-azure-cli?view=azure-cli-latest).

1. If you've got multiple Azure subscriptions, make sure you [set the subscription](https://docs.microsoft.com/en-us/cli/azure/account?view=azure-cli-latest#az-account-set) to the one with the Azure resources.

1. Get the Principal Id for your user account using the command:

    ```
    az ad user show --id "my@example.com" --query objectId
    ```

    replacing `my@example.com` with the email address used to sign into Azure.

1. Allow access to the Key Vault for your user using the command:

    ```
    az keyvault set-policy --name "my-keyvault" --object-id my-object-id --secret-permissions get list set
    ```

    replacing `my-keyvault` with the name of the key vault and `my-object-id` with the principal id of your user account.

1. Build the admin utility using the command:

    ```
    dotnet build
    ```

1. Create the collections in Cosmos DB using the command:

    ```
    dotnet run -- cosmos init    
    ```

1. Create the Azure Search indexes using the command:

    ```
    dotnet run -- search init --indexes datasets,files,nominations
    ```

1. Create an API key for Application Insights and store it in the Key Vault using the command:

    ```
    az monitor app-insights api-key create --api-key admin-reader --read-properties ReadTelemetry -g [resource-group-name] --app [app-insights-name]

    az keyvault secret set --name ApplicationInsights--Key --vault-name [key-vault-name] --value [generated-api-key]

    az keyvault secret set --name ApplicationInsights--ApplicationId --vault-name [key-vault-name] --value [generated-application-id]
    ```

    _Note that the `generated-api-key` is in the output from the first command but the `generated-application-id` will be found in the Azure Portal under "API Access" for the Application Insights resource._

This utility can now be used to configure the initial set of data into the application.

## Domains

The initial set of dataset domains can be found here:

- `src/Msr.Odr.Admin/Data/Files/domains.json`

Add this to the application using the command:

```
dotnet run -- data init --types domains
```

## Licenses

The common dataset licenses can be found here:

- `src/Msr.Odr.Admin/Data/Files/license-open-use-of-data-agreement.json`
- `src/Msr.Odr.Admin/Data/Files/license-computational-use-of-data-agreement.json`
- `src/Msr.Odr.Admin/Data/Files/license-community-data.json`
- `src/Msr.Odr.Admin/Data/Files/license-creative-commons.json`
- `src/Msr.Odr.Admin/Data/Files/license-microsoft-research-2019.json`

_Note that the first two licenses are of special importance to the application's user interface and should not be removed without also updating the affected user interface._

Add these documents to the application using the command:

```
dotnet run -- data init --types licenses
```

## Frequently Asked Questions (FAQs)

The initial set of FAQs can be found here:

- `src/Msr.Odr.Admin/Data/Files/initial-faqs.md`

Add these templates to the application using the command:

```
dotnet run -- data init --types faqs
```

## Email Templates

The initial set templates used for email notifications can be found here:

- `src/Msr.Odr.Admin/Data/Files/dataset-issue.html`
- `src/Msr.Odr.Admin/Data/Files/dataset-nomination.html`
- `src/Msr.Odr.Admin/Data/Files/general-issue.html`
- `src/Msr.Odr.Admin/Data/Files/nomination-approved.html`
- `src/Msr.Odr.Admin/Data/Files/nomination-rejected.html`

These templates can be customized using the `src/Email/EmailTemplates` project.

Add these licenses to the application using the command:

```
dotnet run -- data init --types email
```

## ARM Templates

The application can create a virual machine (VM), including VMs that are oriented towards data science, along with the contents of a dataset into a user's own Azure subscription. The ARM templates that define these virtual machines can be found here:

- `assets/deployments/dsvm-ubuntu`
- `assets/deployments/dsvm-windows`
- `assets/deployments/vm-ubuntu`
- `assets/deployments/vm-windows`

A file that maps these deployments to the application can be found here:

- `src/Msr.Odr.WebApi/template-mapping.json`

Add these deployments to the application using the command:

```
dotnet run -- data init --types arm
```

## Eligible Dataset Owners

You can specify which users are eligible to be dataset owners by specifying a list of RegEx expressions that match email addresses. The initial set of eligible dataset owners can be found here:

- `src/Msr.Odr.Admin/Data/Files/datasetOwners.json`

Add this to the application using the command:

```
dotnet run -- data init --types datasetowners
```

## Upload Utility

The upload utility can be built using the command:

```
upload-utility/build-upload-utility.ps1
```

Note, this command requires the [Windows Installer WiX tools](https://wixtoolset.org/).

This creates an installer file, `upload-utility/build/DatasetUtil.msi`, which should be uploaded to a public location. This location should be recorded in the `src/Msr.Odr.WebAdminPortal/appsettings.production.json` file in the `Assets`, `DatasetUtil` setting.

## Azure Batch

The Azure Batch applications and configuration can be set up using these steps:

1. Create the Azure Batch `DatasetJob` definition using the command:

    ```
    dotnet run -- batch init
    ```

1. Build the batch application using the command:

    ```
    src/scripts/build-batch-applications.ps1
    ```

1. Deploy the batch applications using the command:

    ```
    src/scripts/deploy-batch-applications.ps1 -name [batch-account-name] -update
    ```

1. In the Azure Portal for the Azure Batch resource, edit the `DatasetPool` and add both of the applications packages to the pool.
