import * as microsoftTeams from '@microsoft/teams-js';
import * as msal from '@azure/msal-browser';

const MSAL_CONFIG: msal.Configuration = {
  auth: {
    clientId: process.env.REACT_APP_MSA_ONLY_CLIENT_ID ?? '',
    redirectUri: `${window.location.origin}/auth-end/msa`,
    authority: 'https://login.microsoftonline.com/consumers',
  },
  cache: {
    cacheLocation: 'localStorage',
  },
};

const SCOPES = process.env.REACT_APP_MSA_ONLY_SCOPE
  ? process.env.REACT_APP_MSA_ONLY_SCOPE.split(' ')
  : [];

const CONSUMER_TENANT_ID = '9188040d-6c67-4c5b-b112-36a304b66dad';

export class MsalAuth {
  private msalInstance: msal.PublicClientApplication;

  constructor() {
    this.msalInstance = new msal.PublicClientApplication(MSAL_CONFIG);
  }

  handleErrorReceived(authError: any) {
    console.log(authError);
    microsoftTeams.authentication.notifyFailure(authError);
  }

  handleTokenReceived(accessToken: string) {
    microsoftTeams.authentication.notifySuccess(accessToken);
  }

  acquireToken() {
    const request: msal.SilentRequest = {
      scopes: SCOPES,
    };

    const accounts = this.msalInstance.getAllAccounts();
    const account =
      accounts.filter((a) => a.tenantId === CONSUMER_TENANT_ID)[0] || null;

    if (account !== null) {
      request.account = account;
      return this.msalInstance
        .acquireTokenSilent(request)
        .then((response) => response.accessToken)
        .catch((error) => {
          console.log('silent token acquisition fails.');
          if (error instanceof msal.InteractionRequiredAuthError) {
            console.log(
              'Interaction required. Will show login button to user when API call fails',
            );
          } else {
            console.error(error);
            this.handleErrorReceived(error);
          }
          return undefined;
        });
    }
  }

  async attemptLogIn() {
    try {
      this.msalInstance.loginRedirect({
        scopes: SCOPES,
      });
    } catch (error) {
      console.error(error);
      this.handleErrorReceived(error);
    }
  }

  loadAuthModule() {
    this.msalInstance
      .handleRedirectPromise()
      .then((response: msal.AuthenticationResult | null) => {
        let accountObj = null;
        if (response !== null) {
          accountObj = response.account;
          this.handleTokenReceived(response.accessToken);
        }
      })
      .catch(console.error);
  }
}
