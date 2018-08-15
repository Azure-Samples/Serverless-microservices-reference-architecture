namespace ServerlessMicroservices.Shared.Services
{
    public interface ISettingService
    {
        string GetSiteName();

        // Management    
        bool IsPersistDirectly();
        int GetDriversAcknowledgeMaxWaitPeriodInSeconds();
        double GetDriversLocationRadiusInMiles();
        int GetTripMonitorIntervalInSeconds();
        int GetTripMonitorMaxIterations();

        // App Insights
        string GetInsightsInstrumentationKey();

        // Cosmos
        string GetDocDbEndpointUri();
        string GetDocDbApiKey();
        string GetDocDbConnectionString();
        string GetDocDbRideShareDatabaseName();
        string GetDocDbMainCollectionName();
        int GetDocDbThroughput();

        // Orchestrators
        string GetStartTripManagerOrchestratorBaseUrl();
        string GetStartTripManagerOrchestratorApiKey();
        string GetTerminateTripManagerOrchestratorBaseUrl();
        string GetTerminateTripManagerOrchestratorApiKey();
        string GetTerminateTripMonitorOrchestratorBaseUrl();
        string GetTerminateTripMonitorOrchestratorApiKey();
    }
}
