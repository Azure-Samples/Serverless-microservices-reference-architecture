namespace ServerlessMicroservices.Shared.Services
{
    // Due to lack of dependency injection in Functions!!!
    public static class ServiceFactory
    {
        private static ISettingService _settingService = null;
        private static ILoggerService _loggerService = null;
        private static IAnalyticService _analyticService = null;
        private static IPersistenceService _persistenceService = null;

        public static ISettingService GetSettingService()
        {
            if (_settingService == null)
                _settingService = new SettingService();

            return _settingService;
        }

        public static ILoggerService GetLoggerService()
        {
            if (_loggerService == null)
                _loggerService = null; //TODO:

            return _loggerService;
        }

        public static IAnalyticService GetAnalyticService()
        {
            if (_analyticService == null)
                _analyticService = null; //TODO:

            return _analyticService;
        }

        public static IPersistenceService GetPersistenceService(ISettingService setting, ILoggerService logger, IAnalyticService analytic)
        {
            if (_persistenceService == null)
                _persistenceService = new CosmosPersistenceService(setting, logger, analytic);

            return _persistenceService;
        }
    }
}
