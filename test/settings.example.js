// Auth
window.authClientId = '';
window.authAuthority = 'https://{tenant_name}.b2clogin.com/{tenant_name}.onmicrosoft.com/{policy_name}';
window.knownAuthority = '{tenant_name}.b2clogin.com';
window.redirectUri = 'http://localhost:4280';
window.loginScopes = ['openid', '{api_scope}'];  // e.g. 'https://rideshare.onmicrosoft.com/serverless-spa-api/Rides.ReadWrite'
window.apiScopes = ['{api_scopes}'];  // e.g. 'https://rideshare.onmicrosoft.com/serverless-spa-api/Rides.ReadWrite'
window.authEnabled = true;


// API endpoints
window.apiKey = '';
window.apiBaseUrl = '';
window.apiDriversBaseUrl = 'http://localhost:7071/api';
window.apiTripsBaseUrl = 'http://localhost:7072/api';
window.apiPassengersBaseUrl = 'http://localhost:7073/api';
window.signalrInfoUrl = 'http://localhost:7072/api/';
