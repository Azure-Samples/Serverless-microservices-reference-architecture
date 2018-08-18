import { UserAgentApplication, Logger } from 'msal';

let logger = new Logger((level, message, containsPii)=>{
    console.log(message);
});

export class Authentication {

    constructor(clientId, authority, scopes) {
        this._scopes = scopes;

        var cb = this._tokenCallback.bind(this);
        var opts = { 
            //logger: logger
        };
        this._userAgentApplication = new UserAgentApplication(clientId, authority, cb, opts);

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
    
    login(){
        this._userAgentApplication.loginRedirect(this._scopes);
    }
    
    logout(){
        this._userAgentApplication.logout();
    }
}
