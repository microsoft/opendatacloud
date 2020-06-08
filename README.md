# Microsoft Research Open Data Platform

An open source version of the Microsoft Research Open Data platform (forked from the original code that implements https://msropendata.com).

This project is the data repository code behind the cloud hosted Microsoft Research Open Data Repository. The code can be used to instantiate a highly customizable data repository that utilizes a rich set of managed Azure cloud services to host datasets under a flexible licensing infrastructure with a high level of security and privacy. 
It provides the ability to deploy datasets directly to an Azure Data Science VM allowing development using popular open source tools such as Python/R on Juypter notebooks, Microsoft ML Server, and deep learning frameworks.

The repository is built on:
- A storage service for producers to securely store datasets on Blob storage and user metadata on CosmosDB 
- Customizable, fully responsive cross-device (web + mobile + tablet), and cross-platform user interfaces: 
    1) for users to browse, download, or consume the datasets on Azure computational resources
    2) for dataset administrators to onboard and administer, and monitor usage for these datasets
- Integration with Azure B2B and B2C Login using any email including your organization email address
- Azure Key Vault for secure storage of passwords and keys
- Azure App Insights for analytics that you can visualize via PowerBI or using your own dashboards through API access


Detailed documentation can be viewed [here](docs/index.md).

## Code of Conduct

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## License

Copyright (c) Microsoft Corporation. All rights reserved.

Licensed under the [MIT](LICENSE.txt) license.
