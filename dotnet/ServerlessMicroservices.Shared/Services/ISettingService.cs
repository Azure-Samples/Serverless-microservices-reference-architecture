namespace ServerlessMicroservices.Shared.Services
{
    public interface ISettingService
    {
        string GetSiteName();

        // App Insights
        string GetInsightsInstrumentationKey();

        // Cosmos
        string GetDocDbEndpointUri();
        string GetDocDbApiKey();
        string GetDocDbConnectionString();
        string GetDocDbRideShareDatabaseName();
        string GetDocDbMainCollectionName();
        int GetDocDbThroughput();
    }
}
