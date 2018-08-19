namespace ServerlessMicroservices.Shared.Services
{
    // Due to lack of dependency injection in Functions!!!
    public static class ServiceFactory
    {
        private static ISettingService _settingService = null;
        private static ILoggerService _loggerService = null;
        private static IAnalyticService _analyticService = null;
        private static IChangeNotifierService _changeNotifierService = null;
        private static IPersistenceService _persistenceService = null;
        private static IRoutesService _routesService = null;

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

        public static IChangeNotifierService GetChangeNotifierService()
        {
            if (_changeNotifierService == null)
                _changeNotifierService = new ChangeNotifierService(GetSettingService(), GetLoggerService());

            return _changeNotifierService;
        }

        public static IPersistenceService GetPersistenceService()
        {
            if (_persistenceService == null)
                _persistenceService = new CosmosPersistenceService(GetSettingService(), GetLoggerService(), GetAnalyticService(), GetChangeNotifierService());

            return _persistenceService;
        }

        public static IRoutesService GetRoutesService()
        {
            if (_routesService == null)
                _routesService = new RoutesService(GetSettingService(), GetLoggerService());

            return _routesService;
        }
    }
}
