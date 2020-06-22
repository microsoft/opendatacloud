# Deployment - Azure B2C Applications

[Azure B2C](https://azure.microsoft.com/en-us/services/active-directory/external-identities/b2c/) is used for authenticating users to the application.

1. Create an [Azure B2C Tenant](https://docs.microsoft.com/en-us/azure/active-directory-b2c/tutorial-create-tenant#create-an-azure-ad-b2c-tenant) for the application to use. 

1. Within the B2C tenant, [register a application](https://docs.microsoft.com/en-us/azure/active-directory/develop/scenario-spa-app-registration) for the main web site using the "MSAL.js 2.0 with auth code flow" instructions.

    - Name the application to identify it as the main web application.

    - Add the "Single-page application (SPA)" platform and specific a redirect url `https://jwt.ms` (this will be changed later).

    - Under "Implicit Grant", ensure that both the `Access tokens` and `ID tokens` options are selected.
    
    Alternately reference this article specific to [registering apps for Azure B2C](https://docs.microsoft.com/en-us/azure/active-directory-b2c/tutorial-register-applications?tabs=app-reg-ga).
    
1. Create a client secret and copy the key. You will need this for enabling Identity Providers in Step 7. 

1. Configure the main web site registration so that it [exposes the Web API](https://docs.microsoft.com/en-us/azure/active-directory/develop/scenario-protected-web-api-app-registration).

    - Use the default application id.

    - Add a scope named "access_as_user" to allow the user access to the protected functionality of the application.

1. Register another application for the administration web site, also using the "MSAL.js 2.0 with auth code flow" instructions.

    - Name the application to identify it as the administration web application.

    - Add the "Single-page application (SPA)" platform and specific a redirect url `https://jwt.ms` (this will be changed later).

    - Under "Implicit Grant", ensure that both the `Access tokens` and `ID tokens` options are selected.

1. Expose the Web API for the administration application.

    - Use the default application id.

    - Add a scope named "access_as_user" to allow the user access to the administration application (we will set up user restrictions later).

1. Add one or more identity providers, for example, using [Microsoft accounts](https://docs.microsoft.com/en-us/azure/active-directory-b2c/identity-provider-microsoft-account).

1. Create a [sign-in user flow](https://docs.microsoft.com/en-us/azure/active-directory-b2c/tutorial-create-user-flows#create-a-sign-up-and-sign-in-user-flow).

    - Use a policy name like `B2C_1_UserSignIn`.

    - Check "Email Address" in "User attributes and claims".
