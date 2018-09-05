import { UserAgentApplication, Logger } from 'msal';

const ACCESS_TOKEN = 'rideshare_access_token';
const ID_TOKEN = 'rideshare_id_token';
const EXPIRES_AT = 'rideshare_expires_at';
const USER_DETAILS = 'rideshare_user_details';

let logger = new Logger((level, message, containsPii) => {
  console.log(message);
});

export class Authentication {
  constructor() {
    // The window values below should by set by public/js/settings.js
    this._scopes = window.authScopes;
    this._clientId = window.authClientId;
    this._authority = window.authAuthority;

    var cb = this._tokenCallback.bind(this);
    var opts = {
      //logger: logger
    };
    this._userAgentApplication = new UserAgentApplication(
      this._clientId,
      this._authority,
      cb,
      opts
    );
  }

  _tokenCallback(errorDesc, token, error, tokenType) {
    this._error = error;
    if (tokenType === 'access_token') {
      //localStorage.setItem(ACCESS_TOKEN, token);
      // Please note: do NOT do this in production! Should grab this value from the auth service.
      //let expiresAt = 60 * 1000 + new Date().getTime();
      //localStorage.setItem(EXPIRES_AT, expiresAt);
      this._token = token;
    } else {
      //localStorage.removeItem(ACCESS_TOKEN);
    }
  }

  getUser() {
    return this._userAgentApplication.getUser();
  }

  getError() {
    return this._error;
  }

  getAccessToken() {
    return this._userAgentApplication.acquireTokenSilent(this._scopes).then(
      accessToken => {
        return accessToken;
      },
      error => {
        return this._userAgentApplication.acquireTokenPopup(this._scopes).then(
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
    //this._userAgentApplication.loginRedirect(this._scopes);
    return this._userAgentApplication.loginPopup(this._scopes).then(
      idToken => {
        const user = this._userAgentApplication.getUser();
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
    this._userAgentApplication.logout();
  }

  isAuthenticated() {
    return !!this.getUser();
  }

  getAccessTokenOrLoginWithPopup() {
    return this._userAgentApplication
      .acquireTokenSilent(this._scopes)
      .catch(err => {
        return this._userAgentApplication.loginPopup().then(() => {
          return this._userAgentApplication.acquireTokenSilent(this._scopes);
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
