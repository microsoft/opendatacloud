# Deployment

This document provides details about how to set up the initial deployment of the application, including creating the Azure Resources required by the application.

Use the [Azure Portal](https://portal.azure.com) to configure the Azure Resources. Make sure you sign in with the Azure subscription that will host the application.

## Azure B2C Applications

[Azure B2C](https://azure.microsoft.com/en-us/services/active-directory/external-identities/b2c/) is used for authenticating users to the application.

1. Create an [Azure B2C Tenant](https://docs.microsoft.com/en-us/azure/active-directory-b2c/tutorial-create-tenant#create-an-azure-ad-b2c-tenant) for the application to use.

1. Within the B2C tenant, [register a application](https://docs.microsoft.com/en-us/azure/active-directory/develop/scenario-spa-app-registration) for the main web site using the "MSAL.js 2.0 with auth code flow" instructions.

    - Name the application to identify it as the main web application.

    - Add the "Single-page application (SPA)" platform and specific a redirect url `https://jwt.ms` (this will be changed later).

    - Under "Implicit Grant", ensure that both options are unselected.

1. Configure the main web site registration so that it [exposes the Web API](https://docs.microsoft.com/en-us/azure/active-directory/develop/scenario-protected-web-api-app-registration).

    - Use the default application id.

    - Add a scope named "access_as_user" to allow the user access to the protected functionality of the application.

1. Register another application for the administration web site, also using the "MSAL.js 2.0 with auth code flow" instructions.

    - Name the application to identify it as the administration web application.

    - Add the "Single-page application (SPA)" platform and specific a redirect url `https://jwt.ms` (this will be changed later).

    - Under "Implicit Grant", ensure that both options are unselected.

1. Expose the Web API for the administration application.

    - Use the default application id.

    - Add a scope named "access_as_user" to allow the user access to the administration application (we will set up user restrictions later).

1. Add an identity provider, for example, using [Microsoft accounts](https://docs.microsoft.com/en-us/azure/active-directory-b2c/identity-provider-microsoft-account).

1. Create a [sign-in user flow](https://docs.microsoft.com/en-us/azure/active-directory-b2c/tutorial-create-user-flows#create-a-sign-up-and-sign-in-user-flow).

    - Use a policy name like `B2C_1_UserSignIn`.

    - Check "Email Address" in "User attributes and claims".

## Send Grid

Notifications are delivered via email using the [SendGrid](https://sendgrid.com/partners/azure/) service.

1. Create a [SendGrid account](https://docs.microsoft.com/en-us/azure/sendgrid-dotnet-how-to-send-email).

1. Generate an [API Key](https://docs.microsoft.com/en-us/azure/sendgrid-dotnet-how-to-send-email#to-find-your-sendgrid-api-key). Save this key for setting up the application configuration.

## Application Configuration



## Azure Resources

1. Create a new resource group for the application's resources.

1. Create the Azure Resources using the templates contained in the `src/Deployment` path. You may perform this deployment from the [Azure Cloud Shell](https://docs.microsoft.com/en-us/azure/azure-resource-manager/templates/deploy-cli#deploy-template-from-cloud-shell). Make sure all of the files are copied to the cloud shell:
