import { UserAgentApplication, Logger } from 'msal';

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

    this._user = this._userAgentApplication.getUser();
  }

  _tokenCallback(errorDesc, token, error, tokenType) {
    this._error = error;
    console.log(tokenType);
    if (tokenType === '') {
      this._token = token;
    }
  }

  getUser() {
    return this._user;
  }

  getAccessToken() {
    return this._userAgentApplication.acquireTokenSilent(this._scopes);
  }

  login() {
    this._userAgentApplication.loginRedirect(this._scopes);
  }

  logout() {
    this._userAgentApplication.logout();
  }

  authenticated() {
    // TODO: complete logic
    return true;
  }
}
