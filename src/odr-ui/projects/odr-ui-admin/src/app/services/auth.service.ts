import { Inject, Injectable } from "@angular/core";
import { environment } from "../../environments/environment";
import { UserAgentApplication, Logger, LogLevel, Configuration } from "msal";
import { CurrentUser } from "../models/current-user.model";
import { ReplaySubject, Observable } from "rxjs";
import { Router } from "@angular/router";

interface B2CAppConfig {
  authority: string;
  clientID: string;
}

interface AzureADConfig {
  tenant: string;
  audience: string;
  policy: string;
}

// Manage User Authentication
@Injectable()
export class AuthService {
  private userAgentApplication: UserAgentApplication;
  private currentUserSubject = new ReplaySubject<CurrentUser>(1);
  private applicationConfig: B2CAppConfig = {
    clientID: "",
    authority: ""
  };

  constructor(
    @Inject("AZURE_AD_CONFIG") private azureADConfig: AzureADConfig,
    private router: Router
  ) {
    const loggerOptions = environment.production
      ? {
          level: LogLevel.Error
        }
      : {
          level: LogLevel.Verbose
        };
    const logger = new Logger((logLevel, message) => {
      if (logLevel === LogLevel.Error) {
        console.error(`[MSAL] ${message}`);
      } else {
        console.log(`[MSAL] ${message}`);
      }
    }, loggerOptions);
    const redirectUri = [...window.location.href.split("/").slice(0, 3)].join(
      "/"
    );
    const postLogoutRedirectUri = [
      ...window.location.href.split("/").slice(0, 3),
      "logout"
    ].join("/");

    this.applicationConfig = {
      authority:
        "https://login.microsoftonline.com/tfp/" +
        azureADConfig.tenant +
        "/" +
        azureADConfig.policy,
      clientID: azureADConfig.audience
    };

    const authConfig: Configuration = {
      auth: {
        clientId: this.applicationConfig.clientID,
        authority: this.applicationConfig.authority,
        validateAuthority: true,
        redirectUri,
        postLogoutRedirectUri
      },
      cache: {
        cacheLocation: "localStorage"
      },
      system: {
        logger
      }
    };

    this.userAgentApplication = new UserAgentApplication(authConfig);
    this.userAgentApplication.handleRedirectCallback((authErr, authRsp) => {
      if (authErr) {
        console.error(authErr);
        this.currentUserSubject.next({
          isAuthenticated: false
        });
      } else if (authRsp) {
        this.login();
      }
    });

    if (
      !this.userAgentApplication.isCallback(window.location.hash) &&
      window.parent === window &&
      !window.opener
    ) {
      const account = this.userAgentApplication.getAccount();
      if (account) {
        this.login();
      } else {
        this.currentUserSubject.next({
          isAuthenticated: false
        });
      }
    }
  }

  public getCurrentUser(): Observable<CurrentUser> {
    return this.currentUserSubject.asObservable();
  }

  public navigateToUnauthenticatedPage() {
    this.router.navigate(["/logout"]);
  }

  public login() {
    const account = this.userAgentApplication.getAccount();
    if (!account) {
      this.userAgentApplication.loginRedirect({
        scopes: [this.applicationConfig.clientID]
      });
    } else {
      this.userAgentApplication
        .acquireTokenSilent({
          scopes: [this.applicationConfig.clientID]
        })
        .then(authRsp => {
          const { accessToken: bearerToken } = authRsp;
          const options = {
            method: "GET",
            headers: new Headers({
              Accept: "application/json",
              Authorization: `Bearer ${bearerToken}`
            })
          };
          return fetch("/api/current-user", options).then(rsp => {
            if (rsp.ok) {
              const name = authRsp.idTokenClaims.name;
              const displayId = authRsp.idTokenClaims.email;
              this.currentUserSubject.next({
                isAuthenticated: true,
                displayId,
                name,
                bearerToken
              });
            } else {
              this.router.navigate(["/logout", { unauthorized: true }]);
            }
          });
        })
        .catch(err => {
          console.error(err);

          // Refreshing the token doesn't work right now due to this:
          // https://github.com/AzureAD/microsoft-authentication-library-for-js/issues/135
          // this.userAgentApplication.acquireTokenRedirect();

          // Once the above issue is addressed, then the above line should replace this:
          this.userAgentApplication.loginRedirect();
        });
    }
  }

  public logout(): void {
    this.userAgentApplication.logout();
  }
}
