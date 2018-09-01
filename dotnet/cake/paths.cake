public static class Paths
{
    // Subscriptions
    public const string ProdSubscriptionId = "e433f371-e5e9-4238-abc2-7c38aa596a18";
    public const string DevSubscriptionId = "e433f371-e5e9-4238-abc2-7c38aa596a18";

    public static string GetSubscriptionId(string env)
    {
        if (env.ToLower() == "prod")
            return ProdSubscriptionId;
        else   
            return DevSubscriptionId;
    }

    // Auth Files
    public static string GetAuthFile(string env)
    {
        return $"{env.ToLower()}-authfile.txt";
    }

    public static FilePath SolutionFile => "dotnet.sln";
    public static DirectoryPath SettingsDirectory => "settings";

    public static DirectoryPath PublishDirectory => "dist";

    public static DirectoryPath CombineWithTarget(DirectoryPath directory, string target)
    {
        return new DirectoryPath($"{directory.ToString()}/{target}");
    }

    public static FilePath GetAppSettingsCSVFile(string site, string env)
    {
        return new FilePath($"{SettingsDirectory.ToString()}/{site}-AppSettings.csv");
    }

    public static FilePath GetConnectionStringsCSVFile(string site, string env)
    {
        return new FilePath($"{SettingsDirectory.ToString()}/{site}-ConnectionStrings.csv");
    }

    public static FilePath Combine(DirectoryPath directory, FilePath file)
    {
        return directory.CombineWithFilePath(file);
    }
} 

public static class Resources
{
    // Resource Group
    public const string ProdResourceGroup = "serverless-microservices";
    public const string DevResourceGroup = "serverless-microservices-dev";

    public static string GetResourceGroup(string env)
    {
        if (env.ToLower() == "prod")
            return ProdResourceGroup;
        else   
            return DevResourceGroup;
    }

    // Cosmos
    public const string ProdCosmosAccount = "rideshare"; 
    public const string DevCosmosAccount = "ridesharedev";
    public const string CosmosDatabase = "RideShare";
    public const string CosmosCollection = "Main";
    public const string CosmosArchiveCollection = "Archive";

    public static string GetCosmosAccount(string env)
    {
        if (env.ToLower() == "prod")
            return ProdCosmosAccount;
        else   
            return DevCosmosAccount;
    }

    public static string GetCosmosDatabase(string env)
    {
        return CosmosDatabase;
    }

    public static string GetCosmosCollection(string env)
    {
        return CosmosCollection;
    }

    public static string GetCosmosArchiveCollection(string env)
    {
        return CosmosArchiveCollection;
    }

    // Storage
    public const string ProdStorageAccount = "ridesharefunctionstore";
    public const string DevStorageAccount = "ridesharefunctiondev";

    public static string GetStorageAccount(string env)
    {
        if (env.ToLower() == "prod")
            return ProdStorageAccount;
        else   
            return DevStorageAccount;
    }

    // App Service Plan
    public const string ProdAppServicePlan = "RideShareConsumptionPlan";
    public const string DevAppServicePlan = "RideShareConsumptionPlanDev";

    public static string GetAppServicePlan(string env)
    {
        if (env.ToLower() == "prod")
            return ProdAppServicePlan;
        else   
            return DevAppServicePlan;
    }

    // Web App Service Plan
    public const string ProdWebAppServicePlan = "RideShareAppServicePlan";
    public const string DevWebAppServicePlan = "RideShareAppServicePlanDev";

    public static string GetWebAppServicePlan(string env)
    {
        if (env.ToLower() == "prod")
            return ProdWebAppServicePlan;
        else   
            return DevWebAppServicePlan;
    }

    // Drivers Functions App 
    public const string ProdDriversFunctionApp = "RideshareDriversFunctionApp";
    public const string DevDriversFunctionApp = "RideshareDriversFunctionAppDev";
    public const string ProdTripsFunctionApp = "RideshareTripsFunctionApp";
    public const string DevTripsFunctionApp = "RideshareTripsFunctionAppDev";
    public const string ProdPassengersFunctionApp = "RidesharePassengersFunctionApp";
    public const string DevPassengersFunctionApp = "RidesharePassengersFunctionAppDev";
    public const string ProdOrchestratorsFunctionApp = "RideshareOrchestratorsFunctionApp";
    public const string DevOrchestratorsFunctionApp = "RideshareOrchestratorsFunctionAppDev";
    public const string ProdTripArchiverFunctionApp = "RideshareTripArchiverFunctionApp";
    public const string DevTripArchiverFunctionApp = "RideshareTripArchiverFunctionAppDev";

    public static string GetDriversFunctionApp(string env)
    {
        if (env.ToLower() == "prod")
            return ProdDriversFunctionApp;
        else   
            return DevDriversFunctionApp;
    }

    public static string GetTripsFunctionApp(string env)
    {
        if (env.ToLower() == "prod")
            return ProdTripsFunctionApp;
        else   
            return DevTripsFunctionApp;
    }

    public static string GetPassengersFunctionApp(string env)
    {
        if (env.ToLower() == "prod")
            return ProdPassengersFunctionApp;
        else   
            return DevPassengersFunctionApp;
    }

    public static string GetOrchestratorsFunctionApp(string env)
    {
        if (env.ToLower() == "prod")
            return ProdOrchestratorsFunctionApp;
        else   
            return DevOrchestratorsFunctionApp;
    }

    public static string GetTripArchiverFunctionApp(string env)
    {
        if (env.ToLower() == "prod")
            return ProdTripArchiverFunctionApp;
        else   
            return DevTripArchiverFunctionApp;
    }

    // Web App 
    public const string ProdRideShareWebApp = "RelecloudRideshare";
    public const string DevRideShareWebApp = "RelecloudRideshareDev";

    public static string GetWebAppName(string env)
    {
        if (env.ToLower() == "prod")
            return ProdRideShareWebApp;
        else   
            return DevRideShareWebApp;
    }

    // SQL 
    public const string SqlDatabaseServerAdminLogin = "rideshareadm!n";
    public const string SqlDatabaseServerAdminPwd = "rideShareS3cureP@ssword";
    public const string ProdSqlDatabaseServer = "rideshare-db";
    public const string DevSqlDatabaseServer = "rideshare-db-dev";
    public const string ProdSqlDatabase = "RideShare";
    public const string DevSqlDatabase = "RideShare";

    public static string GetSqlDatabaseServerName(string env)
    {
        if (env.ToLower() == "prod")
            return ProdSqlDatabaseServer;
        else   
            return DevSqlDatabaseServer;
    }

    public static string GetSqlDatabaseName(string env)
    {
        if (env.ToLower() == "prod")
            return ProdSqlDatabase;
        else   
            return DevSqlDatabase;
    }

    // App Insights
    public const string ProdAppInsights = "rideshare";
    public const string DevAppInsights = "rideshare-dev";

    // Event Grid Topics
    public const string ProdTripsExternalizationsEventGridTopic = "RideShareTripExternalizations";
    public const string DevTripsExternalizationsEventGridTopic = "RideShareTripExternalizationsDev";

    // API Management Service
    public const string ProdAPIManagementService = "rideshare";
    public const string DevAPIManagementService = "rideshare-dev";

    // SignalR  Service
    public const string ProdSignalRService = "rideshare";
    public const string DevSignalRService = "rideshare-dev";

    // B2C Tenant

    // Site
    public static string GetSite(string type, string env)
    {
        if (type.ToLower() == "Drivers".ToLower())
        {
            return GetDriversFunctionApp(env);
        }
        else if (type.ToLower() == "Trips".ToLower())
        {
            return GetTripsFunctionApp(env);
        }
        else if (type.ToLower() == "Passengers".ToLower())
        {
            return GetPassengersFunctionApp(env);
        }
        else if (type.ToLower() == "Orchestrators".ToLower())
        {
            return GetOrchestratorsFunctionApp(env);
        }
        else
            return "Uknown";
    }
}

public class SettingEntry 
{
    public string Key { get; set; }
    public string Value { get; set; }
}
