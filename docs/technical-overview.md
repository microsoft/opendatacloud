# Techical Overview

The Open Data Platform provides a web-based user interface for end users to browse a set of public datasets. The dataset contents are stored in Azure Blog Storage, where each dataset is stored in a separate container. Another container may also be created to store compressed (.zip/.tar.gz) archives of the dataset.

Details about the datsets and files within the dataset are kept in Cosmos DB. These documents are also imported into Azure Search to allow a user to search for specific dataset details.

A few assumptions are made with this implementation:

- The datasets are for public consumption. It does not include functionality to restrict access to datasets.

- An acknowledgement is required from the user to note that the license for using a dataset has been agreed to.  Therefore, a user must signon to the system (using Azure B2C) in order to record this acknowledgment. Only after the user has acknowledged the acceptance of the license will the user get to download or otherwise work with the data.

- The application supports different licenses per dataset as well as a set of standard licenses.

## Components

Most components within the application are written in C# using .NET Core. The web application user interface is created using Angular. The following components are used:

- `src/Deployment` - a set of ARM templates the define the Azure Resources that are required for the application.

- `assets/deployments` - a set of ARM templates that define virtual machines, including data science virtual machines, that can be deployed into a user's Azure subscription along with one or more datasets.

- `src/Msr.Odr.Admin` - a command-line utility that can be used to initialize and administer the application.

- `src/Msr.Odr.Model` - model definitions.

- `src/Msr.Odr.Services` - services that support the application.

- `src/Msr.Odr.Web` - web server for the main application.

- `src/Msr.Odr.WebApi` - web API server for the main application.

- `src/Msr.Odr.WebAdminPortal` - web server for the adminstration application.

- `src/odr-ui` - Angular application for the main and admin applications, including shared components.

- `src/Msr.Odr.Batch.Shared` - a shared assembly for common functionality used by the Azure Batch applications.

- `src/Msr.Odr.Batch.ImportDatasetApp` - an Azure Batch application that imports a dataset into the application's Azure resources.

- `src/Msr.Odr.Batch.CompressDatasetApp` - an Azure Batch application that compresses a dataset into .zip and .tar.gz files.

- `src/Msr.Odr.Services.Test` - Unit tests.

- `src/Msr.Odr.IntegrationTests` - Integration tests.

- `upload-utility` - a small Windows-based utility to upload a dataset.

## Azure Resources

The application uses the following types of Azure Resources.

- [App Service](https://azure.microsoft.com/en-us/services/app-service/) - for web, web API, and admin applications.

- [Blob Storage](https://azure.microsoft.com/en-us/services/storage/blobs/) - for storage of the dataset content files (one dataset per container).

- [Cosmos DB](https://azure.microsoft.com/en-us/services/cosmos-db/) - for storing dataset details and other application information.

- [Azure Search](https://azure.microsoft.com/en-us/services/search/) - to provide search functionality for datasets.

- [Azure Batch](https://azure.microsoft.com/en-us/services/batch/) - to run occasional jobs that import a dataset and compress its contents into archive files.

- [Azure B2C](https://azure.microsoft.com/en-us/services/active-directory/external-identities/b2c/) - to provide user authentication (both main and admin applications).

- [Application Insights](https://azure.microsoft.com/en-us/services/monitor/) - to track information about activity within the applications.

- [Azure Functions](https://azure.microsoft.com/en-us/services/functions/) - to generate email notifications.

- [SendGrid](https://azuremarketplace.microsoft.com/en-us/marketplace/apps/sendgrid.sendgrid) - to deliver email notifications.

- [KeyVault](https://azure.microsoft.com/en-us/services/key-vault/) - to store secrets required by the application.