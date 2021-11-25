import { LogLevel, PublicClientApplication } from '@azure/msal-browser';

const ACCESS_TOKEN = 'rideshare_access_token';
const USER_DETAILS = 'rideshare_user_details';
let _user = null;
let _account = null;
let _loginRequest;




export class Authentication {
  constructor() {
    // The window values below should by set by public/js/settings.js
    const msalConfig = {
      auth: {
        clientId: window.authClientId,
        authority: window.authAuthority
      },
      cache: {
        cacheLocation: 'localStorage'
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
                    default:
                        return;
                }
            },
          logLevel: LogLevel.Verbose
        }
      } 
    };

    this._publicClientApplication = new PublicClientApplication(msalConfig);

    _loginRequest = {
      scopes: window.authScopes
    };
  }

  getUser() {
    return this._user;
  }

  getError() {
    return this._error;
  }

  getAccessToken() {
    return this._publicClientApplication.acquireTokenSilent(_loginRequest).then(
      accessToken => {
        return accessToken;
      },
      error => {
        return this._publicClientApplication.acquireTokenPopup(_loginRequest).then(
          accessToken => {
            return accessToken;
          },
          err => {
            console.error(err);
          }
        );
      }
    );
  }

  login() {
    return this._publicClientApplication.loginPopup(_loginRequest).then(
      idToken => {
        _account = idToken.account;
        const user = idToken.account.user;
        _user = user;
        if (user) {
          return user;
        } else {
          return null;
        }
      },
      () => {
        return null;
      }
    );
  }

  logout() {
    const logoutRequest = {
      account: _account.homeAccountId,
    };
    this._publicClientApplication.logoutPopup(logoutRequest);
  }

  isAuthenticated() {
    return !!this.getUser();
  }

  getAccessTokenOrLoginWithPopup() {
    return this._publicClientApplication
      .acquireTokenSilent(_loginRequest)
      .catch(err => {
        return this._publicClientApplication.loginPopup().then(() => {
          return this._publicClientApplication.acquireTokenSilent(_loginRequest);
        });
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
