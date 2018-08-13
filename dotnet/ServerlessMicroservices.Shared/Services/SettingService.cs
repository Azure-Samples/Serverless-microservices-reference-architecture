using System;

namespace ServerlessMicroservices.Shared.Services
{
    public class SettingService : ISettingService
    {
        public const string LOG_TAG = "SettingService";

        //*** These settings must exist in the FunctionsApp App Settings ***//

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

        public string GetSiteName()
        {
            return GetEnvironmentVariable("WEBSITE_SITE_NAME");
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

        //*** PRIVATE ***//
        private static string GetEnvironmentVariable(string name)
        {
            return System.Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
        }
    }
}
