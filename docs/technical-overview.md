# Techical Overview

Most components within the application are written in C# using .NET Core. The web application user interface is created using Angular. The following components are used:

- src/Deployment - a set of ARM templates the define the Azure Resources that are required for the application.

- assets/deployments - a set of ARM templates that define virtual machines, including data science virtual machines, that can be deployed into a user's Azure subscription along with one or more datasets.

- src/Msr.Odr.Admin - a command-line utility that can be used to initialize and administer the application.



- src/Msr.Odr.Batch.Shared - a shared assembly for common functionality used by the Azure Batch applications.

- src/Msr.Odr.Batch.ImportDatasetApp - an Azure Batch application that imports a dataset into the application's Azure resources.

- 

