using System;

namespace ServerlessMicroservices.Shared.Services
{
    public class SettingService : ISettingService
    {
        public const string LOG_TAG = "SettingService";

        //*** These settings must exist in the FunctionsApp App Settings ***//

        // Global
        private const string EnableAuthKey = "EnableAuth";

        // Storage
        private const string StorageAccountKey = "AzureWebJobsStorage";
        private const string TripManagersQueueKey = "TripManagersQueue";
        private const string TripMonitorsQueueKey = "TripMonitorsQueue";
        private const string TripDemosQueueKey = "TripDemosQueue";
        private const string TripDriversQueueKey = "TripDriversQueue";

        // Management
        private const string IsRunningInContainerKey = "IsRunningInContainer";
        private const string IsEnqueueToOrchestratorsKey = "IsEnqueueToOrchestrators";
        private const string IsPersistDirectlyKey = "IsPersistDirectly";
        private const string DriversAcknowledgeMaxWaitPeriodInSecondsKey = "DriversAcknowledgeMaxWaitPeriodInSeconds";
        private const string DriversLocationRadiusInMilesKey = "DriversLocationRadiusInMiles";
        private const string TripMonitorIntervalInSecondsKey = "TripMonitorIntervalInSeconds";
        private const string TripMonitorMaxIterationsKey = "TripMonitorMaxIterations";

        // Insights Keys - this for the settings. The Functions App needs it also in APPINSIGHTS_INSTRUMENTATIONKEY
        // https://github.com/Azure/Azure-Functions/wiki/App-Insights
        private const string InsightsInstrumentationKey = "InsightsInstrumentationKey";

        // Cosmos DB
        private const string DocDbEndpointUriKey = "DocDbEndpointUri";
        private const string DocDbApiKey = "DocDbApiKey";
        private const string DocDbConnectionStringKey = "DocDbConnectionStringKey";
        private const string DocDbRideShareDatabaseNameKey = "DocDbRideShareDatabaseName";
        private const string DocDbRideShareMainCollectionNameKey = "DocDbRideShareMainCollectionName";
        private const string DocDbThroughput = "DocDbThroughput";

        // Sql
        private const string SqlConnectionStringKey = "SqlConnectionString";

        // Orchestrators
        private const string StartTripManagerOrchestratorBaseUrlKey = "StartTripManagerOrchestratorBaseUrl";
        private const string startTripManagerOrchestratorApiKey = "StartTripManagerOrchestratorApiKey";
        private const string StartTripDemoOrchestratorBaseUrlKey = "StartTripDemoOrchestratorBaseUrl";
        private const string startTripDemoOrchestratorApiKey = "StartTripDemoOrchestratorApiKey";
        private const string TerminateTripManagerOrchestratorBaseUrlKey = "TerminateTripManagerOrchestratorBaseUrl";
        private const string TerminateTripManagerOrchestratorApiKey = "TerminateTripManagerOrchestratorApiKey";
        private const string TerminateTripMonitorOrchestratorBaseUrlKey = "TerminateTripMonitorOrchestratorBaseUrl";
        private const string TerminateTripMonitorOrchestratorApiKey = "TerminateTripMonitorOrchestratorApiKey";

        // Event Grid
        private const string TripExternalizationsEventGridTopicUrlKey = "TripExternalizationsEventGridTopicUrl";
        private const string TripExternalizationsEventGridTopicApiKey = "TripExternalizationsEventGridTopicApiKey";

        // B2C
        private const string AuthorityUrlKey = "AuthorityUrl";
        private const string ApiApplicationIdKey = "ApiApplicationId";
        private const string ApiScopeNameKey = "ApiScopeName";

        // graph settings
        private const string GraphTenantId = "GraphTenantId";
        private const string GraphClientId = "GraphClientId";
        private const string GraphClientSecret = "GraphClientSecret";


        public string GetSiteName()
        {
            return GetEnvironmentVariable("WEBSITE_SITE_NAME");
        }

        public bool EnableAuth()
        {
            if (
                GetEnvironmentVariable(EnableAuthKey) != null &&
                !string.IsNullOrEmpty(GetEnvironmentVariable(EnableAuthKey).ToString()) &&
                GetEnvironmentVariable(EnableAuthKey).ToString().ToLower() == "true"
            )
            {
                return true;
            }

            return false;
        }

        // Storage
        public string GetStorageAccount()
        {
            return GetEnvironmentVariable(StorageAccountKey);
        }

        public string GetTripManagersQueueName()
        {
            return GetEnvironmentVariable(TripManagersQueueKey);
        }

        public string GetTripMonitorsQueueName()
        {
            return GetEnvironmentVariable(TripMonitorsQueueKey);
        }

        public string GetTripDemosQueueName()
        {
            return GetEnvironmentVariable(TripDemosQueueKey);
        }

        public string GetTripDriversQueueName()
        {
            return GetEnvironmentVariable(TripDriversQueueKey);
        }

        // Management
        public bool IsRunningInContainer()
        {
            if (
                GetEnvironmentVariable(IsRunningInContainerKey) != null &&
                !string.IsNullOrEmpty(GetEnvironmentVariable(IsRunningInContainerKey).ToString()) &&
                GetEnvironmentVariable(IsRunningInContainerKey).ToString().ToLower() == "true"
                )
            {
                return true;
            }

            return false;
        }

        public bool IsEnqueueToOrchestrators()
        {
            // defaults to true
            if (string.IsNullOrEmpty(GetEnvironmentVariable(IsEnqueueToOrchestratorsKey))) return true;

            return bool.Parse(GetEnvironmentVariable(IsEnqueueToOrchestratorsKey));
        }

        public bool IsPersistDirectly()
        {
            if (
                GetEnvironmentVariable(IsPersistDirectlyKey) != null &&
                !string.IsNullOrEmpty(GetEnvironmentVariable(IsPersistDirectlyKey).ToString()) &&
                GetEnvironmentVariable(IsPersistDirectlyKey).ToString().ToLower() == "true"
                )
            {
                return true;
            }

            return false;
        }

        public int GetDriversAcknowledgeMaxWaitPeriodInSeconds()
        {
            return GetEnvironmentVariable(DriversAcknowledgeMaxWaitPeriodInSecondsKey) != null ? Int32.Parse(GetEnvironmentVariable(DriversAcknowledgeMaxWaitPeriodInSecondsKey)) : 120;
        }

        public double GetDriversLocationRadiusInMiles()
        {
            return GetEnvironmentVariable(DriversLocationRadiusInMilesKey) != null ? Double.Parse(GetEnvironmentVariable(DriversLocationRadiusInMilesKey)) : 15;
        }

        public int GetTripMonitorIntervalInSeconds()
        {
            return GetEnvironmentVariable(TripMonitorIntervalInSecondsKey) != null ? Int32.Parse(GetEnvironmentVariable(TripMonitorIntervalInSecondsKey)) : 10;
        }

        public int GetTripMonitorMaxIterations()
        {
            return GetEnvironmentVariable(TripMonitorMaxIterationsKey) != null ? Int32.Parse(GetEnvironmentVariable(TripMonitorMaxIterationsKey)) : 20;
        }

        // App Insights
        public string GetInsightsInstrumentationKey()
        {
            return GetEnvironmentVariable(InsightsInstrumentationKey);
        }

        // Cosmos
        public string GetDocDbEndpointUri()
        {
            return GetEnvironmentVariable(DocDbEndpointUriKey);
        }

        public string GetDocDbApiKey()
        {
            return GetEnvironmentVariable(DocDbApiKey);
        }

        public string GetDocDbConnectionString()
        {
            return GetEnvironmentVariable(DocDbConnectionStringKey);
        }

        public string GetDocDbRideShareDatabaseName()
        {
            return GetEnvironmentVariable(DocDbRideShareDatabaseNameKey);
        }

        public string GetDocDbMainCollectionName()
        {
            return GetEnvironmentVariable(DocDbRideShareMainCollectionNameKey);
        }

        public int? GetDocDbThroughput()
        {
            if (int.TryParse(GetEnvironmentVariable(DocDbThroughput), out int throughput)) return throughput;
            return null;
        }

        // Sql
        public string GetSqlConnectionString()
        {
            return GetEnvironmentVariable(SqlConnectionStringKey);
        }

        // Trip Manager Orchestrator
        public string GetStartTripManagerOrchestratorBaseUrl()
        {
            return GetEnvironmentVariable(StartTripManagerOrchestratorBaseUrlKey);
        }

        public string GetStartTripManagerOrchestratorApiKey()
        {
            return GetEnvironmentVariable(startTripManagerOrchestratorApiKey);
        }

        public string GetStartTripDemoOrchestratorBaseUrl()
        {
            return GetEnvironmentVariable(StartTripDemoOrchestratorBaseUrlKey);
        }

        public string GetStartTripDemoOrchestratorApiKey()
        {
            return GetEnvironmentVariable(startTripDemoOrchestratorApiKey);
        }

        public string GetTerminateTripManagerOrchestratorBaseUrl()
        {
            return GetEnvironmentVariable(TerminateTripManagerOrchestratorBaseUrlKey);
        }

        public string GetTerminateTripManagerOrchestratorApiKey()
        {
            return GetEnvironmentVariable(TerminateTripManagerOrchestratorApiKey);
        }

        public string GetTerminateTripMonitorOrchestratorBaseUrl()
        {
            return GetEnvironmentVariable(TerminateTripMonitorOrchestratorBaseUrlKey);
        }

        public string GetTerminateTripMonitorOrchestratorApiKey()
        {
            return GetEnvironmentVariable(TerminateTripMonitorOrchestratorApiKey);
        }

        // Event Grid Urls
        public string GetTripExternalizationsEventGridTopicUrl()
        {
            return GetEnvironmentVariable(TripExternalizationsEventGridTopicUrlKey);
        }

        public string GetTripExternalizationsEventGridTopicApiKey()
        {
            return GetEnvironmentVariable(TripExternalizationsEventGridTopicApiKey);
        }

        // B2C
        public string GetAuthorityUrl()
        {
            return GetEnvironmentVariable(AuthorityUrlKey);
        }

        public string GetApiApplicationId()
        {
            return GetEnvironmentVariable(ApiApplicationIdKey);
        }

        public string GetApiScopeName()
        {
            return GetEnvironmentVariable(ApiScopeNameKey);
        }

        // graph
        public string GetGraphTenantId()
        {
            return GetEnvironmentVariable(GraphTenantId);
        }

        public string GetGraphClientId()
        {
            return GetEnvironmentVariable(GraphClientId);
        }

        public string GetGraphClientSecret()
        {
            return GetEnvironmentVariable(GraphClientSecret);
        }

        //*** PRIVATE ***//
        private static string GetEnvironmentVariable(string name)
        {
            return System.Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
        }
    }
}
