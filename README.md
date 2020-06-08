# Microsoft Research Open Data Platform
![MSROpenData_header](../docs/images/msropendata_header.png)

An open source version of the Microsoft Research Open Data platform. 

This project is the data repository code behind the cloud hosted Microsoft Research Open Data Repository. This code is forked from the original code that implements Microsoft Research Open Data. Refer to https://msropendata.com for an example of what a data repository instance created from this code will look like. 


The code can be used to instantiate a highly customizable cloud based data repository to host and share datasets under a flexible licensing infrastructure with a high level of security and privacy. 

It provides the ability to deploy datasets directly to an Azure Data Science VM allowing development using popular open source tools such as Python/R on Juypter notebooks, and deep learning frameworks.


The repository code can be used to:
- Create a storage service for producers to securely store datasets in the Azure cloud
- Create a customizable, fully responsive cross-device (web + mobile + tablet), and cross-platform user interfaces: 
    1) for users to browse, download, or consume the datasets on Azure computational resources
    2) for dataset administrators to onboard and administer, and monitor usage for these datasets
- Create distinct authenticated experiences for regular users and organizational users 
- Allow dataset owners to administer datasets once they have been onboarded
- Monitor dataset usage analytics that you can visualize via PowerBI or using your own dashboards through API access



Detailed technical overview can be viewed [HERE](./technical-overview.md)

Detailed instructions on deploying the repository is available [HERE](./deployment.md)

Once deployed, guidance on onboarding and administering datasets is available [HERE](./import-dataset.md)

# Code of Conduct

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

# License

Copyright (c) Microsoft Corporation. All rights reserved.

Licensed under the [MIT](LICENSE.txt) license.

# Contact us
For any questions, email us at odrfriends@microsoft.com
or log an issue on Github