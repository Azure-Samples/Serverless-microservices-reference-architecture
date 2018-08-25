# Introduction to serverless microservices

## What are microservices?

Explain in one or two paragraphs what microservices are, and the concepts. Link out to MS docs for full explanation.

TBA - Joel

## What is serverless?

Like the previous section, one or two paragraphs explaining serverless concepts, components in Azure, then links to docs.

TBA - Joel

## Macro Architecture

Relecloud decided to use the following macro architecture in their RideShare solution:

![RideShare Macro Architecture](media/macro-architecture.png)

The architecture major building blocks are:

| Component | Technology | Description |
|---|---|---|
|RideShare Web App |Vu JS SPA|A multi-purpose, single-page application web app that allows users to sign up and sign in against a B2C Active Directory instance. Users have different levels and permissions. For example, passenger users can request rides and receive real-time notifications of ride status. Executive users, on the other hand, can view top-level reports that reveal rides and system performance |
|API Manager |[Azure API Manager](https://docs.microsoft.com/en-us/azure/api-management/)|An API gateway that acts as a front-end to the solution APIs. Among many other benefits, the API management service provides RideShare APIs with security verification, usage telemetry, documentation and rate limiting. |
|RideShare APIs |C# [Azure Functions](https://azure.microsoft.com/en-us/services/functions/)|Three Function Apps are deployed to serve RideShare's APIs: Drivers, Trips and Passengers. These APIs are exposed to the Web App applications via the API manager and provide CRUD operations for each of RideShare entities  |
|Durable Orchestartors |C# [Durable Functions](https://docs.microsoft.com/en-us/azure/azure-functions/durable-functions-overview)|Trip Manager, Monitor and Demo orchestrators are deployed to manage the trip and provide real-time status updates. The orchestrators are launched for the duration of the trip and they perform management and monitoring functions as will be explained in more [details](#durable-orchestrators) later. In essence, these orchestrators make up the heart of the solution. |
|Event Emitter |[Event Grid Topic](https://docs.microsoft.com/en-us/azure/event-grid/overview) | A custom topic used to externalize trips as they go through the diffrent stages.|
|Event Subscribers |Functions & Logic Apps | Several event grid topic subscribers listen to the event grid topic events to provide multi-process capability of an externalized trip|

The following are the Event Grid Subscribers:

| Subscriber | Technology | Description |
|---|---|---|
|Notification|[Logic App](https://azure.microsoft.com/en-us/services/logic-apps/)| A trip processor to notify admins i.e. emails or SMS as the trip passes through the different stages.|
|SignalR| C# Azure Function| A trip processor to update passengers (via browsers or mobile apps) in real-time about trip status.|       
|PowerBI| C# Azure Function| A trip processor to insert the trip into an SQL Database and possibly into a PowerBI dataset (via APIs).|  
|Archiver|Node Azure Function| A trip processor to archive the trip into Cosmos|

Relecloud decided to use the following criteria to determine when a certain piece of functionality is to be considered a Microservice:

- The functionality must scale or be deployed independently from other parts. 
- The functionality must be written in a separate language/technology like Node.js in case there is some certain expertise that is only available in that specific technology.
- The functionality must be isolated by a clean boundary 

Given the above principles, the following are identified as Microservices:

| Microservice | Technology | Reason |
|---|---|---|
|Drivers APIs| C# | The `Drivers` API is code and deployment independent isolated in a functions app.|
|Trips APIs| C# | The `Trips` API is code and deployment independent isolated in a functions app.|
|Passengers APIs| C# | The `Passengers` API is code and deployment independent isolated in a functions app.|
|Durable Orchestartors| C# |The Trip `Manager`, `Monitor` and `Demo` i.e. Orchestrators are independent as they provide the heart of the solution. They need to scale and deploy independently.|
|Event Grid Notification Handler| Logic App | The `Logic App` handler adds value to the overall solution but work independently. |
|Event Grid SignalR Handler | C# | The `SignalR` handler adds value to the overall solution but work independently.|
|Event Grid PowerBI Handler | C# | The `PowerBI` handler adds value to the overall solution but work independently.|
|Event Grid Archiver | NodeJS | The NodeJS `Archiver` handler adds value to the overall solution but work independently.|

**Please note** that, due to code layout, some Microservices might be a Function within a Function App. Examples of this are the `Event Grid SignalR Handler` and `Event Grid PowerBI Handler` Microservices. They are both part of the `Trips` Functions App.

In the sections below, we will go trough each architecture component in more details. 

## Web App

TBA - Joel

Describe how the SPA communicates with the B2C AD to provide different levels of permissions.

## API Management

There are many benefits to use an API manager. In the case the RideShare solution, there are really four major benefits:

1. **Security**: the API manager layer verifies the incoming requests' [JWT](https://jwt.io/) token against the B2C Authority URL. This is accomplished via an inbound policy that intercepts each call:

```xml
<validate-jwt header-name="Authorization" failed-validation-httpcode="401" failed-validation-error-message="Unauthorized. Access token is missing or invalid."> 
    <openid-config url="<--your_own_authorization_url-->" />
    <audiences>
        <audience><-- your_own_app_id --></audience>
    </audiences>
</validate-jwt>
```

**Please note** that Relecloud considered using Azure Functions [Filters](https://github.com/Azure/azure-webjobs-sdk/tree/dev/src/Microsoft.Azure.WebJobs.Host/Filters) to intercept HTTP calls and validate the JWT token in code instead of relying on an APIM layer. This has the advantage of applying security validation regardless of whether an APIM is used or not.  

Here is the `Attribute` that was created:

```csharp
public class B2cValidationAttribute : FunctionInvocationFilterAttribute
{
    public override Task OnExecutingAsync(FunctionExecutingContext executingContext, CancellationToken cancellationToken)
    {
        var httpRequest = executingContext.Arguments.First().Value as HttpRequest;
        if (httpRequest == null)
            throw new ValidationException("Http Request is not the first argument!");

        var validationService = ServiceFactory.GetTokenValidationService();
        if (validationService.AuthEnabled)
        {
            //TODO: Not the best way to do this!!
            var user = validationService.AuthenticateRequest(httpRequest).Result;

            if (user == null)
            {
                //httpRequest.HttpContext.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
                //return Task.FromResult(0);
                throw new ValidationException("Unauthorized!");
            }
        }

        return base.OnExecutingAsync(executingContext, cancellationToken);
    }
}
```

It can then be used to decorate a specific function like this:

```csharp
[B2cValidation]
[FunctionName("GetTrips")]
public static async Task<IActionResult> GetTrips([HttpTrigger(AuthorizationLevel.Function, "get", Route = "trips")] HttpRequest req,
    ILogger log)
{
...
}
``` 

It is very elegant and it actually does work! But unfortunately, it seems that it can only throw exceptions. Relecloud was not  able to find a way to abort the HTTP request and throw a 401 status code. If an exception is thrown in the filter pipeline, the caller gets a 500 Internal Service Error which is hardly descriptive of the problem.

Eventually, Relecloud received an input from a security expert who advised that the `JWT Validation` be added to the code instead of APIM for the very same reason that the the HTTP endpoints will be protected regardless of whether APIM is used or not. To support this, the reference implementation includes a utility method that can be used to check the validation:

```csharp
public static async Task ValidateToken(HttpRequest request)
{
    var validationService = ServiceFactory.GetTokenValidationService();
    if (validationService.AuthEnabled)
    {
        var user = await validationService.AuthenticateRequest(request);
        if (user == null)
            throw new Exception(Constants.SECURITY_VALITION_ERROR);
    }
}
```

This method is used in each API Function to validate tokens and it throws a `known` exception. Upon exception, the function examines the exception to determine whether to send 401 (security check) or 400 (bad request) as shown here:

```csharp
[FunctionName("GetDrivers")]
public static async Task<IActionResult> GetDrivers([HttpTrigger(AuthorizationLevel.Function, "get", 
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

**Please note** that the token validation is enforced only if the `AuthEnabled` setting is set to true. 

2. **Documentation**: the API manager provides developers writing applications against RideShare APIs with a complete development portal for documentation and testing

3. **Usage Stats**: the API manager provides usage stats on all API calls (and report failures) which makes it really convenient to assess the API performance

4. **Rate Limiting**: the API manager can be configured to rate limit APIs based on IP origin, access, etc. This can be useful to prevent DOD attacks or provide different tiers of access based on users. 

**Please note** that, in the case of Azure Functions, while the APIs are front-ended with an API manager (and hence shielded, protected and rate limited), the APIs are still publicly available!!! This means that a DOD attack or other attacks can still happen against the bare APIs if someone discovers them in the wide.    

## RideShare APIs

As the macro architecture depicts, the APIs are implemented using C# Azure Functions. They have a very simple architecture that can be illustrated as follows:

![APIs Architecture](media/function-apis-architecture.png)

Please note the folowing:

- The `Persistence Layer` implements the `IPersistenceService` interface. In the reference solution implementation, there are two implementations: `CosmosPersistenceLayer` and `SqlPersistenceService`. Only the `CosmosPersistenceLayer` is fully implemented in the reference implementation:

```csharp
public interface IPersistenceService
{
    // Drivers
    Task<DriverItem> RetrieveDriver(string code);
    Task<List<DriverItem>> RetrieveDrivers(int max = Constants.MAX_RETRIEVE_DOCS);
    Task<List<DriverItem>> RetrieveDrivers(double latitude, double longitude, double miles, int max = Constants.MAX_RETRIEVE_DOCS);
    Task<List<DriverItem>> RetrieveActiveDrivers(int max = Constants.MAX_RETRIEVE_DOCS);
    Task<int> RetrieveDriversCount();
    Task<DriverItem> UpsertDriver(DriverItem driver, bool isIgnoreChangeFeed = false);
    Task<string> UpsertDriverLocation(DriverLocationItem driver, bool isIgnoreChangeFeed = false);
    Task<List<DriverLocationItem>> RetrieveDriverLocations(string code, int max = Constants.MAX_RETRIEVE_DOCS);
    Task DeleteDriver(string code);

    // Trips
    Task<TripItem> RetrieveTrip(string code);
    Task<List<TripItem>> RetrieveTrips(int max = Constants.MAX_RETRIEVE_DOCS);
    Task<List<TripItem>> RetrieveTrips(double latitude, double longitude, double miles, int max = Constants.MAX_RETRIEVE_DOCS);
    Task<List<TripItem>> RetrieveActiveTrips(int max = Constants.MAX_RETRIEVE_DOCS);
    Task<int> RetrieveTripsCount();
    Task<int> RetrieveActiveTripsCount();
    Task<TripItem> UpsertTrip(TripItem trip, bool isIgnoreChangeFeed = false);
    Task DeleteTrip(string code);

    // High-level methods
    Task<TripItem> AssignTripAvailableDrivers(TripItem trip, List<DriverItem> drivers);
    Task<TripItem> AssignTripDriver(TripItem trip, string driverCode);
    Task RecycleTripDriver(TripItem trip);
    Task<TripItem> CheckTripCompletion(TripItem trip);
    Task<TripItem> AbortTrip(TripItem trip);
}
```

- To make things testable, the Functions are only a wrapper around the PersistenceLayer. Here is an example:

```csharp
[FunctionName("GetTrips")]
public static async Task<IActionResult> GetTrips([HttpTrigger(AuthorizationLevel.Function, "get", Route = "trips")] HttpRequest req,
    ILogger log)
{
    log.LogInformation("GetTrips triggered....");

    try
    {
        var persistenceService = ServiceFactory.GetPersistenceService();
        return (ActionResult)new OkObjectResult(await persistenceService.RetrieveTrips());
    }
    catch (Exception e)
    {
        var error = $"GetTrips failed: {e.Message}";
        log.LogError(error);
        return new BadRequestObjectResult(error);
    }
}
```
- The `PersistenceService` accepts an `IChangeNotifierService` as one of its dependencies. The purpose of this service is to handle entity changes:

```csharp
public interface IChangeNotifierService
{
    Task DriverChanged(DriverItem driver);
    Task TripCreated(TripItem trip, int activeTrips);
    Task TripDeleted(TripItem trip);
    Task PassengerChanged(PassengerItem trip);
}
```

When a trip is added, for example, the change notifier service implementation triggers the `TripManagerOrchestrator` so it creates and assigns a new instance to manage the newly created trip. 

In addition, depending on whether the newly created trip is `normal` or `demo` mode, the change notifier service might trigger the `TripDemoOrchestrator` so it creates and assigns a new instance to mimik a demo/robot behavior such as accepting a driver, stepping through a driver route until the final destination is reached. More explanation about this in the [Durable Orchestrators](#durable-orchestrators) section:

```csharp
public async Task TripCreated(TripItem trip, int activeTrips)
{
    var error = "";

    try
    {
        // Start a trip manager 
        var baseUrl = _settingService.GetStartTripManagerOrchestratorBaseUrl();
        var key = _settingService.GetStartTripManagerOrchestratorApiKey();
        if (string.IsNullOrEmpty(baseUrl) || string.IsNullOrEmpty(key))
            throw new Exception("Trip manager orchestrator base URL and key must be both provided");

        // Trigger the trip manager orchestrator
        // ...omitted for brevity

        if (trip.Type == TripTypes.Demo)
        {
            // Trigger the trip demo orchestrator
            // ...omitted for brevity
        }
    }
    catch (Exception ex)
    {
        error = $"Error while starting the trip manager: {ex.Message}";
        throw new Exception(error);
    }
    finally
    {
        _loggerService.Log($"{LOG_TAG} - TripCreated - Error: {error}");
    }
}
```

## Durable Orchestrators

Durable Orchestrators are the heart of the solution. They are made up of 3 orchestrators:

- Trip Manager
- Trip Monitor
- Trip Demo (optional)

In the RideShare solution, orchestrators are like Serverless [Actors](https://en.wikipedia.org/wiki/Actor_model). They are stateful instances running in the Azure Functions container and made persistent to a storage account automatically. Read more about [Azure Functions Durable Functions](https://docs.microsoft.com/en-us/azure/azure-functions/durable-functions-overview).  

Each orchestrator has 3 sections:

- HTTP Trigger Endpoints - used to start, terminate and retrieve state of a particular orchestrator instance.    
- Orchestrator Function - used to provide the orchestrator main body of execution and state management.
- Orchestrator Activity Functions - one or more activity functions that the orchestrator calls upon to run the different activities that make up the execution.

To make functions easily identifiable, the reference implementation follows a naming convention where the Trigger Functions start with a `T_` i.e. `T_StartTripManager`, the Orchestrator Functions start with an `O_` i.e. `O_ManageTrip` and the Activity Functions start with an `A_` and a 2-digit identifier i.e. `A_TM_AssignTripDriver`. The `_TM_` denotes Trip Manager, for example.

Orchestrator instances require application-level unique instance IDs. In the reference implementation, the Trip code is used as an instance ID for the Trip Manager. The Trip Monitor uses the trip code and appends `-M` to make it unique while the Trip Demo uses the trip code and appends `-D` to make it unique.

As the macro architecture depicts, the orchestrators are implemented in C#. The following illustrates their overall architecture:

![Orchestrators Architecture](media/orchestrators-architecture.png)

The following describes the process that newly created trips go through:

- The `ChangeNotifierService` triggers the Trip Manager Orchestrator to start a new Trip Manager instance.
- The new instance retrieves the available drivers (available & within x miles from trip's source location) and notifies them of a new trip
- The instance then waits for either an external event to arrive (driver accepts the trip) or time out to occur.
- If time out occurs, the instance aborts the trip and exits
- If a driver accepts the trip, the instance assigns the driver to the trip and enqueues the trip code to a storage queue
- The storage queue will trigger the Trip Monitor Orchestrator to start a new Trip Monitor instance.
- The new monitor instance starts the trip and waits for a configurable seconds
- The  instance checks for a completion and re-waits until either the trip completes or the the configured number of iterations gets exhausted
- If the number of iterations is exhausted, the instance will abort the trip
- If the trip is in demo mode, the `ChangeNotifierService` triggers both the Trip Manager Orchestrator and the Trip Demo Orchestrator to start new instances

![Orchestrators Architecture (demo)](media/orchestrators-double-architecture.png)

- The `Trip Demo` instance acts like a bot to simulate accepting a driver and navigating through the locations of a random route

![Orchestrators Demo Architecture](media/orchestrators-demo-architecture.png)

**Please note** that, in the the reference implementation:

- The trip is considered `complete` if the trip's driver location matches the trip's destination location. While this is not realistic, it does provide a method to determine when the trip is complete. In reality though, there has to be a more reliable way of determining completion.
- The orchestrators currently use the persistence layer (described above) instead of calling the APIs to retrieve and persist trips. There is a setting in the `ISettingService` that controls this behavior i.e. `IsPersistDirectly`. More about this in the [source code](#source-code-structure) section.
- The route locations that the `Demo` uses to step through the trip's source and destination locations is not really. It is basically the random number of locations made up from the trip's source location and destination location. In real scenarios, [Bing Route API](https://msdn.microsoft.com/en-us/library/ff701717.aspx?f=255&MSPPError=-2147217396) can be used to determine the actual route between the source and destination.      

The Azure Durable Functions are quite powerful as they provide a way to instantiate thousands of managed stateful instances in a serverless environment. This capability exists in other Azure products such as [Service Fabric](https://azure.microsoft.com/en-us/services/service-fabric/)'s stateful actors. The difference is that the Azure Durable Functions require a lot less effort to setup, maintain and code.

Although Azure Durable Functions can [query and enumerate all instances](https://docs.microsoft.com/en-us/azure/azure-functions/durable-functions-instance-management) of a specific orchestrator:

```csharp
IList<DurableOrchestrationStatus> instances = await context.GetStatusAsync(); // You can pass CancellationToken as a parameter.
foreach (var instance in instances)
{
    log.Info(JsonConvert.SerializeObject(instance));
};
```

it is still probably a good idea to store the instance ids and their status in a table storage for example in case a solution requires special querying capability against the instances.

## Event Grid

The durable orchestrators externalize the trip at the following events:

```csharp
    // Event Grid Event Subjects
    public const string EVG_SUBJECT_TRIP_DRIVERS_NOTIFIED = "Drivers notified!";
    public const string EVG_SUBJECT_TRIP_DRIVER_PICKED = "Driver picked :-)";
    public const string EVG_SUBJECT_TRIP_STARTING = "Trip starting :-)";
    public const string EVG_SUBJECT_TRIP_RUNNING = "Trip running...";
    public const string EVG_SUBJECT_TRIP_COMPLETED = "Trip completed :-)";
    public const string EVG_SUBJECT_TRIP_ABORTED = "Trip aborted :-(";
}
```

The trips are externalized to an [Event Grid Topic](https://docs.microsoft.com/en-us/azure/event-grid/overview). The key advantages of the Event Grid Topic are:

- The emitter fires and forgets. No need to wait until a response arrives. 
- Events can be delivered to multiple listeners that can process the event data.
- Events have data and meta data such as subject that can be used to determine processing. For example, the `PowerBI Trip Processor filters out events based on subject.

As shown in the macro architecture section, the solution implements several listeners for the trip:

![Event Grid Listeners](media/event-grid-listeners.png)

#### Notification Handler via a Logic App

[Logic Apps](https://azure.microsoft.com/en-us/services/logic-apps/) provide a special trigger for Event Grids. When selected, the connector handles all the things needed to provide the web hook required to subscribe to the event grid topic. Please refer to the [setup](./setup.md) to see how to set this up. 

In the reference implementation, the logic app is triggered by the Event Grid topic to send an Email to admins about the trip:

![Logic App Listener](media/logic-app-listener.png)

**Please note** that the [Logic Apps](https://azure.microsoft.com/en-us/services/logic-apps/) Event Grid trigger seems to only expose the event's meta data ...not its data.  

#### SignalR Handler

Azure Functions provide a special binding `EventGridEvent` which makes receiving an Event Grid event a breeze:

```csharp
[FunctionName("EVGH_TripExternalizations2SignalR")]
public static async Task ProcessTripExternalizations2SignalR([EventGridTrigger] EventGridEvent eventGridEvent,
    ILogger log)
{
    log.LogInformation($"ProcessTripExternalizations2SignalR triggered....EventGridEvent" +
                    $"\n\tId:{eventGridEvent.Id}" +
                    $"\n\tTopic:{eventGridEvent.Topic}" +
                    $"\n\tSubject:{eventGridEvent.Subject}" +
                    $"\n\tType:{eventGridEvent.EventType}" +
                    $"\n\tData:{eventGridEvent.Data}");

    try
    {
        TripItem trip = JsonConvert.DeserializeObject<TripItem>(eventGridEvent.Data.ToString());
        if (trip == null)
            throw new Exception("Trip i snull!");

        log.LogInformation($"ProcessTripExternalizations2SignalR trip code {trip.Code}");

        //Do something with the trip...
    }
    catch (Exception e)
    {
        var error = $"ProcessTripExternalizations2SignalR failed: {e.Message}";
        log.LogError(error);
        throw e;
    }
}
```

**Please note** that, in the reference implementation, `EVGH_` is added to the function name that handles an Event Grid event i.e. `EVGH_TripExternalizations2SignalR`.

TBA - Details 

#### PowerBI Handler

Similar to the SignalR handler above, the PowerBI event grid handler follows the same convention:

```csharp
[FunctionName("EVGH_TripExternalizations2PowerBI")]
public static async Task ProcessTripExternalizations2PowerBI([EventGridTrigger] EventGridEvent eventGridEvent,
    ILogger log)
{
    log.LogInformation($"ProcessTripExternalizations2PowerBI triggered....EventGridEvent" +
                    $"\n\tId:{eventGridEvent.Id}" +
                    $"\n\tTopic:{eventGridEvent.Topic}" +
                    $"\n\tSubject:{eventGridEvent.Subject}" +
                    $"\n\tType:{eventGridEvent.EventType}" +
                    $"\n\tData:{eventGridEvent.Data}");

    try
    {
        TripItem trip = JsonConvert.DeserializeObject<TripItem>(eventGridEvent.Data.ToString());
        if (trip == null)
            throw new Exception("Trip is null!");

        log.LogInformation($"ProcessTripExternalizations2PowerBI trip code {trip.Code}");

        if (eventGridEvent.Subject == Constants.EVG_SUBJECT_TRIP_ABORTED ||
            eventGridEvent.Subject == Constants.EVG_SUBJECT_TRIP_COMPLETED)
        {
            var archiveService = ServiceFactory.GetArchiveService();
            await archiveService.UpsertTrip(trip);

            var powerBIService = ServiceFactory.GetPowerBIService();
            await powerBIService.UpsertTrip(trip);
        }
    }
    catch (Exception e)
    {
        var error = $"ProcessTripExternalizations2PowerBI failed: {e.Message}";
        log.LogError(error);
        throw e;
    }
}
```
The handler only cares about the `completed` and `aborted` trip events. It then calls upon the SQL archive service to persist the data to Azure SQL database. Please see [Data storage](#data-storage) for more details on the SQL Database. 

Once in SQL, the data can be used to construct a PowerBI report to provide different performance indicators:
- Total Trips 
- Average Trip Duration
- Top Drivers
- Top Pasengers
- Average Available Drivers
- Etc

This is a sample PowerBI report against test trip data:

![Sample PowerBI Trip Report](media/sample-trip-powerbi-report.png)

In addition, the handler sends trip information to the PowerBI Service which, if configured, sends it to a streaming dataset so real-time trip data can be displayed in a PowerBI dashboard. This is great for product launches but it is outside the scope of this reference implementation.    

#### Archiver Handler

TBA

## Data storage

Relecloud decided to use [Cosmos]() as the main data storage for the solution emtities. With globally distributed capability, Comos can meet the scaling requirement of RideShare. 

**Please note** that the Cosmos `Main` and `Archive` collections used in the reference implementation use fixed data size and the minimum 400 RUs without a partition key. Obviously this needs to be better addressed in a real solution. 

Relecloud also decided to use Azure SQL Database to persist trip summaries so they can be reported on in PowerBI, for example. To this end, the solution defines a `TripFact` table to store the trip flat summaries. Please refer to the [setup](./setup.md) to learn how you provision it.

## Source Code Structure

### .NET

The .NET solution conists of 7 projects:

![.NET Source](media/dotnet-source-structure.png)

- The `Models` project defines all the model classes required by RideShare 
- The `Shared` project contains all the services which are used by the functions to provide different functionality 
- The `Seeder` project contains some integration tests to pump trips through the solution 
- The `Drivers` Function App project contains the `Drivers` APIs 
- The `Trips` Function App project contains the `Trips` APIs 
- The `Passengers` Function App project contains the `Passengers` APIs 
- The `Orchestrators` Function App project contains the `Orchestrators` 

The following are some notes about the source code:

- The concept of `ServiceFactory` is used to create static singleton instances:

```csharp
private static ISettingService _settingService = null;

public static ISettingService GetSettingService()
{
    if (_settingService == null)
    _settingService = new SettingService();

    return _settingService;
}
```

- The `ISettingService` service implementation is used to read settings:

```csharp
var seconds = _settingService.GetTripMonitorIntervalInSeconds();
var maxIterations = _settingService..GetTripMonitorMaxIterations();
```

- The `ILoggerService` service implementation sends traces, exceptions, custom events and metrics to the `Application Insights` reosurce associated with the Functions App:

```csharp
    // Send a trace 
    _loggerService.Log($"{LOG_TAG} - TripCreated - Error: {error}");

    // Send an event telemetry
    _loggerService.Log("Trip created", new Dictionary<string, string>
    {
        {"Code", trip.Code },
        {"Passenger", $"{trip.Passenger.FirstName} {trip.Passenger.LastName}" },
        {"Destination", $"{trip.Destination.Latitude} - {trip.Destination.Longitude}" },
        {"Mode", $"{trip.Type}" }
    });

    // Send a metric telemetry
    _loggerService.Log("Active trips", activeTrips);
```

- `IPersistenceService` has two implementations: `CosmosPersistenceService` and `SqlPersistenceService`. The Cosmos implementation is complete and used in the APIs while the SQL implementation is partially implemented and only used in the `TripExternalizations2PowerBI` handler to persist trip summaries to SQL. 
- The `CosmosPersistenceService` assigns Cosmos ids manually which is a combination of the `collection type` and some identifier. Cosmos's `ReadDocumentAsync` retrieves really fast if an `id` is provided. 
- The `IsPersistDirectly` setting is used mainly by the orchestrators to determine whether to communicte with the storage directly (via the persistence layer) or whether to use the exposed APIs to retrieve and update. In the reference implementation, the `IsPersistDirectly` setting is set to true. 

### Node

TBA

### Web

TBA

## Integration Testing

The .NET `ServerlessMicroservices.Seeder` project contains a multi-thread tester that can be used to submit `demo` trip requests against the `Trips` API. The test will simulate load on the deployed solution and test end-to-end. 

**Please note** that the test will usually run against a dev environment where the `AuthEnabled` setting is set to false.

The `Seeder` main `Program` takes 3 arguments i.e. `seeder.exe url 2 60`

- Test Parameters URL to read the test data from. 
- Optional: # of iterations. Default to 1. 
- Optional: # of seconds to delay between each iteration. Default to 60.

The Test Parameters URL is the `RetrieveTripTestParameters` endpoint defined in the Trips API Function App. It reads test parameters stored in blob storage i.e. . The blob storage is written to by the `StoreTripTestParameters` endpoint defined in the Trips API Function App. 

The following is a sample POST payload the `StoreTripTestParameters` API i.e. `https://<your-trips-function-api>.azurewebsites.net/api/triptestparameters?code=<your code>`. It defines 4 trips to run simulatenously:

```json
[
	{
		"url": "https://<your-trips-function-app>.azurewebsites.net/api/trips?code=<your code>",
		"passengerCode": "bsam@gmail.com",
		"passengerFirstName": "Bill",
		"passengerLastName": "Sam",
		"PassengerMobile": "50551000",	
		"PassengerEmail": "bsam@gmail.com",	
		"sourceLatitude": 31,	
		"sourceLongitude": 50,	
		"destinationLatitude": 32,	
		"destinationLongitude": 60
	},
	{
		"url": "https://<your-trips-function-app>.azurewebsites.net/api/trips?code=<your code>",
		"passengerCode": "krami@gmail.com",
		"passengerFirstName": "Kurt",
		"passengerLastName": "Ramo",
		"PassengerMobile": "505551515",	
		"PassengerEmail": "krami@gmail.com",	
		"sourceLatitude": 28,	
		"sourceLongitude": 40,	
		"destinationLatitude": 33,	
		"destinationLongitude": 51
	},
	{
		"url": "https://<your-trips-function-app>.azurewebsites.net/api/trips?code=<your code>",
		"passengerCode": "sjones@gmail.com",
		"passengerFirstName": "Smith",
		"passengerLastName": "Jones",
		"PassengerMobile": "50551102",	
		"PassengerEmail": "sjones@gmail.com",	
		"sourceLatitude": 31,	
		"sourceLongitude": 50,	
		"destinationLatitude": 32,	
		"destinationLongitude": 60
	},
	{
		"url": "https://<your-trips-function-app>.azurewebsites.net/api/trips?code=<your code>",
		"passengerCode": "rita_ghana@gmail.com",
		"passengerFirstName": "Rita",
		"passengerLastName": "Ghana",
		"PassengerMobile": "505556156",	
		"PassengerEmail": "rita_ghana@gmail.com",	
		"sourceLatitude": 28,	
		"sourceLongitude": 40,	
		"destinationLatitude": 33,	
		"destinationLongitude": 51
	}
]
``` 

Please note the following about the `Seeder` test:

- Since the tester loads the test parameters from a URL, the test parameters can be varied independently without having to re-compile the code. 
- Since each test paramer defines the URL to submit trip requests to, production and dev environments can be tested at the same time.

One way to verify that the test ran successfully is to query the trip summaries in the `TripFact` table for the number of entries after the test ran:
```sql
SELECT * FROM dbo.TripFact
```

The number of entries should match the number of submitted trips. Let us say, for example, we started the test with the test parameters shown above: `Seeder.exe url 2 60`. This means that the test will run for 2 iterations submitting 4 trips in each itertaion. Therefore we expect to see 8 new entries in the `TripFact` table.   

The following is a sample tester output for 2 iterations:

```
Iteration 0 starting....
TestTripRunner - Url https://ridesharetripsfunctionappdev.azurewebsites.net/api/trips?code=rtTQCEXCzUvrw0l28oCfZjhxkIMDeIyQWWj2NFuLxYbld/OwGdZ9aA== started....
TestTripRunner - Simulate a little delay....
TestTripRunner - Url https://ridesharetripsfunctionappdev.azurewebsites.net/api/trips?code=rtTQCEXCzUvrw0l28oCfZjhxkIMDeIyQWWj2NFuLxYbld/OwGdZ9aA== started....
TestTripRunner - Simulate a little delay....
TestTripRunner - Url https://ridesharetripsfunctionappdev.azurewebsites.net/api/trips?code=rtTQCEXCzUvrw0l28oCfZjhxkIMDeIyQWWj2NFuLxYbld/OwGdZ9aA== started....
TestTripRunner - Simulate a little delay....
TestTripRunner - Url https://ridesharetripsfunctionappdev.azurewebsites.net/api/trips?code=rtTQCEXCzUvrw0l28oCfZjhxkIMDeIyQWWj2NFuLxYbld/OwGdZ9aA== started....
TestTripRunner - Simulate a little delay....
TestTripRunner - Passenger Code: sjones@gmail.com ....
TestTripRunner - Passenger Code: rita_ghana@gmail.com ....
TestTripRunner - Passenger Code: bsam@gmail.com ....
TestTripRunner - Passenger Code: krami@gmail.com ....
TestTripRunner - submitted in 15.02846 seconds.
TestTripRunner - submitted in 18.5976287 seconds.
TestTripRunner - submitted in 11.6632886 seconds.
TestTripRunner - submitted in 17.1535626 seconds.
Thread 0 => Duration: 17.1535626 - Error:
Thread 1 => Duration: 11.6632886 - Error:
Thread 2 => Duration: 15.02846 - Error:
Thread 3 => Duration: 18.5976287 - Error:
All tasks are finished.
Iteration 0 completed
Delaying for 60 seconds before starting iteration 1....
Iteration 1 starting....
TestTripRunner - Url https://ridesharetripsfunctionappdev.azurewebsites.net/api/trips?code=rtTQCEXCzUvrw0l28oCfZjhxkIMDeIyQWWj2NFuLxYbld/OwGdZ9aA== started....
TestTripRunner - Simulate a little delay....
TestTripRunner - Url https://ridesharetripsfunctionappdev.azurewebsites.net/api/trips?code=rtTQCEXCzUvrw0l28oCfZjhxkIMDeIyQWWj2NFuLxYbld/OwGdZ9aA== started....
TestTripRunner - Simulate a little delay....
TestTripRunner - Url https://ridesharetripsfunctionappdev.azurewebsites.net/api/trips?code=rtTQCEXCzUvrw0l28oCfZjhxkIMDeIyQWWj2NFuLxYbld/OwGdZ9aA== started....
TestTripRunner - Simulate a little delay....
TestTripRunner - Url https://ridesharetripsfunctionappdev.azurewebsites.net/api/trips?code=rtTQCEXCzUvrw0l28oCfZjhxkIMDeIyQWWj2NFuLxYbld/OwGdZ9aA== started....
TestTripRunner - Simulate a little delay....
TestTripRunner - Passenger Code: krami@gmail.com ....
TestTripRunner - Passenger Code: bsam@gmail.com ....
TestTripRunner - submitted in 1.3980593 seconds.
TestTripRunner - submitted in 1.2487726 seconds.
TestTripRunner - Passenger Code: rita_ghana@gmail.com ....
TestTripRunner - submitted in 1.3474113 seconds.
TestTripRunner - Passenger Code: sjones@gmail.com ....
TestTripRunner - submitted in 1.3841847 seconds.
Thread 0 => Duration: 1.2487726 - Error:
Thread 1 => Duration: 1.3980593 - Error:
Thread 2 => Duration: 1.3841847 - Error:
Thread 3 => Duration: 1.3474113 - Error:
All tasks are finished.
Iteration 1 completed
Test is completed. Press any key to exit...
```

## Monitoring

[Application Insights](https://azure.microsoft.com/en-us/services/application-insights/) and [Azure Dashboard](https://azure.microsoft.com/en-us/updates/upload-download-azure-dashboards/) are great resources to monitor a solution in production. One can pin response time, requests and failure requests from the solution Applicaion Insights resource right into the Azure Dashboard:

![Server Telemetry](media/app-insights-normal.png)

In addition, one can also create custom queries and pin their results to the dashboard as well. For example, the following is an analytic query that shows the distribution of custom events (sent from the code) in the last 24 hours:

```sql
customEvents
| where timestamp > ago(24h) 
| summarize count() by name
| render piechart  
```

The result shows the distribution of trip different stages:

![Trip Stages](media/app-insights-custom-events.png)

Custom metrics are sent from the solution to the Application Insights resources to denote a metric value. In fact, if an Application Insights is attacched to a Function App, the Azure Functions framework automatically sends `Count`, `AvgDurationMs`, `MaxDurationMs`, `MinDurationMs`, `Failures`, `Successes` and `SuccessRate` custom metrics for each function i.e. trigger, orchestrator or activity. 

The following is an anaytic query that shows in a pie chart the occurrences of the following two custom metrics in the last 24 hours: `Active trips` and `O_MonitorTrip`:

```sql
customMetrics
| where timestamp > ago(24h) 
| where name == "Active trips" or name contains "O_MonitorTrip"   
| summarize count() by name
| render piechart 
```
The result shows the distribution of the above 2 custom metrics:

![Custom Metrics](media/app-insights-custom-metrics.png)

## Deployment

Function App deployments can happen from [Visual Studio]() IDE, [Visual Studio Team Services](https://visualstudio.microsoft.com/vso/) by defining a build pipeline that can be triggered upon push to the code repository, for example, or a build script such as [Cake](https://cakebuild.net/) or [Psake](https://github.com/psake/psake).

Relecloud decided to use [Visual Studio team Services](https://visualstudio.microsoft.com/vso/) for production build and deployment and [Cake](https://cakebuild.net/) for development build and deployment.

### VSTS 

TBA
Function Apps
Web App

### Cake for C# Function Apps

The `Cake` script reponsible to `deploy` and `provision` is included in the `dotnet` source directory. In order to run the Cake Script locally and deploy to your Azure Subscription, there are some pre-requisites. Please refer to the [setup](setup.md/#cake) page to know how to do this. 

Make sure that the `settings` directory CSV files are updated as shown in [setup](./setup.md/#update-the-setting-files) to reflect your own resource app settings and connection strings.

Once all of the above is in place, Cake is now able to authenticate and deploy the C# function apps provided that you used the same resource names as defined in [setup](./setup.md/#resources). If this is not the case, you can adjust the `paths.cake` file to match your resource names. 

From a PowerShell command, use the following commands for the `Dev` environment:

- `./build.ps1 -Target Deploy -Configuration Debug -ScriptArgs '--Site=Drivers','--App=ServerlessMicroservices.FunctionApp.Drivers','--Env=Dev'`
- `./build.ps1 -Target Deploy -Configuration Debug -ScriptArgs '--Site=Orchestrators','--App=ServerlessMicroservices.FunctionApp.Orchestrators','--Env=Dev'`
- `./build.ps1 -Target Deploy -Configuration Debug -ScriptArgs '--Site=Trips','--App=ServerlessMicroservices.FunctionApp.Trips','--Env=Dev'`
- `./build.ps1 -Target Deploy -Configuration Debug -ScriptArgs '--Site=Passengers','--App=ServerlessMicroservices.FunctionApp.Passengers','--Env=Dev'`

From a PowerShell command, use the following commands for the `Prod` environment:

- `./build.ps1 -Target Deploy -Configuration Release -ScriptArgs '--Site=Drivers','--App=ServerlessMicroservices.FunctionApp.Drivers','--Env=Prod'`
- `./build.ps1 -Target Deploy -Configuration Release -ScriptArgs '--Site=Orchestrators','--App=ServerlessMicroservices.FunctionApp.Orchestrators','--Env=Prod'`
- `./build.ps1 -Target Deploy -Configuration Release -ScriptArgs '--Site=Trips','--App=ServerlessMicroservices.FunctionApp.Trips','--Env=Prod'`
- `./build.ps1 -Target Deploy -Configuration Release -ScriptArgs '--Site=Passengers','--App=ServerlessMicroservices.FunctionApp.Passengers','--Env=Prod'`
