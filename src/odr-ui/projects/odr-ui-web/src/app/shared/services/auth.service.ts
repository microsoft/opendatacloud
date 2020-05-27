import { Inject, Injectable } from "@angular/core";
import { NavigationEnd, Router } from "@angular/router";
import { Configuration, Logger, LogLevel, UserAgentApplication } from "msal";
import { Observable, ReplaySubject } from "rxjs";
import { filter, tap } from "rxjs/operators";
import { environment } from "../../../environments/environment";
import { isEmptyValue } from 'odr-ui-shared';

const AppAuthenticationStateKey = "app-auth-state";

interface AzureADConfig {
  tenant: string;
  audience: string;
  policy: string;
}

interface B2CAppConfig {
  authority: string;
  clientID: string;
}

interface AppAuthenticationState {
  navTo?: string;
  navFrom?: string;
}

interface CurrentUserDetails {
  name?: string;
  emails?: string[];
  uniqueId?: string;
}

const orDefaultRoute = (url: string) => {
  return url || "/";
};

/**
 * Manage User Authentication
 */
@Injectable()
export class AuthService {
  private currentUserDetails: CurrentUserDetails = {};
  private currentUrl: string;
  private applicationConfig: B2CAppConfig = {
    clientID: "",
    authority: ""
  };

  private authenticationStatus = new ReplaySubject<boolean>(1);
  private bearerToken = new ReplaySubject<string>(1);
  private clientApplication: UserAgentApplication;

  constructor(
    @Inject("AZURE_AD_CONFIG") azureADConfig: AzureADConfig,
    private router: Router
  ) {
    router.events
      .pipe(
        filter(evt => evt instanceof NavigationEnd),
        tap(({ url }: NavigationEnd) => {
          this.currentUrl = url;
        })
      )
      .subscribe();

    this.applicationConfig = {
      authority:
        "https://login.microsoftonline.com/tfp/" +
        azureADConfig.tenant +
        "/" +
        azureADConfig.policy,
      clientID: azureADConfig.audience
    };

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

    const redirectUri = [
      ...window.location.href.split("/").slice(0, 3),
      ""
    ].join("/");
    const postLogoutRedirectUri = [
      ...window.location.href.split("/").slice(0, 3),
      ""
    ].join("/");
    const authConfig: Configuration = {
      auth: {
        clientId: this.applicationConfig.clientID,
        authority: this.applicationConfig.authority,
        validateAuthority: true,
        redirectUri,
        postLogoutRedirectUri,
        navigateToLoginRequestUrl: false
      },
      cache: {
        cacheLocation: "localStorage"
      },
      system: {
        logger
      }
    };

    this.clientApplication = new UserAgentApplication(authConfig);
    this.clientApplication.handleRedirectCallback((authErr, authRsp) => {
      if (authErr) {
        console.error(authErr);
        this.authenticationStatus.next(false);
        this.bearerToken.next("");
      } else if (authRsp) {
        const { navTo } = this.getAppAuthenticationState();
        this.updateUserDetails(navTo);
      }
    });

    setTimeout(() => {
      this.updateUserDetails();
    }, 0);
  }

  private updateUserDetails(navigateTo?: string): void {
    const account = this.clientApplication.getAccount();
    if (account) {
      this.clientApplication
        .acquireTokenSilent({
          scopes: [this.applicationConfig.clientID]
        })
        .then(authRsp => {
          const emails = !isEmptyValue(authRsp.idTokenClaims.upn)
            ? [authRsp.idTokenClaims.upn]
            : !isEmptyValue(authRsp.idTokenClaims.email)
              ? [authRsp.idTokenClaims.email]
              : [];
          this.currentUserDetails = {
            name: authRsp.idTokenClaims.name,
            emails,
            uniqueId: authRsp.idTokenClaims.sub
          };
          this.bearerToken.next(authRsp.accessToken);
          this.authenticationStatus.next(true);
          if (navigateTo) {
            this.router.navigateByUrl(navigateTo);
          }
        })
        .catch(authErr => {
          console.error(authErr);
          this.authenticationStatus.next(false);
          this.bearerToken.next("");
        });
    } else {
      this.authenticationStatus.next(false);
      this.bearerToken.next("");
    }
  }

  public getAuthenticationStatus(): Observable<boolean> {
    return this.authenticationStatus.asObservable();
  }

  public getBearerToken(): Observable<string> {
    return this.bearerToken.asObservable();
  }

  public logout(): void {
    // Removes all sessions, need to call AAD endpoint to do full logout
    this.clientApplication.logout();
  }

  public navigateToLogin(toAuthenticatedRoute: string = null) {
    this.setAppAuthenticationState({
      navTo: orDefaultRoute(toAuthenticatedRoute || this.currentUrl),
      navFrom: orDefaultRoute(this.currentUrl)
    });
    setTimeout(() => {
      this.router.navigateByUrl("/auth", { skipLocationChange: true });
    });
  }

  public redirectToLoginPage() {
    this.clientApplication.loginRedirect({
      scopes: [this.applicationConfig.clientID]
    });
  }

  getAuthenticatedUserDetails(): CurrentUserDetails {
    return this.currentUserDetails;
  }

  private getAppAuthenticationState(): AppAuthenticationState {
    let state = {};
    const value = window.sessionStorage.getItem(AppAuthenticationStateKey);
    if (value) {
      try {
        state = JSON.parse(value);
      } catch (err) {
        console.error(err);
      }
    }
    return state;
  }

  private setAppAuthenticationState(
    state: AppAuthenticationState = null
  ): void {
    if (state) {
      const value = JSON.stringify(state);
      window.sessionStorage.setItem(AppAuthenticationStateKey, value);
    } else {
      window.sessionStorage.removeItem(AppAuthenticationStateKey);
    }
  }
}
