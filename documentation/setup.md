# Serverless Microservices reference architecture

In this document:

- [Resources](#resources)
- [Provision](#provision)
    - [Manual via the Portal](#manual-via-the-portal)
        - [Create the Azure function apps](#create-the-azure-apps)
        - [Create the Resource Group](#create-the-resource-group)
        - [Create the Cosmos Assets](#create-the-cosmos-assets)
        - [Create the Storage Account](#create-the-storage-account)
        - [Create the Storage Account](#create-the-storage-account)
        - [Create the Azure function apps](#create-the-azure-apps)
        - [Create the Web App Service Plan](#create-the-web-app-service-plan)
        - [Create the Azure SQL Database Assets](#create-the-azure-sql-database-assets)
        - [Create the Event Grid Topic](#create-the-event-grid-topic)
        - [Create the App Insights Resource](#create-the-app-insights-resource)
        - [Create the Logic App](#create-the-logic-app)
        - [Create the API Management Service](#create-the-api-management-service)
        - [Create the SignalR Service](#create-the-signalr-service)
        - [Create the B2C Tenant](#create-the-b2c-tenant)
    - [ARM Template](#arm-template)
    - [Cake](#cake)
- [Setup](#setup)
    - [Add APIM Products and APIs](#add-apim-products-and-apis)
    - [Connect Event Grid to its listeners](#connect-event-grid-to-its-listeners)
    - [Connect Event Grid to Logic App](#connect-event-grid-to-logic-app)
    - [Create TripFact Table](#create-tripfact-table)
- [Setting Files](#setting-files)
    - [Drivers Function App](#drivers-function-app)
    - [Passengers Function App](#passengers-function-app)
    - [Orchestrators Function App](#orchestrators-function-app)
    - [Trips Function App](#trips-function-app)
- [Build the solution](#build-the-solution)
    - [.NET](#.net)
    - [Node](#node)
    - [Web](#web)
- [Deployment](#deployment)
    - [VSTS](#vsts)
    - [Cake](#cake)

## Resources

The following is a summary of all Azure resources required to deploy the solution:

| Prod Resource Name | Dev Resource Name | Type | Provision Mode |
|---|---|---|:---:|
| serverless-microservices | serverless-microservices-dev | Resource Group | Auto | 
| rideshare | rideshare | Cosmos DB Account | Auto |
| Main | Main | Cosmos DB Collection | Auto |
| Archive | Archive | Cosmos DB Collection | Auto |
| ridesharefunctionstore | ridesharefunctiondev | Storage Account | Auto |
| RideShareFunctionAppPlan | RideShareFunctionAppPlan | Consumtpion Plan | Auto |
| RideShareDriversFunctionApp | RideShareDriversFunctionAppDev | Function App | Auto |
| RideShareTripsFunctionApp | RideShareTripsFunctionAppDev | Function App | Auto |
| RideSharePassengersFunctionApp | RideSharePassengersFunctionAppDev | Function App | Auto |
| RideShareOrchestratorsFunctionApp | RideShareOrchestratorsFunctionAppDev | Function App | Auto |
| RideShareArchiverFunctionApp | RideShareArchiverFunctionAppDev | Function App | Auto |
| RideShareAppServicePlan | RideShareAppServicePlanDev | Web App Service Plan | Auto |
| RelecloudRideshare | RelecloudRideshareDev | Web App Service | Auto |
| rideshare-db | rideshare-db-dev | SQL Database Server | Auto |
| RideShare | RideShare | SQL Database | Auto |
| TripFact | TripFact | SQL Database Table | Manual |
| RideShareTripExternalizations | RideShareTripExternalizationsDev | Event Grid Topic | Manual |
| rideshare | rideshare-dev | App Insights | Manual |
| ProcessTripExternalization | ProcessTripExternalizationDev | Logic App | Manual |
| rideshare | N/A | API Management Service | Manual |
| rideshare | rideshare-dev | SignalR Service | Manual |
| relecloudrideshare.onmicrosoft.com | N/A | B2C Tenant | Manual |

## Provision

There are 3 ways to provision the required resources:

- [Manual via the Portal](#manual-via-the-portal)
- [ARM Template](#arm-template)
- [Cake](#cake)

### Manual via the Portal

#### Create the resource group

#### Create the Cosmos Assets

#### Create the Storage Account

#### Create the Azure function apps

**--FORMAT-- Have intro under each step explaining the concepts, what they're doing, and why. Link to associated section in [Introduction](./introduction.md)**

**-- Show code snippets within a section if appropriate. Like when provisioning Event Grid and creating topics, maybe show code snippet or two from functions where the topics are being used --**

In this step, you will be creating six new function apps in the Azure portal. There are many ways this can be accomplished, such as [publishing from Visual Studio](), [Visual Studio Code](), the [Azure CLI](), Azure [Cloud Shell](), an [Azure Resource Manager (ARM) template](), and through the Azure portal.

Each of these function apps act as a hosting platform for one or more functions. In our solution, they double as microservices with each function serving as an endpoint or method. Having functions distributed amongst multiple function apps enables isolation, providing physical boundaries between the microservices, as well as independent release schedules, administration, and scaling.

1.  Log in to the [Azure portal](https://portal.azure.com).

1.  Type **Function App** into the Search box at the top of the page, then select **Function App** within the Marketplace section.

    ![Type Function App into the Search box](media/function-app-search-box.png 'Function App search')

1.  Complete the function app creation form with the following:

    a. **App name**: Enter a unique value for the **Drivers** function app.
    b. **Subscription**: Select your Azure subscription.
    c. **Resource Group**: Either select an existing Resource Group or create a new one such as "serverless-microservices".
    d. **OS**: Select Windows.
    e. **Hosting Plan**: Select Consumption Plan.
    f. **Location**: Select a region closest to you. Make sure you select the same region for the rest of your resources.
    g. **Storage**: Select Create new and supply a unique name. You will use this storage account for the remaining function apps.
    h. **Application Insights**: Set to Off. We will create an Application Insights instance later that will be associated with all of the Function Apps and other services.

    ![Screenshot of the Function App creation form](media/new-function-app-form.png 'Create Function App form')

1.  Repeat the steps above to create the **Trips** function app.

    a. Enter a unique value for the App name, ensuring it has the word **Trips** within the name so you can easily identify it.
    b. Make sure you enter the same remaining settings and select the storage account you created in the previous step.

1.  Repeat the steps above to create the **Orchestrators** function app.

1.  Repeat the steps above to create the **Passengers** function app.

At this point, your Resource Group should have a list of resources similar to the following:

![List of resources in the Resource Group after creating function apps](media/resource-group-function-apps.png 'Resource Group resource list')

#### Create the Web App Service Plan

#### Create the Web App

#### Create the Azure SQL Database Assets

#### Create the Event Grid Topic

#### Create the App Insights Resource

#### Create the Logic App 

#### Create the API Management Service 

#### Create the SignalR Service 

#### Create the B2C Tenant 

Once completed, please jump to the [setup](#setup) section to continue. 

### ARM Template

Once completed, please jump to the [setup](#setup) section to continue. 

### Cake 

The `Cake` script reponsible to `deploy` and `provision` is included in the `dotnet` source directory. In order to run the Cake Script locally and deploy to your Azure Subscription, there are some pre-requisites:

1. Create a service principal that can be used to authenticate the script to use your Azure subscription. This can be easily accomplished using the following PowerShell script:

```powershell
# Login
Login-AzureRmAccount

# Set the Subscriptions
Get-AzureRmSubscription  

# Set the Subscription to your preferred subscription
Select-AzureRmSubscription -SubscriptionId "<your_subs_id>"

# Create an application in Azure AD
$pwd = convertto-securestring "<your_pwd>" -asplaintext -force
$app = New-AzureRmADApplication  -DisplayName "RideSharePublisher"  -HomePage "http://rideshare" -IdentifierUris "http://rideshare" -Password $pwd

# Create a service principal
New-AzureRmADServicePrincipal -ApplicationId $app.ApplicationId

# Assign role
New-AzureRmRoleAssignment -RoleDefinitionName Contributor -ServicePrincipalName $app.ApplicationId.Guid
```
2. Place two text files in the `dotnet` directory that can tell the Cake script about the service principal that you just created. The two text files are: `dev_authfile.txt` and `prod_authfile.txt`. They contain the following:

```
subscription=<your_subs_id>  
client=<your_client_id_produced_by_ps_above>  
key=<your_pwd_you_set_up_in_ps_above>  
tenant=<your_azure_tenant_id>
managementURI=https\://management.core.windows.net/  
baseURL=https\://management.azure.com/  
authURL=https\://login.windows.net/  
graphURL=https\://graph.windows.net/ 
```

If your `dev` and `prod` environments are hosted on the same Azure subscription, then the two auth files will be identical.

Once the above is completed, from a PowerShell command, use the following command to provision the `Dev` environment:
`./build.ps1 -Target Provision -ScriptArgs '--Env=Dev'`

From a PowerShell command, use the following command to provision the `Prod` environment:
`./build.ps1 -Target Provision -ScriptArgs '--Env=Prod'`

**Please note** that provisiong a Cosmos DB Account takes a long time to be online. If you proceed with creating a database and the colections while the status is `Creating`, you will get an error that says something like `bad request` without much of an explanation. Once the DB Account becomes `Online`, you can continue to provision the rest (by re-invoking the `provision` command). The exact error is: One or more errors occurred. Long running operation failed with status 'BadRequest'.

Unfortunately, the Cake script cannot provision the following resources because they are currently not supported in the [Azure Management Libraries for .NET](https://github.com/Azure/azure-libraries-for-net). So please complete the following provisions manually:

- [Event Grid](#create-the-event-grid-topic)
- [App Insights](#create-the-app-insights-resource)
- [Logic App](#create-the-logic-app)
- [API Manager](#create-the-api-management-service)
- [SignalR Service](#create-the-signalr-service)
- [B2C Tenant](#create-the-b2c-tenant)

Once completed, please jump to the [setup](#setup) section to continue. 

## Setup

After you have provisioned all your resources, there are some manual steps that you need to do to complete the setup 

- [Add APIM Products and APIs](#Add-APIM-Products-and-APIs)
- [Connect Event Grid to its listeners](#connect-event-grid-to-its-listeners)
- [Connect Event Grid to Logic App](#connect-event-grid-to-logic-app) 
- [Run a script to create the TripFact table](#create-tripfact-table)

### Add APIM Products and APIs

### Connect Event Grid to its listeners

### Connect Event Grid to Logic App

### Create TripFact Table 

Connect to the databse and run the following script to create the `TripFact` table and its indices:  

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

## Setting Files

The reference implementation solution requires several settings for each function app. The `settings` directory contains the setting file for each function app. The files are a collection of `KEY` and `VALUE` delimited by a `|`. They need to be imported as `Application Settings` for each function app. Alternatively, the Cake deployment script auto-imports these files into the `Application Settings`.

### Drivers Function App

| KEY | DESCRIPTION |
|---|---|
| APPINSIGHTS_INSTRUMENTATIONKEY | The App Insights Resource Instrumentation Key. This key is required by the Function App so it knows there is an app insights resource associated with it | 
| FUNCTIONS_EXTENSION_VERSION | Must be set to `beta` since the solution uses V2 beta | 
| DocDbApiKey | The Cosmos DB API Key | 
| DocDbEndpointUri | The Cosmos DB Endpoint URI | 
| DocDbRideShareDatabaseName | The Cosmos Database i.e. `RideShare` | 
| DocDbRideShareMainCollectionName | The Cosmos Main Collection i.e. `Main` | 
| DocDbThroughput | The provisioned collection RUs i.e. 400  | 
| InsightsInstrumentationKey | Same value as APPINSIGHTS_INSTRUMENTATIONKEY. This value is used by the Function App while the other is used by the Function framework  | 
| AuthorityUrl | The B2C Authority URL i.e. https://login.microsoftonline.com/tfp/relecloudrideshare.onmicrosoft.com/b2c_1_default-signin/v2.0| 
| ApiApplicationId | The B2C Client ID | 
| ApiScopeName | The Scope Name i.e. rideshare | 
| EnableAuth | if set to true, the JWT token validatidaion will be enforced | 

### Passengers Function App

| KEY | DESCRIPTION |
|---|---|
| APPINSIGHTS_INSTRUMENTATIONKEY | The App Insights Resource Instrumentation Key. This key is required by the Function App so it knows there is an app insights resource associated with it | 
| FUNCTIONS_EXTENSION_VERSION | Must be set to `beta` since the solution uses V2 beta | 
| DocDbApiKey | The Cosmos DB API Key | 
| DocDbEndpointUri | The Cosmos DB Endpoint URI | 
| DocDbRideShareDatabaseName | The Cosmos Database i.e. `RideShare` | 
| DocDbRideShareMainCollectionName | The Cosmos Main Collection i.e. `Main` | 
| DocDbThroughput | The provisioned collection RUs i.e. 400  | 
| InsightsInstrumentationKey | Same value as APPINSIGHTS_INSTRUMENTATIONKEY. This value is used by the Function App while the other is used by the Function framework  | 
| AuthorityUrl | The B2C Authority URL i.e. https://login.microsoftonline.com/tfp/relecloudrideshare.onmicrosoft.com/b2c_1_default-signin/v2.0| 
| ApiApplicationId | The B2C Client ID | 
| ApiScopeName | The Scope Name i.e. rideshare | 
| EnableAuth | if set to true, the JWT token validatidaion will be enforced | 
| GraphTenantId| Azure Tenant ID |
| GraphClientId| Azure Graph client ID |
| GraphClientSecret| Azure Graps secret |

### Orchestrators Function App

| KEY | DESCRIPTION |
|---|---|
| APPINSIGHTS_INSTRUMENTATIONKEY | The App Insights Resource Instrumentation Key. This key is required by the Function App so it knows there is an app insights resource associated with it | 
| FUNCTIONS_EXTENSION_VERSION | Must be set to `beta` since the solution uses V2 beta | 
| DocDbApiKey | The Cosmos DB API Key | 
| DocDbEndpointUri | The Cosmos DB Endpoint URI | 
| DocDbRideShareDatabaseName | The Cosmos Database i.e. `RideShare` | 
| DocDbRideShareMainCollectionName | The Cosmos Main Collection i.e. `Main` | 
| DocDbThroughput | The provisioned collection RUs i.e. 400  | 
| InsightsInstrumentationKey | Same value as APPINSIGHTS_INSTRUMENTATIONKEY. This value is used by the Function App while the other is used by the Function framework  | 
| DriversAcknowledgeMaxWaitPeriodInSeconds |The number of seconds to wait before the solution times out waiting for drivers to accept a trip i.e. 120|
| DriversLocationRadiusInMiles |The miles radius that the solution locates available drivers within i.e. 15|
| TripMonitorIntervalInSeconds | The number of seconds the `TripMonitor` waits in its monitoring loop i.e. 10 |
| TripMonitorMaxIterations |The number of maximum iterations the `TripMonitor` loops before it aborts the trip i.e. 20|
| IsPersistDirectly| If true, the orechestrators access the data storage layer directly. Default to true |
| TripExternalizationsEventGridTopicUrl| The URL of the event grid topic i.e. https://ridesharetripexternalizations.eastus-1.eventgrid.azure.net/api/events|
| TripExternalizationsEventGridTopicApiKey|The API Key of the event grid topic |


### Trips Function App

| KEY | DESCRIPTION |
|---|---|
| APPINSIGHTS_INSTRUMENTATIONKEY | The App Insights Resource Instrumentation Key. This key is required by the Function App so it knows there is an app insights resource associated with it | 
| FUNCTIONS_EXTENSION_VERSION | Must be set to `beta` since the solution uses V2 beta | 
| DocDbApiKey | The Cosmos DB API Key | 
| DocDbEndpointUri | The Cosmos DB Endpoint URI | 
| DocDbRideShareDatabaseName | The Cosmos Database i.e. `RideShare` | 
| DocDbRideShareMainCollectionName | The Cosmos Main Collection i.e. `Main` | 
| DocDbThroughput | The provisioned collection RUs i.e. 400  | 
| InsightsInstrumentationKey | Same value as APPINSIGHTS_INSTRUMENTATIONKEY. This value is used by the Function App while the other is used by the Function framework  | 
| AuthorityUrl | The B2C Authority URL i.e. https://login.microsoftonline.com/tfp/relecloudrideshare.onmicrosoft.com/b2c_1_default-signin/v2.0| 
| ApiApplicationId | The B2C Client ID | 
| ApiScopeName | The Scope Name i.e. rideshare | 
| EnableAuth | if set to true, the JWT token validatidaion will be enforced | 
| SqlConnectionString | The connection string to the Azure SQL Databse where `TripFact` is provisioned  | 
| SqlConnectionString | The connection string to the Azure SQL Databse where `TripFact` is provisioned  | 
| StartTripManagerOrchestratorApiKey|The Start Trip Manager Orchestrator trigger endpoint function code key |
| StartTripManagerOrchestratorBaseUrl|The Start Trip Manager Orchestrator trigger endpoint function base url |
| StartTripDemoOrchestratorApiKey|The Trip Start Demo Orchestrator trigger endpoint function code key |
| StartTripDemoOrchestratorBaseUrl|The Trip Start Demo Orchestrator trigger endpoint function base url |
| TerminateTripManagerOrchestratorApiKey|The Terminate Trip Manager Orchestrator trigger endpoint function code key |
| TerminateTripManagerOrchestratorBaseUrl|The Trip Manager Orchestrator trigger endpoint function base url |
| TerminateTripMonitorOrchestratorApiKey|The Terminate Trip Demo Orchestrator trigger endpoint function code key |
| TerminateTripMonitorOrchestratorBaseUrl|The Trip Terminate Demo Orchestrator trigger endpoint function base url |

## Build the solution

### .NET

Pre-requisites:

- VS2017 15.7 or later
- .NET Core 2.1 SDK Installed

### Node

### Web

## Deployment

Function App deployments can happen from [Visual Studio]() IDE, [Visual Studio Team Services](https://visualstudio.microsoft.com/vso/) by defining a build pipeline that can be triggered upon push to the code repository, for example, or a build script such as [Cake](https://cakebuild.net/) or [Psake](https://github.com/psake/psake).

Relecloud decided to use [Visual Studio team Services](https://visualstudio.microsoft.com/vso/) for production build and deployment and [Cake](https://cakebuild.net/) for development build and deployment.

### VSTS 

TBA
Function Apps
Web App

### Cake

The `Cake` script reponsible to `deploy` and `provision` is included in the `dotnet` source directory. In order to run the Cake Script locally and deploy to your Azure Subscription, there are some pre-requisites. Please refer to the [Cake](#cake) provision section to know how to do this. 

Make sure the `settings` are updated as shown in [Setting Files](#setting-files) section to reflect your own resource app settings and connection strings.

Once all of the above is in place, Cake is now able to authenticate and deploy the C# function apps provided that you used the same resource names as defined in [resources](#resources) section. If this is not the case, you can adjust the `paths.cake` file to match your resource names. 

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


