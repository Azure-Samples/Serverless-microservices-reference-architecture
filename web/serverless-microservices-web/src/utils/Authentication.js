import { LogLevel, PublicClientApplication } from '@azure/msal-browser';

// refer to https://github.com/AzureAD/microsoft-authentication-library-for-js/blob/dev/lib/msal-browser/docs/working-with-b2c.md

const ACCESS_TOKEN = 'rideshare_access_token';
const USER_DETAILS = 'rideshare_user_details';
let _accountId = null;
let _loginRequest;
let _tokenRequest;

export class Authentication {
  constructor() {
    // The window values below should by set by public/js/settings.js
    const msalConfig = {
      auth: {
        clientId: window.authClientId,
        authority: window.authAuthority,
        knownAuthorities: [window.knownAuthority],
        redirectUri: window.redirectUri
      },
      cache: {
        cacheLocation: "sessionStorage", // Configures cache location. "sessionStorage" is more secure, but "localStorage" gives you SSO between tabs.
        storeAuthStateInCookie: false, // If you wish to store cache items in cookies as well as browser cache, set this to "true".
      },
      system: {
        loggerOptions: {
          loggerCallback: (level, message, containsPii) => {
            if (containsPii) {	
                return;	
            }
            switch (level) {	
                case LogLevel.Error:	
                    console.error(message);	
                    return;	
                case LogLevel.Info:	
                    console.info(message);	
                    return;	
                case LogLevel.Verbose:	
                    console.debug(message);	
                    return;	
                case LogLevel.Warning:	
                    console.warn(message);	
                    return;
            }
          }
        }
      } 
    };

    this._publicClientApplication = new PublicClientApplication(msalConfig);

    _loginRequest = {
      scopes: window.loginScopes
    };

    _tokenRequest = {
      scopes: window.apiScopes,
      forceRefresh: false // Set this to "true" to skip a cached token and go to the server to get a new token
    }
  }

  getUser() {
    // if _accountId is not null, then the user is already logged in
    let user = null;
    if (_accountId) {
      user = this._publicClientApplication.getAccountByHomeId(_accountId);
    }
    return user;
  }

  getError() {
    return this._error;
  }

  getAccessToken() {
    _tokenRequest.account = this._publicClientApplication.getAccountByHomeId(_accountId);
    return this._publicClientApplication.acquireTokenSilent(_tokenRequest).then(
      response => {
        return response.accessToken;
      },
      error => {
        return this._publicClientApplication.acquireTokenPopup(_tokenRequest).then(
          response => {
            return response.accessToken;
          },
          err => {
            console.error(err);
          }
        );
      }
    );
  }

  login() {
    return this._publicClientApplication.loginPopup(_loginRequest)
    .then(
      response => {
        _accountId = response.account.homeAccountId;
        return response.account;
      },
      () => {
        return null;
      }
    );
  }

  logout() {
    const logoutRequest = {
      account: _accountId,
      redirectUri: window.logoutURI
    };
    this._publicClientApplication.logoutPopup(logoutRequest).then(() => {
      window.location.replace('/');
    });
  }

  isAuthenticated() {
    return !!this.getUser();
  }

  getAccessTokenOrLoginWithPopup() {
    _tokenRequest.account = this._publicClientApplication.getAccountByHomeId(_accountId);
    return this._publicClientApplication
      .acquireTokenSilent(_tokenRequest)
      .catch(err => {
        return this.login();
        });
  }
}

export function requireAuth(to, from, next) {
  const auth = new Authentication();
  if (!auth.isAuthenticated()) {
    next({
      path: '/no-auth',
      query: { redirect: to.fullPath }
    });
  } else {
    next();
  }
}

export function getToken() {
  return localStorage.getItem(ACCESS_TOKEN);
}

export function getUserDetails() {
  return JSON.parse(localStorage.getItem(USER_DETAILS));
}
