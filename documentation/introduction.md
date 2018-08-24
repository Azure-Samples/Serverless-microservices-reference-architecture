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

Given the above Micrservice principles, the following are identified as Microservices:

| Microservice | Technology | Reason |
|---|---|---|
|Drivers APIs| C# | The `Drivers` is are code and deloyment independent isolated in a functions app. Hence it is considered a serverless Microservice.|
|Trips APIs| C# | The `Trips` API is code and deloyment independent isolated in a functions app. Hence it is considered a serverless Microservice.|
|Passengers APIs| C# | The `Passengers` API is code and deloyment independent isolated in a functions app. Hence it is considered a serverless Microservice.|
|Durable Orchestartors| C# |The Trip `Manager`, `Monitor` and `Demo` i.e. Orchestrators are independent as they provide the heart of the solution. They need to scale and deploy independently, hence they are a good fit for a serverless Microservice.|
|Event Grid Notification Handler| Logic App | The `Logic App` handler adds value to the overall solution but work independetly. Hence it is considered a serverless Microservice.|
|Event Grid SignalR Handler | C# | The `SignalR` handler adds value to the overall solution but work independently. Hence it is considered a serverless Microservice.|
|Event Grid PowerBI Handler | C# | The `PowerBI` handler adds value to the overall solution but work independently. Hence it is considered a serverless Microservice.|
|Event Grid Archiver | NodeJS | The NodeJS `Archiver` handler adds value to the overall solution but work independently. Hence it is considered a serverless Microservice.|

**Please note** that, due to code layout, some Microservices might be a Function within a Functions App. Examples of this are the `Event Grid SignalR Handler` and `Event Grid PowerBI Handler` Microservices. They are actually part of the `Trips` Functions App.

In the sections below, we will go trough each architecture component in more details. 

## Web App

TBA - Joel

Describe how the SPA communicates with the B2C AD to provide different levels of permissions.

## API Management

There are many benefits to use an API manager. In the case the RideShare solution, there are really four major benefits:

1. **Security**: the API manager layer verifies the incoming requests' [JWT](https://jwt.io/) token against the B2C Authority URL. This is accomplshed via an inbound policy that intercepts each call:

```xml
<validate-jwt header-name="Authorization" failed-validation-httpcode="401" failed-validation-error-message="Unauthorized. Access token is missing or invalid."> 
    <openid-config url="<--your_own_authorization_url-->" />
    <audiences>
        <audience><-- your_own_app_id --></audience>
    </audiences>
</validate-jwt>
```
2. **Documentation**: the API manager provides develpers writing applications against RideShare APIs with a complete development portal for documentation and testing

3. **Usage Stats**: the API manager provides usage stats on all API calls (and report failures) which makes it really convenient to assess the API performance

4. **Rate Limiting**: the API manager can be configured to rate limit APIs based on IP origin, access, etc. This can be useful to prevent DOD attacks or provide different tiers of access based on users. 

**Please note** that, in the case of Azure Functions, while the APIs are front-ended with an API manager (and hence shielded, protected and rate limited), the APIs are still publicly available!!! This means that a DOD attack or other attacks can still happen against the bare APIs if someone discovers them in the wide.    

**Please note** that Relecloud considered using Azure Functions Filters......


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

TBA - Khaled
Why was this selected? What does it do for the solution, and in particular Relecloud's requirements?

TBA - Khaled
How could this be expanded in the future beyond the scope of the sample solution?

TBA - Khaled
What are some potential challenges?

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

To make functions easily identifiable, the reference implementation follows a naming convention where the Trigger Functions start with a `T_` i.e. `T_StartTripManager`, the Orchestrator Functions start with an `O_` i.e. `O_ManageTrip` and the Activity Functions start with an `A_` and a 2-digit inditifier i.e. `A_TM_AssignTripDriver`. The `_TM_` denotes Trip Manager, for example.

Orchestrator instances require application-level unique instance IDs. In the reference implementation, the Trip code is used as an instance ID for the Trip Manager. The Trip Monitor uses the trip code and appends `-M` to make it unique while the Trip Demo uses the trip code and appends `-D` to make it unique.

As the macro architecture depicts, the orchestrators are implemented in C#. The following illustrates their overall architecture:

![Orchestrators Architecture](media/orchestrators-architecture.png)

Please note the following:

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
- The orchestrators currently use the persistence layer (described above) instead of calling the APIs to retrieve and persist trips. There is a setting in the `ISettingService` that controls this behavior i.e. `IsPersistDirectly`. More about this in the source code section.

TBA - Khaled
What makes Durable Functions different than standard ones? What do they solve for Relecloud? Remember, they said this was one of the things that set Azure's FaaS offering apart from the competition.

TBA - Khaled
Explain how Durable Functions are being used in the solution for orchestration. How could what is shown in the sample solution be expanded on? What are some other possible scenarios (briefly touch on fan out/fan in, etc.)

## Event Grid

The durable orchestrators externalize the trip at the following events:

- Drivers Notified
- Driver Picked
- Trip Starting
- Trip Running i.e. an event every x seconds
- Trip Completed
- Trip Aborted

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

As shown in the macro architecture section, the solution impelements several listeners for the trip:

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

**Please note** that, in the reference implementation, `EVGH_` is added to the function name that handles the Event Grid event i.e. `EVGH_TripExternalizations2SignalR`.

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
The handler only cares about the `completed` and `aborted` trip events. It then calls upon the SQL archive service to persist the data to Azure SQL database. Please see [Data storage](#data-storage) for details on the table structure. 

Once in SQL, the data can be used to construct a PowerBI report to provide different performance indicators:
- Trips per month 
- Average trip duration
- Top drivers
- Average trip available drivers
- Etc

TBA - show a sample PowerBI screen shots

In addition, the handler can also send trip information to a PowerBI streaming dataset so real-time trip data can be displayed in a PowerBI dashboard. This is great for product lauches.     

#### Archiver Handler

TBA

## Data storage

Relecloud decided to use Cosmos....

**Please note** that the Cosmos collection used in the reference implementation uses fixed data size and the minimum 400 RUs without a partition key. Obviously this needs to be addressed in a real solution. 

In addition Azure SQL Database is used to persist trip summaries so they can be reportyed on, in PowerBI, for example. 

To this end, the solution defines a `TripFact` table to store the trip flat summaries:

```sql
    USE [RideShare]
    GO

    SET ANSI_NULLS ON
    GO

    SET QUOTED_IDENTIFIER ON
    GO

    CREATE TABLE[dbo].TripFact (
        [Id][int] IDENTITY(1, 1) NOT NULL,
        [StartDate][datetime] NOT NULL,
        [EndDate][datetime] NULL,
        [AcceptDate][datetime] NULL,
        [TripCode] [nvarchar] (20) NOT NULL,
        [PassengerCode] [nvarchar] (20) NULL,
        [PassengerName] [nvarchar] (100) NULL,
        [PassengerEmail] [nvarchar] (100) NULL,
        [AvailableDrivers] [int] NULL,
        [DriverCode] [nvarchar] (20) NULL,
        [DriverName] [nvarchar] (100) NULL,
        [DriverLatitude] [float] NULL,
        [DriverLongitude] [float] NULL,
        [DriverCarMake] [nvarchar] (100) NULL,
        [DriverCarModel] [nvarchar] (100) NULL,
        [DriverCarYear] [nvarchar] (4) NULL,
        [DriverCarColor] [nvarchar] (20) NULL,
        [DriverCarLicensePlate] [nvarchar] (20) NULL,
        [SourceLatitude] [float] NULL,
        [SourceLongitude] [float] NULL,
        [DestinationLatitude] [float] NULL,
        [DestinationLongitude] [float] NULL,
        [Duration] [float] NULL,
        [MonitorIterations] [int] NULL,
        [Status] [nvarchar] (20) NULL,
        [Error] [nvarchar] (200) NULL,
        [Mode] [nvarchar] (20) NULL
        CONSTRAINT[PK_dbo.TripFact] PRIMARY KEY CLUSTERED
    (
        [Id] ASC
    )WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
    )

    GO

    CREATE INDEX IX_TRIP_START_DATE ON dbo.TripFact(StartDate);
    CREATE INDEX IX_TRIP_CODE ON dbo.TripFact(TripCode);
    CREATE INDEX IX_TRIP_PASSENGER_CODE ON dbo.TripFact(PassengerCode);
    CREATE INDEX IX_TRIP_DRIVER_CODE ON dbo.TripFact(DriverCode);
```

TBA - What are some potential challenges?

## Source Code Structure

### .NET

The .NET solution conists of 7 projects:

![.NET Source](media/dotnet-source-structure.png)

- The `Models` project defines all the model classes rquired by RideShare 
- The `Shared` project contains all the services which are used by the functions to provide different functionality 
- The `Seeder` project contains some integration tests to pump trips through the solution 
- The `Drivers` Function App project contains the `Drivers` APIs 
- The `Trips` Function App project contains the `Trips` APIs 
- The `Passengers` Function App project contains the `Passengers` APIs 

Some notes about the source code:

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

- `IPersistenceService` has two implementations: `CosmosPersistenceService` and `SqlPersistenceService`. The Cosmos implementaion is complete and used in the APIs while the SQL implemention is partially implemented and only used in the `TripExternalizations2PowerBI` handler to persist to SQL. 
- The `CosmosPersistenceService` assigns Cosmos ids manually which is a combination of the `collection type` and some identifier. Cosmos's `ReadDocumentAsync` retrieves really fast if an `id` is provided. 

### Node

### Web

## Integration Testing

## Monitoring

## Deployment

## Provision
