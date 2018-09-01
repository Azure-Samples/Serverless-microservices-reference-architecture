///////////////////////////////////////////////////////////////////////////////
// TOOLS
///////////////////////////////////////////////////////////////////////////////

///////////////////////////////////////////////////////////////////////////////
// ADDINS
///////////////////////////////////////////////////////////////////////////////

#addin nuget:?package=Microsoft.Net.Http
#addin nuget:?package=Newtonsoft.Json
#addin nuget:?package=Microsoft.VisualBasic
#addin nuget:?package=Microsoft.Azure.KeyVault.Core&version=1.0.0.0
#addin nuget:?package=Microsoft.Data.Services.Client&version=5.8.1.0
#addin nuget:?package=Microsoft.Data.OData&version=5.8.1.0
#addin nuget:?package=Microsoft.Data.Edm&version=5.8.1.0
#addin nuget:?package=System.Spatial&version=5.8.1.0
#addin nuget:?package=WindowsAzure.Storage&version=8.1.4.0
#addin nuget:?package=Microsoft.IdentityModel.Clients.ActiveDirectory&version=2.28.3
#addin nuget:?package=Microsoft.Rest.ClientRuntime&version=2.3.9
#addin nuget:?package=Microsoft.Rest.ClientRuntime.Azure&version=3.3.10
#addin nuget:?package=Microsoft.Rest.ClientRuntime.Azure.Authentication&version=2.3.2
#addin nuget:?package=Microsoft.Azure.DocumentDB
#addin nuget:?package=Microsoft.Azure.Management.Fluent&version=1.14.0
#addin nuget:?package=Microsoft.Azure.Management.Storage.Fluent&version=1.14.0
#addin nuget:?package=Microsoft.Azure.Management.KeyVault.Fluent&version=1.14.0
#addin nuget:?package=Microsoft.Azure.Management.Graph.RBAC.Fluent&version=1.14.0
#addin nuget:?package=Microsoft.Azure.Management.Compute.Fluent&version=1.14.0
#addin nuget:?package=Microsoft.Azure.Management.Network.Fluent&version=1.14.0
#addin nuget:?package=Microsoft.Azure.Management.Batch.Fluent&version=1.14.0
#addin nuget:?package=Microsoft.Azure.Management.TrafficManager.Fluent&version=1.14.0
#addin nuget:?package=Microsoft.Azure.Management.Dns.Fluent&version=1.14.0
#addin nuget:?package=Microsoft.Azure.Management.Sql.Fluent&version=1.14.0
#addin nuget:?package=Microsoft.Azure.Management.Redis.Fluent&version=1.14.0
#addin nuget:?package=Microsoft.Azure.Management.Cdn.Fluent&version=1.14.0
#addin nuget:?package=Microsoft.Azure.Management.Search.Fluent&version=1.14.0
#addin nuget:?package=Microsoft.Azure.Management.ServiceBus.Fluent&version=1.14.0
#addin nuget:?package=Microsoft.Azure.Management.ContainerInstance.Fluent&version=1.14.0
#addin nuget:?package=Microsoft.Azure.Management.ContainerRegistry.Fluent&version=1.14.0
#addin nuget:?package=Microsoft.Azure.Management.ContainerService.Fluent&version=1.14.0
#addin nuget:?package=Microsoft.Azure.Management.CosmosDB.Fluent&version=1.14.0
#addin nuget:?package=Microsoft.Azure.Management.Locks.Fluent&version=1.14.0
#addin nuget:?package=Microsoft.Azure.Management.Msi.Fluent&version=1.14.0
#addin nuget:?package=Microsoft.Azure.Management.BatchAI.Fluent&version=1.14.0
#addin nuget:?package=Microsoft.Azure.Management.Monitor.Fluent&version=1.14.0
#addin nuget:?package=Microsoft.Azure.Management.EventHub.Fluent&version=1.14.0
#addin nuget:?package=Microsoft.Azure.Management.ResourceManager.Fluent&version=1.14.0
#addin nuget:?package=Microsoft.Azure.Management.AppService.Fluent&version=1.14.0
#addin nuget:?package=Microsoft.Azure.Management.Sql.Fluent&version=1.14.0

///////////////////////////////////////////////////////////////////////////////
// LOADS
///////////////////////////////////////////////////////////////////////////////

#load cake/paths.cake

///////////////////////////////////////////////////////////////////////////////
// USINGS
///////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Text;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Microsoft.VisualBasic.FileIO;
using System.Threading.Tasks;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Azure.Management.AppService.Fluent;
using Microsoft.Azure.Management.CosmosDB.Fluent;
using Microsoft.Azure.Management.CosmosDB.Fluent.Models;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Management.Storage.Fluent;
using Microsoft.Azure.Management.Sql.Fluent;
using Microsoft.Azure.Management.Sql.Fluent.Models;


///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument("Target", "Build");
var site = Argument("Site", "site");
var app = Argument("App", "app");
var env = Argument("Env", "Prod");
var configuration = Argument("Configuration", "Debug");

///////////////////////////////////////////////////////////////////////////////
// GLOBAL VARIABLES
///////////////////////////////////////////////////////////////////////////////

IAzure azure = null;
IResourceGroup resourceGroup = null;
ICosmosDBAccount cosmosAccount = null;
IStorageAccount storageAccount = null;
IAppServicePlan appServicePlan = null;
IAppServicePlan webAppServicePlan = null;
IWebApp webApp = null;
IFunctionApp  driversFunctionsApp = null;
IFunctionApp  tripsFunctionsApp = null;
IFunctionApp  passengersFunctionsApp = null;
IFunctionApp  orchestratorsFunctionsApp = null;
IFunctionApp  archiverFunctionsApp = null;
ISqlServer sqlDatabaseServer = null;
ISqlDatabase sqlDatabase = null;

string kuduApiAuthorizationToken = "";

///////////////////////////////////////////////////////////////////////////////
// PRIVATE METHODS
///////////////////////////////////////////////////////////////////////////////

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////

Setup(ctx =>
{
	// Executed BEFORE the first task.
	Information("Running tasks...");
});

Teardown(ctx =>
{
	// Executed AFTER the last task.
	Information("Finished running tasks.");
});

///////////////////////////////////////////////////////////////////////////////
// DEPLOY TASKS
///////////////////////////////////////////////////////////////////////////////

Task("Restore")
.Does(() => {
	Information($"Restoring API...{Paths.SolutionFile}");
	NuGetRestore(Paths.SolutionFile);
});

Task("Build")
.IsDependentOn("Restore")
.Does(() => {
	Information("Bulding API....");
	DotNetBuild(Paths.SolutionFile,
	settings => settings.SetConfiguration(configuration).WithTarget("Build"));
});

Task("Zip")
.IsDependentOn("Build")
.Does(() => {
	Information("Zipping API....");
	EnsureDirectoryExists(Paths.PublishDirectory);
	CleanDirectory(Paths.PublishDirectory);

	// Copy the Functions App deploy files
    CopyDirectory($"{app}/bin/{configuration}/netstandard2.0", Paths.PublishDirectory);

	// Zip up
	Zip(Paths.PublishDirectory, Paths.Combine(Paths.PublishDirectory, $"{Resources.GetSite(site, env)}.zip"));
});

Task("Authenticate")
.IsDependentOn("Zip")
.Does(() => {
	Information("Authenticate against Azure....");
	AzureCredentials credentials = SdkContext.AzureCredentialsFactory.FromFile(Paths.GetAuthFile(env));

	azure = Azure
		.Configure()
		.WithLogLevel(HttpLoggingDelegatingHandler.Level.Basic)
		.Authenticate(credentials)
		.WithSubscription(Paths.GetSubscriptionId(env));

	string functionsAppId = $"/subscriptions/{Paths.GetSubscriptionId(env)}/resourceGroups/{Resources.GetResourceGroup(env)}/providers/Microsoft.Web/sites/{Resources.GetSite(site, env)}";
	Information($"Functions App ID: {functionsAppId}");
	var functionsApp = azure.WebApps.GetById(functionsAppId);
	if (functionsApp == null)
		throw new Exception($"Unable to retrieve web app: {functionsAppId}");

	var webPublishProfile = functionsApp.GetPublishingProfile();
	Information($"Web App User ID: {webPublishProfile.GitUsername}");
	kuduApiAuthorizationToken = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{webPublishProfile.GitUsername}:{webPublishProfile.GitPassword}"));
	Information($"Kudu API Auth Token: {kuduApiAuthorizationToken}");
});

Task("ImportAppSettings")
.IsDependentOn("Authenticate")
.Does(() => {
	Information("Import App Settings....");
	var settingsFilePath = Paths.GetAppSettingsCSVFile(Resources.GetSite(site, env), env);
	Information($"Application settings File Path : {settingsFilePath}");

	// https://github.com/projectkudu/kudu/wiki/REST-API
	// https://docs.microsoft.com/en-us/rest/api/appservice/webapps/updateapplicationsettings

	Dictionary<string, string> settings = new Dictionary<string, string>();
	TextFieldParser parser = new TextFieldParser(settingsFilePath.ToString());
	parser.TextFieldType = FieldType.Delimited;
	parser.SetDelimiters("|");
	int row = 0;
	while (!parser.EndOfData)
	{
		string[] fields = parser.ReadFields();
		if (row > 0) // Skip the header row
		{
			//Process row
			try
			{
				var key = "";
				var value = "";
				int index = 0;
				foreach (string field in fields)
				{
					if (index == 0) { key = field; }
					else if (index == 1)
					{
						value = field;
						Information($"App Setting - Key: {key} => value: {value}");
						settings.Add(key, value);
					}

					index = index + 1;
				}
			}
			catch (Exception e)
			{
				Error("A parser error on row {0} occurred {1} ", row, e.Message);
			}
		}

		row++;
	}

	parser.Close();

	string webAppId = $"/subscriptions/{Paths.GetSubscriptionId(env)}/resourceGroups/{Resources.GetResourceGroup(env)}/providers/Microsoft.Web/sites/{Resources.GetSite(site, env)}";
	Information($"Web App ID : {webAppId}");
	var webApp = azure.WebApps.GetById(webAppId);
	if (webApp == null)
		throw new Exception($"Unable to retrieve web app: {webAppId}");
	
	Information($"Updating web app settings: {settings.Count}");
	webApp.Update().WithAppSettings(settings).Apply();
});

Task("Deploy")
.IsDependentOn("ImportAppSettings")
.Does(async () => {
	Information($"Deploying API using Kudu ZipDeploy API....target: {target} - site: {Resources.GetSite(site, env)} - configuration: {configuration}");

	var scmPrefix = $"{Resources.GetSite(site, env)}";

	var kuduApiUri = $"https://{scmPrefix}.scm.azurewebsites.net/api/zipdeploy";
	Information($"Kudu URL: {kuduApiUri}");
	var zipFilePath = Paths.Combine(Paths.PublishDirectory, $"{Resources.GetSite(site, env)}.zip").ToString();
	Information($"Zip File Path: {zipFilePath}");

	HttpClient client = null;

	try
	{
		client = new HttpClient();
		client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", kuduApiAuthorizationToken);

		using (var stream = System.IO.File.OpenRead(zipFilePath))
		{
			var content = new System.Net.Http.StreamContent(stream);
			content.Headers.Add("Content-Type", "multipart/form-data");
			var response = await client.PostAsync(kuduApiUri, content);
			response.EnsureSuccessStatusCode();
		}
	}
	catch (Exception ex)
	{
		Error($"Deploy failed: {ex.Message}");
	}
	finally
	{
		if (client != null)
			client.Dispose();
	}
});

///////////////////////////////////////////////////////////////////////////////
// PROVISION TASKS
///////////////////////////////////////////////////////////////////////////////

Task("ProvisionAuth")
.Does(() => {
	Information("ProvisionAuth ....");
	AzureCredentials credentials = SdkContext.AzureCredentialsFactory.FromFile(Paths.GetAuthFile(env));

	azure = Azure
		.Configure()
		.WithLogLevel(HttpLoggingDelegatingHandler.Level.Basic)
		.Authenticate(credentials)
		.WithSubscription(Paths.GetSubscriptionId(env));

	Information("Authenticated!");
});

Task("ProvisionResourceGroup")
.IsDependentOn("ProvisionAuth")
.Does(() => {
	Information($"ProvisionResourceGroup....");

	resourceGroup = azure.ResourceGroups.List().Where(r => r.Name.ToLower() == Resources.GetResourceGroup(env).ToLower()).FirstOrDefault();
	if (resourceGroup == null)
	{
		resourceGroup = azure.ResourceGroups
						.Define(Resources.GetResourceGroup(env))
						.WithRegion(Region.USEast)
						.Create();

		Information($"Resource group: {resourceGroup.Name} created!");
	}
	else
		Information($"Resource group: {resourceGroup.Name} already created!");
});

Task("ProvisionCosmos")
.IsDependentOn("ProvisionResourceGroup")
.Does(async () => {
	Information($"ProvisionCosmos....");

	cosmosAccount = azure.CosmosDBAccounts.List().Where(a => a.Name.ToLower() == Resources.GetCosmosAccount(env)).FirstOrDefault();
	if (cosmosAccount == null)
	{
		// NOTE: Provisiong a Cosmos Database takes a long time to be online. If you proceed with creating a database and a colection, 
		// you will get an error that says something like `bad request` without much of an explanation. Once the account is online, you can 
		// provision the rest (really all you have to do is restart the provision task).
		// The error is this: Error: One or more errors occurred. Long running operation failed with status 'BadRequest'.  
		// The status in the portal is 'Creating'
		cosmosAccount = azure.CosmosDBAccounts.Define(Resources.GetCosmosAccount(env))
								.WithRegion(Region.USEast)
								.WithExistingResourceGroup(resourceGroup)
								.WithKind(DatabaseAccountKind.GlobalDocumentDB)
								.WithSessionConsistency()
								.WithWriteReplication(Region.USEast)
								.WithReadReplication(Region.USEast)
								.Create();

		Information($"Cosmos Account: {cosmosAccount.Name} created!");
	}
	else
		Information($"Cosmos Account: {cosmosAccount.Name} already created!");

	// Get credentials for the CosmosDB.
	var databaseAccountListKeysResult = cosmosAccount.ListKeys();
	string masterKey = databaseAccountListKeysResult.PrimaryMasterKey;
	string endPoint = cosmosAccount.DocumentEndpoint;

	// Connect to CosmosDB and add a collection
	DocumentClient documentClient = new DocumentClient(new System.Uri(endPoint),
					masterKey, ConnectionPolicy.Default,
					ConsistencyLevel.Session);

	var databases = documentClient.CreateDatabaseQuery().ToList();
	var database = databases.Where(d => d.Id == Resources.GetCosmosDatabase(env)).FirstOrDefault();
	if (database == null)
	{
		// Define a new database
		Database myDatabase = new Database();
		myDatabase.Id = Resources.GetCosmosDatabase(env);

		myDatabase = await documentClient.CreateDatabaseAsync(myDatabase, null);
		Information("Created a new database");

		// Define a new collection
		DocumentCollection myCollection = new DocumentCollection();
		myCollection.Id = Resources.GetCosmosCollection(env);

		// Set the provisioned throughput for this collection to be 400 RUs.
		RequestOptions requestOptions = new RequestOptions();
		requestOptions.OfferThroughput = 400;

		// Create a new collection.
		myCollection = await documentClient.CreateDocumentCollectionAsync(
				"dbs/" + Resources.GetCosmosDatabase(env), 
				myCollection, 
				requestOptions);
		Information("Created a new main collection");

		// Define a new archive collection
		DocumentCollection archiveCollection = new DocumentCollection();
		archiveCollection.Id = Resources.GetCosmosArchiveCollection(env);

		// Set the provisioned throughput for this collection to be 400 RUs.
		RequestOptions archiveRequestOptions = new RequestOptions();
		archiveRequestOptions.OfferThroughput = 400;

		// Create a new collection.
		archiveCollection = await documentClient.CreateDocumentCollectionAsync(
				"dbs/" + Resources.GetCosmosDatabase(env), 
				archiveCollection, 
				archiveRequestOptions);
		Information("Created a new archive collection");
	}
	else
		Information($"Cosmos Database: {Resources.GetCosmosDatabase(env)} already created!");
});

Task("ProvisionStorage")
.IsDependentOn("ProvisionCosmos")
.Does(() => {
	Information($"ProvisionStorage....");
	storageAccount = azure.StorageAccounts.List().Where(r => r.Name.ToLower() == Resources.GetStorageAccount(env).ToLower()).FirstOrDefault();
	if (storageAccount == null)
	{
		storageAccount = azure.StorageAccounts
						.Define(Resources.GetStorageAccount(env))
						.WithRegion(Region.USEast)
						.WithExistingResourceGroup(resourceGroup)
						.Create();
		Information($"Storage Account: {storageAccount.Name} created with {storageAccount.Key}!");
	}
	else
		Information($"Storage Account: {storageAccount.Name} already created!");
});

Task("ProvisionPlan")
.IsDependentOn("ProvisionStorage")
.Does(() => {
	Information($"ProvisionPlan....");

	// NOTE: It appears I cannot create a `Consumption` based service plan for Azure Functions
	// Instead the first function will auto-create it...but it will be auto-named which means it will contains weird numbers in the name!!
	// appServicePlan = azure.AppServices.AppServicePlans.ListByResourceGroup(resourceGroup.Name).Where(r => r.Name.ToLower() == Resources.GetAppServicePlan(env).ToLower()).FirstOrDefault();
	// if (appServicePlan == null)
	// {
	// 	appServicePlan = azure.AppServices.AppServicePlans
	// 					.Define(Resources.GetAppServicePlan(env))
	// 					.WithRegion(Region.USEast)
	// 					.WithExistingResourceGroup(resourceGroup)
	// 					.WithPricingTier(PricingTier.StandardS1) // The pricing tier does not include `Consumption`
	// 					.WithOperatingSystem(Microsoft.Azure.Management.AppService.Fluent.OperatingSystem.Windows)
	// 					.Create();
	// 	Information($"App Service Plan: {appServicePlan.Name} created!");
	// }
	// else
	// 	Information($"App Service Plan: {appServicePlan.Name} already created!");

	// Create a Web App Service Plan for the front-end
	webAppServicePlan = azure.AppServices.AppServicePlans.ListByResourceGroup(resourceGroup.Name).Where(r => r.Name.ToLower() == Resources.GetWebAppServicePlan(env).ToLower()).FirstOrDefault();
	if (webAppServicePlan == null)
	{
		webAppServicePlan = azure.AppServices.AppServicePlans
						.Define(Resources.GetWebAppServicePlan(env))
						.WithRegion(Region.USEast)
						.WithExistingResourceGroup(resourceGroup)
						.WithPricingTier(PricingTier.FreeF1) 
						.WithOperatingSystem(Microsoft.Azure.Management.AppService.Fluent.OperatingSystem.Windows)
						.Create();
		Information($"App Service Plan: {webAppServicePlan.Name} created!");
	}
	else
		Information($"App Service Plan: {webAppServicePlan.Name} already created!");
});

Task("ProvisionFunctionApps")
.IsDependentOn("ProvisionPlan")
.Does(() => {
	Information($"ProvisionFunctionApps....");
	driversFunctionsApp = azure.AppServices.FunctionApps.ListByResourceGroup(resourceGroup.Name).Where(r => r.Name.ToLower() == Resources.GetDriversFunctionApp(env).ToLower()).FirstOrDefault();
	if (driversFunctionsApp == null)
	{
		driversFunctionsApp = azure.AppServices.FunctionApps
							.Define(Resources.GetDriversFunctionApp(env))
							.WithRegion(Region.USEast)
							.WithExistingResourceGroup(resourceGroup)
							.WithExistingStorageAccount(storageAccount)
							.Create();
		Information($"Functions App: {driversFunctionsApp.Name} created!");
	}
	else
		Information($"Functions App: {driversFunctionsApp.Name} already created!");

	appServicePlan = azure.AppServices.AppServicePlans.GetById(driversFunctionsApp.AppServicePlanId);

	tripsFunctionsApp = azure.AppServices.FunctionApps.ListByResourceGroup(resourceGroup.Name).Where(r => r.Name.ToLower() == Resources.GetTripsFunctionApp(env).ToLower()).FirstOrDefault();
	if (tripsFunctionsApp == null)
	{
		tripsFunctionsApp = azure.AppServices.FunctionApps
							.Define(Resources.GetTripsFunctionApp(env))
							.WithExistingAppServicePlan(appServicePlan)
							.WithExistingResourceGroup(resourceGroup)
							.WithExistingStorageAccount(storageAccount)
							.Create();
		Information($"Functions App: {tripsFunctionsApp.Name} created!");
	}
	else
		Information($"Functions App: {tripsFunctionsApp.Name} already created!");

	passengersFunctionsApp = azure.AppServices.FunctionApps.ListByResourceGroup(resourceGroup.Name).Where(r => r.Name.ToLower() == Resources.GetPassengersFunctionApp(env).ToLower()).FirstOrDefault();
	if (passengersFunctionsApp == null)
	{
		passengersFunctionsApp = azure.AppServices.FunctionApps
							.Define(Resources.GetPassengersFunctionApp(env))
							.WithExistingAppServicePlan(appServicePlan)
							.WithExistingResourceGroup(resourceGroup)
							.WithExistingStorageAccount(storageAccount)
							.Create();
		Information($"Functions App: {passengersFunctionsApp.Name} created!");
	}
	else
		Information($"Functions App: {passengersFunctionsApp.Name} already created!");

	orchestratorsFunctionsApp = azure.AppServices.FunctionApps.ListByResourceGroup(resourceGroup.Name).Where(r => r.Name.ToLower() == Resources.GetOrchestratorsFunctionApp(env).ToLower()).FirstOrDefault();
	if (orchestratorsFunctionsApp == null)
	{
		orchestratorsFunctionsApp = azure.AppServices.FunctionApps
							.Define(Resources.GetOrchestratorsFunctionApp(env))
							.WithExistingAppServicePlan(appServicePlan)
							.WithExistingResourceGroup(resourceGroup)
							.WithExistingStorageAccount(storageAccount)
							.Create();
		Information($"Functions App: {orchestratorsFunctionsApp.Name} created!");
	}
	else
		Information($"Functions App: {orchestratorsFunctionsApp.Name} already created!");

	archiverFunctionsApp = azure.AppServices.FunctionApps.ListByResourceGroup(resourceGroup.Name).Where(r => r.Name.ToLower() == Resources.GetTripArchiverFunctionApp(env).ToLower()).FirstOrDefault();
	if (archiverFunctionsApp == null)
	{
		archiverFunctionsApp = azure.AppServices.FunctionApps
							.Define(Resources.GetTripArchiverFunctionApp(env))
							.WithExistingAppServicePlan(appServicePlan)
							.WithExistingResourceGroup(resourceGroup)
							.WithExistingStorageAccount(storageAccount)
							.Create();
		Information($"Functions App: {archiverFunctionsApp.Name} created!");
	}
	else
		Information($"Functions App: {archiverFunctionsApp.Name} already created!");
});

Task("ProvisionWebApp")
.IsDependentOn("ProvisionFunctionApps")
.Does(() => {
	Information($"ProvisionWebApp....");
	webApp = azure.WebApps.ListByResourceGroup(resourceGroup.Name).Where(r => r.Name.ToLower() == Resources.GetWebAppName(env).ToLower()).FirstOrDefault();
	if (webApp == null)
	{
        webApp = azure.WebApps
				.Define(Resources.GetWebAppName(env))
				.WithExistingWindowsPlan(webAppServicePlan)
				.WithExistingResourceGroup(resourceGroup)
				.Create();
		Information($"Web App: {webApp.Name} created!");
	}
	else
		Information($"Web App: {webApp.Name} already created!");
});

Task("ProvisionSql")
.IsDependentOn("ProvisionWebApp")
.Does(() => {
	Information($"ProvisionSql....");
	sqlDatabaseServer = azure.SqlServers.ListByResourceGroup(resourceGroup.Name).Where(r => r.Name.ToLower() == Resources.GetSqlDatabaseServerName(env).ToLower()).FirstOrDefault();
	if (sqlDatabaseServer == null)
	{
		sqlDatabaseServer = azure.SqlServers
							.Define(Resources.GetSqlDatabaseServerName(env))
							.WithRegion(Region.USEast)
							.WithExistingResourceGroup(resourceGroup)
							.WithAdministratorLogin(Resources.SqlDatabaseServerAdminLogin)
							.WithAdministratorPassword(Resources.SqlDatabaseServerAdminPwd)
							.Create();
		Information($"SQL Server: {sqlDatabaseServer.Name} created!");
	}
	else
		Information($"SQL Server: {sqlDatabaseServer.Name} already created!");

	sqlDatabase = sqlDatabaseServer.Databases.List().Where(r => r.Name.ToLower() == Resources.GetSqlDatabaseName(env).ToLower()).FirstOrDefault();
	if (sqlDatabase == null)
	{
		sqlDatabase = sqlDatabaseServer.Databases
					.Define(Resources.GetSqlDatabaseName(env))
					.Create();
		Information($"SQL Server Database: {sqlDatabase.Name} created!");
	}
	else
		Information($"SQL Server Database: {sqlDatabase.Name} already created!");
});

Task("ProvisionEventGrid")
.IsDependentOn("ProvisionSql")
.Does(() => {
	Information($"ProvisionEventGrid....");

	// Unfortunately Event Grids cannot be provisioned using the fluent management APIs yet!! 
});

Task("ProvisionLogicApp")
.IsDependentOn("ProvisionEventGrid")
.Does(() => {
	Information($"ProvisionLogicApp....");

	// Unfortunately Logic Apps cannot be provisioned using the fluent management APIs yet!! 
});

Task("ProvisionAppInsights")
.IsDependentOn("ProvisionLogicApp")
.Does(() => {
	Information($"ProvisionAppInsights....");

	// Unfortunately Application Insights cannot be provisioned using the fluent management APIs yet!! 
});

Task("Provision")
.IsDependentOn("ProvisionAppInsights")
.Does(() => {
	Information($"Provision....");

	// Update the functions app code

});

///////////////////////////////////////////////////////////////////////////////
// DE-PROVISION TASKS
///////////////////////////////////////////////////////////////////////////////

Task("Deprovision")
.IsDependentOn("ProvisionAuth")
.Does(() => {
	Information($"Deprovision....");
	// NOTE: This takes forever...like 8-10 minutes...so be patient :-)
	azure.ResourceGroups.DeleteByName(Resources.GetResourceGroup(env));
});

///////////////////////////////////////////////////////////////////////////////
// BOOTSTRAP
///////////////////////////////////////////////////////////////////////////////

RunTarget(target);

// Sample Provision PowerShell commands:
// ./build.ps1 -Target Provision -ScriptArgs '--Env=Prod'
// ./build.ps1 -Target Provision -ScriptArgs '--Env=Dev'
// ./build.ps1 -Target Deprovision -ScriptArgs '--Env=Prod'
// ./build.ps1 -Target Deprovision -ScriptArgs '--Env=Dev'

// Sample Deploy PowerShell commands:
// ./build.ps1 -Target Deploy -Configuration Debug -ScriptArgs '--Site=Drivers','--App=ServerlessMicroservices.FunctionApp.Drivers','--Env=Dev'
// ./build.ps1 -Target Deploy -Configuration Debug -ScriptArgs '--Site=Orchestrators','--App=ServerlessMicroservices.FunctionApp.Orchestrators','--Env=Dev'
// ./build.ps1 -Target Deploy -Configuration Debug -ScriptArgs '--Site=Trips','--App=ServerlessMicroservices.FunctionApp.Trips','--Env=Dev'
// ./build.ps1 -Target Deploy -Configuration Debug -ScriptArgs '--Site=Passengers','--App=ServerlessMicroservices.FunctionApp.Passengers','--Env=Dev'

// ./build.ps1 -Target ImportAppSettings -ScriptArgs '--Site=Drivers','--App=ServerlessMicroservices.FunctionApp.Drivers','--Env=Dev'
// ./build.ps1 -Target ImportAppSettings -ScriptArgs '--Site=Orchestrators','--App=ServerlessMicroservices.FunctionApp.Orchestrators','--Env=Dev'
// ./build.ps1 -Target ImportAppSettings -ScriptArgs '--Site=Trips','--App=ServerlessMicroservices.FunctionApp.Trips','--Env=Dev'
// ./build.ps1 -Target ImportAppSettings -ScriptArgs '--Site=Passengers','--App=ServerlessMicroservices.FunctionApp.Passengers','--Env=Dev'

// Bootstrapping configuration via cake.config

