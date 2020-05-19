# How do I use a download link for an entire dataset?

A download link for an entire dataset provides the location of the dataset in
Azure as well as a special time-limited key that allows you to download the
entire dataset. This link can be used with tools that can copy files from Azure, like
the following:

- [AzCopy](https://docs.microsoft.com/en-us/azure/storage/common/storage-use-azcopy) - a
  command-line tool for Windows or Linux that copies files to and from Azure.
- [Azure Storage Explorer](https://azure.microsoft.com/en-us/features/storage-explorer/) -
  a utility that is used to manage Azure storage.
  
# How do I use a download link for a single file within a dataset?

A download link for a single file within a dataset provides the location of the file
in Azure as well as special time-limited key for downloading the single file. The link
is a URL that can be posted in a browser in order to download it or used with another
tool, such as
[wget](https://en.wikipedia.org/wiki/Wget) or
[cURL](https://en.wikipedia.org/wiki/CURL).

# What does it mean to "Deploy to Azure"?

Azure provides resources that are geared towards needs of data scientists and researchers.
Specifically, there are virtual machines (computers that run in the cloud) preconfigured
with the popular
[data analysis and research tools](https://azure.microsoft.com/en-us/services/virtual-machines/data-science-virtual-machines/).

When you select "Deploy to Azure", the application can create one of these virtual machines
for you and copy the selected dataset so that it is available to start using. 

# Do I need an Azure Subscription to deploy to Azure?

Yes, but you can create a [free trial](https://azure.microsoft.com/en-us/free/) to get started.

# Are there fees for the use of a dataset?

No, there is no additional fee or surcharge for using these datasets.  However, if you deploy
a dataset to your Azure subscription, there are fees for using Azure resources. Additional
pricing information can be found [here](https://azure.microsoft.com/en-us/pricing/).

# Are there restrictions on the size of a dataset?

When the size of datasets exceed a certain size it becomes impractical to transmit
it over the wire.  A definitive size is a bit subjective, but for the time being,
we are considering that datasets over 250 GB should not be uploaded directly to ODR.
Instead, the user should be provided with some instructions on how to contact the
correct Microsoft staff for procedures to ship the data on physical media.
