using System;

namespace ServerlessMicroservices.Shared.Services
{
    public class SettingService : ISettingService
    {
        public const string LOG_TAG = "SettingService";

        //*** These settings must exist in the FunctionsApp App Settings ***//

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

        // Orchestrators
        private const string StartTripManagerOrchestratorBaseUrlKey = "StartTripManagerOrchestratorBaseUrl";
        private const string startTripManagerOrchestratorApiKey = "StartTripManagerOrchestratorApiKey";
        private const string TerminateTripManagerOrchestratorBaseUrlKey = "TerminateTripManagerOrchestratorBaseUrl";
        private const string TerminateTripManagerOrchestratorApiKey = "TerminateTripManagerOrchestratorApiKey";
        private const string TerminateTripMonitorOrchestratorBaseUrlKey = "TerminateTripMonitorOrchestratorBaseUrl";
        private const string TerminateTripMonitorOrchestratorApiKey = "TerminateTripMonitorOrchestratorApiKey";

        public string GetSiteName()
        {
            return GetEnvironmentVariable("WEBSITE_SITE_NAME");
        }

        // Management
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

        public int GetDocDbThroughput()
        {
            return GetEnvironmentVariable(DocDbThroughput) != null ? Int32.Parse(GetEnvironmentVariable(DocDbThroughput)) : 400;
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

        //*** PRIVATE ***//
        private static string GetEnvironmentVariable(string name)
        {
            return System.Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
        }
    }
}
