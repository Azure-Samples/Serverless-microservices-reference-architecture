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

        // Event Grid Urls
        string GetTripExternalizationsEventGridTopicUrl();
        string GetTripExternalizationsEventGridTopicApiKey();

        // B2C settings
        string GetAuthorityUrl();
        string GetApiApplicationId();
        string GetApiScopeName();
        
        // graph settings
        string GetGraphTenantId();
        string GetGraphClientId();
        string GetGraphClientSecret();
    }
}
