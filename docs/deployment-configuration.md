# Deployment - Application Configuration

The application requires a number of configuration options to be set up before it can be deployed.

Note, these instructions require [NodeJS](https://nodejs.org/en/download/).

1. Clone the source code to a local directory on your machine.

1. Open a command prompt/shell and navigate to the `src/Deployment` directory.

1. Choose a prefix that will be used for Azure Resources (like `myopendata`). It should start with a letter and contain nothing but letters and numbers and be no more than 10 characters.

1. Create an initial configuration file by running the command:

    ```
    node GenerateAppConfig.js [prefix]
    ```

1. A new file, `AppConfig.json`, will be created in the same directory that looks something like this:

    ```js
    {
        "databaseAccountName": "myopendata-db",
        "searchAccountName": "myopendata-search",
        "datasetStorageName": "myopendatadatasets01",
        "applicationStorageName": "myopendataappstorage",
        "webPlanName": "myopendata-web-plan",
        "webAppName": "myopendata-web-app",
        "webApiName": "myopendata-web-api",
        "webAdminName": "myopendata-web-admin",
        "batchAccountName": "myopendatabatch",
        "consumptionPlanName": "myopendata-email-plan",
        "emailFxnName": "myopendata-email-fxn",
        "fromEmail": "Name <name@example.com>",
        "toEmail": [
            "Name One <name.one@example.com>",
            "Name Two <name.two@example.com>"
        ],
        "sendGridApiKey": "SEND-GRID-API-KEY",
        "appInsightsName": "myopendata-insights",
        "keyVaultName": "myopendata-keyvault",
        "domainName": "myopendata-web-app.azurewebsites.net",
        "b2cTenant": "B2C-TENANT-NAME",
        "b2cWebAudience": "B2C-WEB-CLIENT-ID",
        "b2cWebPolicy": "B2C-WEB-POLICY-NAME",
        "b2cAdminAudience": "B2C-ADMIN-CLIENT-ID",
        "b2cAdminPolicy": "B2C-ADMIN-POLICY-NAME",
        "authorizedAdminUsers": [
            "one@example.com",
            "two@example.com"
        ]
    }
    ```

    Modify this file with the specifics of your deployment.

    - `fromEmail` - the email address that notifications will be from.
    - `toEmail` - a list of email addresses where notifications will be delivered to.
    - `sendGridApiKey` - the key your created for the SendGrid API.
    - `domainName` - if your instance will be set up to use a custom domain, then this should be set to the domain name you will use (e.g. `myopendata.com`).
    - `b2cTenant` - the Azure B2C tenant that will be used (e.g. `myopendata.onmicrosoft.com`).
    - `b2cWebAudience` - the client id of the Web application configured in B2C.
    - `b2cWebPolicy` - the name of the Web application policy configured in B2C.
    - `b2cAdminAudience` - the client id of the Administration application configured in B2C.
    - `b2cAdminPolicy` - the name of the Administration application policy configured in B2C.
    - `authorizedAdminUsers` - a list of email addresses that are authorized to use the administration application.

1. Copy these configuraiton settings to other configuration files within the application by running this command:

    ```
    node GenerateAppConfig.js
    ```

1. Check these configuration files into your repository for building and deploying the application components later.
