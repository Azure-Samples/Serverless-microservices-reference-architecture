# Client application

- [Passengers page](#passengers-page)
- [Drivers page](#drivers-page)
- [Authentication](#authentication)
- [Wrapping HTTP calls with authentication token](#wrapping-http-calls-with-authentication-token)
- [Next steps](#next-steps)

The Relecloud Rideshare website is a single page application (SPA) written in Vue.js. It is here that users sign in with an Azure Active Directory B2C account, access passenger and driver information, and request new trips. Each HTTP request flows through the API Management endpoints to each of the underlying Azure Functions that serve those requests.

## Passengers page

This page displays passenger information that is stored within Azure Active Directory B2C, using the [Microsoft Graph](https://developer.microsoft.com/en-us/graph/) API. When the `passengers` GET request is made to API Management, that request is routed to the `GetPassengers` function within the **Passengers** Function App.

```javascript
// Excerpt from the api/passengers.js file within the SPA website:

import { checkResponse, get } from '@/utils/http';
const baseUrl = window.apiPassengersBaseUrl;
const apiKey = window.apiKey;

// GET methods
export function getPassengers() {
  return get(`${baseUrl}/passengers`, {}, apiKey).then(checkResponse);
}
```

```csharp
// GetPassengers function within the Passengers Function App:

[FunctionName("GetPassengers")]
public static async Task<IActionResult> GetPassengers([HttpTrigger(AuthorizationLevel.Anonymous, "get",
        Route = "passengers")] HttpRequest req,
    ILogger log)
{
    log.LogInformation("GetPassengers triggered....");

    try
    {
        await Utilities.ValidateToken(req);
        var passengers = ServiceFactory.GetUserService();
        var (users, error) = await passengers.GetUsers();
        if (!string.IsNullOrWhiteSpace(error))
            throw new Exception(error);

        return (ActionResult)new OkObjectResult(users.ToList());
    }
    catch (Exception e)
    {
        var error = $"GetPassengers failed: {e.Message}";
        log.LogError(error);
        if (error.Contains(Constants.SECURITY_VALITION_ERROR))
            return new StatusCodeResult(401);
        else
            return new BadRequestObjectResult(error);
    }
}
```

The `UserService.GetUsers` method makes a secure call to the Microsoft Graph API as in the following excerpt:

```csharp
const string GraphBaseUrl = "https://graph.windows.net/";
const string GraphVersionQueryString = "?" + GraphVersion;
const string GraphVersion = "api-version=1.6";

private readonly AuthenticationContext _authContext;
private readonly ClientCredential _clientCreds;
private readonly string _graphUrl;

public UserService(string tenantId, string clientId, string clientSecret)
{
    _graphUrl = GraphBaseUrl + tenantId;

    var authority = "https://login.microsoftonline.com/" + tenantId;
    _authContext = new AuthenticationContext(authority);
    _clientCreds = new ClientCredential(clientId, clientSecret);
}

// Code removed for brevity...

public async Task<(IEnumerable<User>, string error)> GetUsers()
{
    var url = _graphUrl + "/users" + GraphVersionQueryString;

    // Call with HttpClient:
    var response = await client.GetAsync(url);
    if (response.IsSuccessStatusCode)
    {
        var json = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<UsersResult>(json);
        return (result.Value, null);
    }
    else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
    {
        var json = await response.Content.ReadAsStringAsync();
        var badRequest = JsonConvert.DeserializeObject<BadRequestResponse>(json);
        return (null, badRequest.ErrorMessage);
    }
    else
    {
        return (null, "Error Getting Users. HTTP Status Code: " + (int)response.StatusCode);
    }
}
```

The Microsoft Graph data is deserialized to a `UsersResult` object containing a collection of `User` strongly-typed class objects that store the user profile data that is ultimately returned to the client in JSON format.

![Screenshot of the Passengers page, displaying a list of passengers pulled from the Graph API](media/passengers-page.png 'Passengers page')

When you select a user, additional information about the user is displayed in a modal window.

![Screenshot of the passenger information modal window](media/passengers-page-information.png 'Passengers page: passenger information')

## Drivers page

The Drivers page displays driver information that is stored in Cosmos DB. The information includes the unique driver code, driver location, and whether they are currently accepting rides. If they are currently linked to an active trip, the accepting rides status is set to `false`. If no drivers within proximity to a passenger are accepting rides at the time of a trip request, the trip request will ultimately fail with a warning that no drivers are available if one does not accept before the configured time out period.

When the `drivers` GET request is made to API Management, that request is routed to the `GetDrivers` function within the **Drivers** Function App.

```javascript
// Excerpt from the api/passengers.js file within the SPA website:

import { checkResponse, post, get, put } from '@/utils/http';
const baseUrl = window.apiDriversBaseUrl;
const apiKey = window.apiKey;

// GET methods
export function getDrivers() {
  return get(`${baseUrl}/drivers`, {}, apiKey).then(checkResponse);
}
```

```csharp
// GetDrivers function within the Drivers Function App:

[FunctionName("GetDrivers")]
public static async Task<IActionResult> GetDrivers([HttpTrigger(AuthorizationLevel.Anonymous, "get",
        Route = "drivers")] HttpRequest req,
    ILogger log)
{
    log.LogInformation("GetDrivers triggered....");

    try
    {
        await Utilities.ValidateToken(req);
        var persistenceService = ServiceFactory.GetPersistenceService();
        return (ActionResult)new OkObjectResult(await persistenceService.RetrieveDrivers());
    }
    catch (Exception e)
    {
        var error = $"GetDrivers failed: {e.Message}";
        log.LogError(error);
        if (error.Contains(Constants.SECURITY_VALITION_ERROR))
            return new StatusCodeResult(401);
        else
            return new BadRequestObjectResult(error);
    }
}
```

The `GetDrivers` function calls the `RetrieveDrivers` method from the `IPersistenceService` implementation. In this case we using the `CosmosPersistenceService` to handle the request and pull the data from Cosmos DB:

```csharp
public async Task<List<DriverItem>> RetrieveDrivers(int max = Constants.MAX_RETRIEVE_DOCS)
{
    var error = "";
    double cost = 0;

    try
    {
        if (string.IsNullOrEmpty(_docDbDigitalMainCollectionName))
            throw new Exception("No Digital Main collection defined!");

        FeedOptions queryOptions = new FeedOptions { MaxItemCount = max };

        var query = (await GetDocDBClient(_settingService)).CreateDocumentQuery<DriverItem>(
                        UriFactory.CreateDocumentCollectionUri(_docDbDatabaseName, _docDbDigitalMainCollectionName), queryOptions)
                        .Where(e => e.CollectionType == ItemCollectionTypes.Driver)
                        .OrderByDescending(e => e.UpsertDate)
                        .Take(max)
                        .AsDocumentQuery();

        List<DriverItem> allDocuments = new List<DriverItem>();
        while (query.HasMoreResults)
        {
            var queryResult = await query.ExecuteNextAsync<DriverItem>();
            cost += queryResult.RequestCharge;
            allDocuments.AddRange(queryResult.ToList());
        }

        return allDocuments;
    }
    catch (Exception ex)
    {
        error = ex.Message;
        throw new Exception(error);
    }
    finally
    {
        _loggerService.Log($"{LOG_TAG} - RetrieveDrivers - Error: {error}");
    }
}
```

![Screenshot of the Drivers page, displaying a list of drivers pulled from Cosmos DB](media/drivers-page.png 'Drivers page')

When you select a driver, their information will appear within a modal window, including their car information that is displayed to a passenger when the driver has accepted their trip request.

![Screenshot of the Drivers page with the driver information modal window displayed](media/drivers-page-information.png 'Drivers page: driver information')

## Authentication

Azure Active Directory B2C is used for user authentication and profile management. With it, users can self-service their accounts, which means they are able to register for a new account, manage their profile information (mailing address, phone number, etc.), and initiate a password reset if needed.

![Screenshot displaying the Azure Active Directory B2C login form](media/azure-active-directory-b2c-login.png 'Azure Active Directory B2C Login form')

The screenshot above shows the home page of the website with the login form displayed in a popup window after selecting the Login link on the page menu. The features are as follows:

1. User selects Login on the page menu.
1. The `msal` library requests the login form popup from Azure Active Directory B2C via the following command: `this._userAgentApplication.loginPopup(this._scopes)`
1. A user may register for a new account by selecting the **Sign up now** link.
1. If a user forgets their password, they can reset it with the **Forgot your password?** link.

If you attempt to access a protected page, such as My Trip, Passengers, or Drivers, you will be prompted to log in before continuing:

![No Access page displayed when attempting to access a protected page before signing in](media/no-access.png 'No access')

The `utils` folder contains a file named `Authentication.js`, which wraps the [Microsoft Authentication Library (MSAL)](https://github.com/AzureAD/microsoft-authentication-library-for-js), enabling the client to easily log in and out of their Azure Active Directory B2C account:

```javascript
import { UserAgentApplication, Logger } from 'msal';
```

User settings are supplied by the `public/js/settings.js` file, which are used when instantiating a new `UserAgentApplication` class:

```javascript
export class Authentication {
  constructor() {
    // The window values below should by set by public/js/settings.js
    this._scopes = window.authScopes;
    this._clientId = window.authClientId;
    this._authority = window.authAuthority;

    var cb = this._tokenCallback.bind(this);
    var opts = {
      validateAuthority: false
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
      this._token = token;
    }
  }
```

Now that we have a reference to `msal`'s `UserAgentApplication`, we can use it to easily authenticate the user and perform other tasks against Azure Active Directory B2C:

```javascript
  getUser() {
    return this._userAgentApplication.getUser();
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
```

## Wrapping HTTP calls with authentication token

Also in the `utils` folder is an HTTP helper (`http.js`) that standardizes HTTP calls to Azure. The `getHeaders` method applies default headers, including the authorization header if a token is present:

```javascript
function getHeaders(token, apiKey) {
  let defaultHeaders = '';
  let authHeaders = '';

  if (apiKey) {
    defaultHeaders = {
      'Cache-Control': 'no-cache',
      'Ocp-Apim-Trace': true,
      'Ocp-Apim-Subscription-Key': apiKey
    };
  }

  if (token) {
    authHeaders = {
      Authorization: `Bearer ${token}`
    };
    if (apiKey) {
      defaultHeaders = { ...defaultHeaders, ...authHeaders };
    } else {
      defaultHeaders = authHeaders;
    }
  }

  return defaultHeaders;
}
```

Each HTTP method ensures these headers are added to each request:

```javascript
export function post(uri, data, apiKey) {
  return auth.getAccessToken().then(token => {
    return axios.post(uri, data, {
      headers: getHeaders(token, apiKey),
      withCredentials: false
    });
  });
}

export function put(uri, data, apiKey) {
  return auth.getAccessToken().then(token => {
    return axios.put(uri, data, {
      headers: getHeaders(token, apiKey),
      withCredentials: false
    });
  });
}

export function remove(uri, apiKey) {
  return auth.getAccessToken().then(token => {
    return axios.delete(uri, {
      headers: getHeaders(token, apiKey),
      withCredentials: false
    });
  });
}

export function get(uri, data = {}, apiKey) {
  if (Object.keys(data).length > 0) {
    uri = `${uri}?${qs(data)}`;
  }
  return auth.getAccessToken().then(token => {
    return axios.get(uri, {
      headers: getHeaders(token, apiKey),
      withCredentials: false
    });
  });
```

The HTTP helper helps simplify API calls and ensure standardization across calls to the microservices endpoints. The `api` folder contains files for each of these services (Drivers, Passengers, Trips) that are accessed by the website.

Here is a sample from the `drivers.js` API file, which uses the HTTP helper:

```javascript
import { checkResponse, post, get, put } from '@/utils/http';
const baseUrl = window.apiDriversBaseUrl;
const apiKey = window.apiKey;

// GET methods
export function getDrivers() {
  return get(`${baseUrl}/drivers`, {}, apiKey).then(checkResponse);
}

export function getDriver(driverCode) {
  return get(`${baseUrl}/drivers/${driverCode}`, {}, apiKey).then(
    checkResponse
  );
}

export function getActiveDrivers() {
  return get(`${baseUrl}/activedrivers`, {}, apiKey).then(checkResponse);
}

export function getDriversWithinLocation(latitude, longitude, miles) {
  return get(
    `${baseUrl}/drivers/${latitude}/${longitude}/${miles}`,
    {},
    apiKey
  ).then(checkResponse);
}

export function getDriverLocationChanges(driverCode) {
  return get(`${baseUrl}/driverlocations/${driverCode}`, {}, apiKey).then(
    checkResponse
  );
}

// POST methods
export function createDriver(driver) {
  return post(`${baseUrl}/drivers`, driver, apiKey).then(checkResponse);
}

// PUT methods
export function updateDriver(driver) {
  return put(`${baseUrl}/drivers`, driver, apiKey).then(checkResponse);
}

export function updateDriverLocation(driver) {
  return put(`${baseUrl}/driverlocations`, driver, apiKey).then(checkResponse);
}
```

## SignalR Service message handling and trip request flow

As [covered earlier](services-intercommunication.md#signalr-handler) in this document, the [SignalR Service](https://azure.microsoft.com/en-us/services/signalr-service/) makes it very easy to push real-time messages through a websocket connection between the website and the Azure Function that serves as the SignalR Service handler microservice.

As an example, the customer visits the "My Trip" page on the website to request a new trip. They start out by selecting the pickup location and their destination. When they select **Request Driver**, the following steps take place:

1.  The `requestDriver` method is called within the `Trip.vue` view. The passenger information is retrieved, using the signed in user's token, and this information along with the trip parameters are sent to the `createTrip` method within the `store/trips.js` file, which in turn updates the trip state and calls the `createTrip` method in the `api/trips.js` file:

    ```javascript
    // Trip.vue file excerpt:

    methods: {
    ...commonActions(['setUser']),
    ...tripActions(['setTrip', 'setCurrentStep', 'createTrip']),
    createTripRequest(trip) {
      this.createTrip(trip)
        .then(response => {
          this.setCurrentStep(1);
          this.$toast.success(
            `Request Code: <b>${response.code}`,
            'Driver Requested Successfully',
            this.notificationSystem.options.success
          );
        })
        .catch(err => {
          this.$toast.error(
            err.response ? err.response : err.message ? err.message : err,
            'Error',
            this.notificationSystem.options.error
          );
        });
    },
    requestDriver() {
      if (this.user) {
        getPassenger(this.user.idToken.oid)
          .then(response => {
            this.passengerInfo = response.data;

            var trip = {
              passenger: {
                code: this.passengerInfo.email,
                firstName: this.passengerInfo.givenName,
                surname: this.passengerInfo.surname,
                //"mobileNumber": this.passengerInfo.mobileNumber,
                email: this.passengerInfo.givenName
              },
              source: {
                latitude: this.selectedPickUpLocation.latitude,
                longitude: this.selectedPickUpLocation.longitude
              },
              destination: {
                latitude: this.selectedDestinationLocation.latitude,
                longitude: this.selectedDestinationLocation.longitude
              },
              type: 1 //0 = Normal, 1 = Demo
            };
            this.createTripRequest(trip);
          })
          .catch(err => {
            this.$toast.error(
              err.response,
              'Error',
              this.notificationSystem.options.error
            );
          });
      } else {
        this.$toast.error(
          'You must be logged in to start a new trip!',
          'Error',
          this.notificationSystem.options.error
        );
      }
    }
    ```

    ```javascript
    // store/trips.js excerpt:

    async createTrip({ commit }, value) {
      try {
        commit('contentLoading', true);
        let trip = await createTrip(value);
        commit('trip', trip.data);
        return trip.data;
      } catch (e) {
        throw e;
      } finally {
        commit('contentLoading', false);
      }
    }
    ```

    ```javascript
    // api/trips.js file:
    import { checkResponse, post } from '@/utils/http';
    const baseUrl = window.apiTripsBaseUrl;
    const apiKey = window.apiKey;

    // POST methods
    export function createTrip(trip) {
      return post(`${baseUrl}/trips`, trip, apiKey).then(checkResponse);
    }
    ```

1.  On this page, a **Driver Requested Successfully** toast message is displayed to the user, the **Trip requested** step is highlighted, and the user is told that Rideshare is searching for a nearby driver.

    ![Screenshot showing the My Trip page after the user submits a new trip request](media/trip-request-submitted.png 'Trip request submitted')

1.  The API Management **/trips** endpoint routes the request to the `CreateTrip` function within the **Trips** Function App. This function validates the authentication token, validates the passenger information, and finally calls the `UpsertTrip` method within the [Persistence Layer](api-endpoints.md#rideshare-apis):

    ```csharp
    [FunctionName("CreateTrip")]
    public static async Task<IActionResult> CreateTrip([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "trips")] HttpRequest req,
        ILogger log)
    {
        log.LogInformation("CreateTrip triggered....");

        try
        {
            await Utilities.ValidateToken(req);
            string requestBody = new StreamReader(req.Body).ReadToEnd();
            TripItem trip = JsonConvert.DeserializeObject<TripItem>(requestBody);

            // validate
            if (trip.Passenger == null || string.IsNullOrEmpty(trip.Passenger.Code))
                throw new Exception("A passenger with a valid code must be attached to the trip!!");

            trip.EndDate = null;
            var persistenceService = ServiceFactory.GetPersistenceService();
            return (ActionResult)new OkObjectResult(await persistenceService.UpsertTrip(trip));
        }
        catch (Exception e)
        {
            var error = $"CreateTrip failed: {e.Message}";
            log.LogError(error);
            if (error.Contains(Constants.SECURITY_VALITION_ERROR))
                return new StatusCodeResult(401);
            else
                return new BadRequestObjectResult(error);
        }
    }
    ```

1.  The `UpsertTrip` method within the `Persistence Layer` saves the trip information to Cosmos DB and calls the `TripCreated` method of the `ChangeNotifierService` to initiate the **Trip Manager** Durable Orchestrator, as outlined in the [Durable Orchestrators](api-endpoints.md#durable-orchestrators) section:

    ```csharp
    // Excerpt from the CosmosPersistenceService.UpsertTrip method:

    var response = await (await GetDocDBClient(_settingService)).UpsertDocumentAsync(UriFactory.CreateDocumentCollectionUri(_docDbDatabaseName, _docDbDigitalMainCollectionName), trip);

    if (!isIgnoreChangeFeed && blInsert)
    {
        await _changeNotifierService.TripCreated(trip, await RetrieveActiveTripsCount());
    }
    ```

    ```csharp
    // Excerpt from the ChangeNotifierService.TripCreated method:

    // Start a trip manager
    if (!_settingService.IsEnqueueToOrchestrators())
    {
        var baseUrl = _settingService.GetStartTripManagerOrchestratorBaseUrl();
        var key = _settingService.GetStartTripManagerOrchestratorApiKey();
        if (string.IsNullOrEmpty(baseUrl) || string.IsNullOrEmpty(key))
            throw new Exception("Trip manager orchestrator base URL and key must be both provided");

        await Utilities.Post<dynamic, dynamic>(null, trip, $"{baseUrl}/tripmanagers?code={key}", new Dictionary<string, string>());
    }
    else
    {
        await _storageService.Enqueue(trip);
    }
    ```

1.  From here, the **Trip Manager** Durable Orchestrator is triggered, which in turn triggers the **Trip Monitor** Durable Orchestrator. As the trip progresses, new Event Grid events are fired to trigger actions by [multiple listeners](services-intercommunication.md#event-grid), including the [SignalR Azure Functions handler](services-intercommunication.md#signalr-handler). The `/components/SignalRTrips.vue` file contains the [JavaScript SignalR client code](services-intercommunication.md#javascript-signalr-client) that connects to the SignalR Service and receives and processes each message. In the code excerpt below, we are handling the `tripDriverPicked` SignalR message, updating the current trip step, setting the local trip state to display to the user, and firing the toast notification:

    ```javascript
    hubConnection.on('tripDriverPicked', trip => {
      console.log(`tripDriverPicked Trip code: ${trip.code}`);
      this.setCurrentStep(2);
      this.setTrip(trip);
      this.$toast.info(
        `Trip Code: ${trip.code}. Message: tripDriverPicked.`,
        'Driver Picked',
        this.notificationSystem.options.info
      );
    });
    ```

The following is a screenshot of the My Trip page that is updated in real-time as a result of the SignalR messages flowing to the SPA website:

![Screenshot of the My Trip page that has been updated as a result of SignalR messages](media/my-trip-page-completed-trip.png 'My Trip page')

These are the following features of this page:

1. Toast message showing the trip status, appropriate to the current step of the trip. In this case, the `tripCompleted` SignalR message was received.
1. Visual trip progress indicator highlights the current stage of the trip as it progresses (`this.setCurrentStep(n)`).
1. The driver information is displayed after a driver is selected. This happens when the `tripDriverPicked` SignalR message is received by updating the local trip state with the `this.setTrip(trip)` command.

## Next steps

Create the required assets for the SPA website, then configure it for your environment:

- [Create the Web App Service Plan](setup.md#create-the-web-app-service-plan)
- [Create the Web App](setup.md#create-the-web-app)
- [Configure and build the web solution](setup.md#web)

Read details about how Relecloud is performing monitoring and testing in this reference architecture:

- [Monitoring and testing](monitoring-testing.md)
  - [Integration testing](monitoring-testing.md#integration-testing)
  - [Monitoring](monitoring-testing.md#monitoring)
    - [Telemetry correlation](monitoring-testing.md#telemetry-correlation)
    - [Monitoring for different audiences](monitoring-testing.md#monitoring-for-different-audiences)
