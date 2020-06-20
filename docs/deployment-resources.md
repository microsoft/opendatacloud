# Deployment - Azure Resources

[Azure Resource Manager (ARM)](https://docs.microsoft.com/en-us/azure/azure-resource-manager/templates/overview) templates are used to define the Azure resources required by the application. 

Note, these instructions use the [Azure Powershell](https://docs.microsoft.com/en-us/powershell/azure/?view=azps-4.2.0) scripting components.

1. Open the [Azure Portal](https://portal.azure.com) and sign in with the Azure subscription that will host the application.

1. Create a [new resource group](https://docs.microsoft.com/en-us/azure/azure-resource-manager/management/manage-resource-groups-portal) for the application's resources.

1. Open the [Azure Cloud Shell](https://docs.microsoft.com/en-us/azure/cloud-shell/overview). This will ensure a storage account is created that can be used for the deployment.

1. On your local machine, go to the `src/Deployment` directory.

1. Sign in to Azure [using Azure Powershell](https://docs.microsoft.com/en-us/powershell/azure/authenticate-azureps?view=azps-4.2.0).

1. If you have multiple Azure subscriptions, make sure the [context is set](https://docs.microsoft.com/en-us/powershell/azure/context-persistence?view=azps-4.2.0) to the subscription that contains the resource group created above. The `Get-AzContext` command will show you what the current context is.

1. Perform a test run of the deployment using this command:

    ```
    ./DeployResources.ps1 -ResourceGroupName <rg-name>
    ```

    where `rg-name` is the name of the resource group where the application will be created. This will verify that resource names are unique and that you've got the appropriate Azure resource types provisioned within your subscription.

1. If everything looks good, create the resources using this command:

    ```
    ./DeployResources.ps1 -ResourceGroupName <rg-name> -Deploy
    ```

After a few minutes, you will be able to see the resources created in the Azure Portal.

## Update the Application Configuration

After the resources have been deployed, update the configuration with the new App Insights instrumentation key.

1. In the Azure Portal, go to the Application Insights resource and copy the Instrumentation Key.

1. Edit the configuration file, `AppConfig.json`, and update the `instrumentationKey` value.

1. Update the application configuraiton files with the command:

    ```
    node GenerateAppConfig.js
    ```

1. Check these configuration files into your repository for building and deploying the application components later.

## Update the Azure B2C Configuration

Return to the Azure B2C settings and update the configuration based on the web and administration application that were created.

1. Update the web application registration:

    * Under the single-page application settings, add two redirect URLs (changing `myopendata-web-app` to the name of the web application that was created). The second URL is used when running the application on a local development machine.

        * https://myopendata-web-app.azurewebsites.net/auth
        * http://localhost:53048/auth

    * Change the logout URL.

        * https://myopendata-web-app.azurewebsites.net/

1. Update the administration application registration:

    * Under the single-page application settings, add two redirect URLs (changing `myopendata-web-admin` to the name of the administration application that was created). The second URL is used when running the application on a local development machine.

        * https://myopendata-web-admin.azurewebsites.net/
        * http://localhost:58784/

    * Change the logout URL.

        * https://myopendata-web-admin.azurewebsites.net/logout

