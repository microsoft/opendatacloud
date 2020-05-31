# Deployment

This document provides details about how to set up the initial deployment of the application, including creating the Azure Resources required by the application.

## Azure B2C Applications

Go to the [Azure Portal](https://portal.azure.com) and sign in with the subscription that will host the application.

1. Create an [Azure B2C Tenant](https://docs.microsoft.com/en-us/azure/active-directory-b2c/tutorial-create-tenant#create-an-azure-ad-b2c-tenant) for the application to use.

## Azure Resources

Go to the [Azure Portal](https://portal.azure.com) and sign in with the subscription that will host the application.

1. Create a new resource group for the application's resources.

1. Create the Azure Resources using the templates contained in the `src/Deployment` path. You may perform this deployment from the [Azure Cloud Shell](https://docs.microsoft.com/en-us/azure/azure-resource-manager/templates/deploy-cli#deploy-template-from-cloud-shell). Make sure all of the files are copied to the cloud shell:

    - `src\Deployment\azuredeploy.json`
    - `src\Deployment\templates\adminwebapp.json`
    - `src\Deployment\templates\appinsights.json`
    - `src\Deployment\templates\azfunction.json`
    - `src\Deployment\templates\batch.json`
    - `src\Deployment\templates\cosmosdb.json`
    - `src\Deployment\templates\keyvault.json`
    - `src\Deployment\templates\search.json`
    - `src\Deployment\templates\storage.json`
    - `src\Deployment\templates\webapp.json`

    The following parameters should be provided:

    - **_artifactsLocation** - the location of the template files above.
    - **_artifactsLocationSasToken** - a SAS token if required to access the template files.
    - **cosmosDbAccountName** - the name of the Cosmos DB account.
    - **appInsightsName** - the name of the App Insights account.
    - **webAppName** - the name of the main web application.
    - **adminWebAppName** - the name of the admin web application.
    - **apiAppName** - the name of the web API application.
    - **webAppSKU** - the pricing tier to use for the main web and web API applications.
    - **admninWebAppSKU** - the pricing tier to use for the admin web application.
    - **storageAccountName** - the name of the blob storage account.
    - **storageAccountType** - the type of the blob storage account.
    - **searchName** - the name of the Azure Search account.
    - **searchSKU** - the pricing tier to use for the Azure Search account.
    - **keyVaultName** - the name of the Key Vault.
    - **vaultSKU** - the pricing tier to use for the Key Vault.
    - **aadTenant** - the Azure B2C AAD tenant that was configured.
    - **aadAudience** - the Azure B2C audience that was configured.
    - **aadB2CScopes0** - the Azure B2C scope that was configured.
    - **aadPolicy** - the Azure B2C policy that was configured.
    - **aadWebApi** - the App ID that was configured for the web API.
    - **azfunctionAppName** - the name of the Azure Function.
    -