# Azure Functions

This directory contains the NodeJS Azure Functions that are
used by the MSR ODR application.

There are two functions:

- `SendEmailTrigger` - responds to changes in documents and sends emails
    as appropriate.
- `ManualEmailTrigger` - responds to manual request to send an email for
    a particular document.

These triggers work off of the `UserData` collection in CosmosDB. When
a document is changed, the `SendEmailTrigger` is fired and then what
email is sent is based on the changes made to the document.

## Types of Emails

There are five types of emails that can be sent:

- `Dataset Nomination (id: 286353d1-2d54-4d25-8930-00465136cb96)`

    Sent to the ODR admin group when a  nomination is received.

- `Dataset Issue (id: ab4313a8-b3fc-447a-bf61-413e6a6c983f)`

    Sent to the ODR admin group when a dataset nomination is received.

- `General Issue (id: e63bd247-16f6-45d9-bc49-0ad1372963c9)`

    Sent to the ODR admin group when a dataset nomination is received.

- `Dataset Published (id: 0926ee72-a168-4ae5-afb3-12e46d09e263)`

    Sent to the ODR admin group when a dataset nomination is received.

- `Nomination Rejected (id: 8b891dda-6d5f-4bcb-ab11-b5f0ebd61dcd)`

    Sent to the ODR admin group when a dataset nomination is received.

The content for the template used by each of these emails is stored
in the CosmosDB `Datasets` collection with the following values:

- `id` - from above
- `datasetId` - `fd56f7c8-89a5-4997-82bc-95e955468e14` (shared by all
    configuration values and is used as the partition key)
- `dataType` - `email-templates`
- `html` - the HTML email content which includes replaceable parameters

## Local Testing

You can test the Azure Functions locally using [these instructions](https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local).
These functions are currently using version 1 of the runtime.

You will also need to configure a `local.settings.json` file containing
the configuration for running the Azure Functions.

You can disable triggers by modified any of the `function.json` files.

```
  "disabled": true,
```

Start the local runtime using the command

```
func host start
```

You can send an id to the `ManualEmailTrigger` using Bash with the command

```
testing/sendemail.sh id
```
