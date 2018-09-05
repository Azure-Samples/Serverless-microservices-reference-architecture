namespace ServerlessMicroservices.Shared.Services
{
    // Due to lack of dependency injection in Functions!!!
    public static class ServiceFactory
    {
        private static ISettingService _settingService = null;
        private static ILoggerService _loggerService = null;
        private static IStorageService _storageService = null;
        private static IChangeNotifierService _changeNotifierService = null;
        private static IPersistenceService _persistenceService = null;
        private static IPersistenceService _archiveService = null;
        private static IPowerBIService _powerBIService = null;
        private static ITokenValidationService _tokenValidationService = null;
        private static IRoutesService _routesService = null;
        private static IUserService _userService = null;

        public static ISettingService GetSettingService()
        {
            if (_settingService == null)
            _settingService = new SettingService();

            return _settingService;
        }

        public static ILoggerService GetLoggerService()
        {
            if (_loggerService == null)
                _loggerService = new LoggerService(GetSettingService());

            return _loggerService;
        }

        public static IStorageService GetStorageService()
        {
            if (_storageService == null)
                _storageService = new StorageService(GetSettingService(), GetLoggerService());

            return _storageService;
        }

        public static IChangeNotifierService GetChangeNotifierService()
        {
            if (_changeNotifierService == null)
            _changeNotifierService = new ChangeNotifierService(GetSettingService(), GetLoggerService(), GetStorageService());

            return _changeNotifierService;
        }

        public static IPersistenceService GetPersistenceService()
        {
            if (_persistenceService == null)
            _persistenceService = new CosmosPersistenceService(GetSettingService(), GetLoggerService(), GetChangeNotifierService());

            return _persistenceService;
        }

        public static IPersistenceService GetArchiveService()
        {
            if (_archiveService == null)
                _archiveService = new SqlPersistenceService(GetSettingService(), GetLoggerService());

            return _archiveService;
        }

        public static IPowerBIService GetPowerBIService()
        {
            if (_powerBIService == null)
                _powerBIService = new PowerBIService(GetSettingService(), GetLoggerService());

            return _powerBIService;
        }

        public static ITokenValidationService GetTokenValidationService()
        {
            if (_tokenValidationService == null)
            _tokenValidationService = new TokenValidationService(GetSettingService(), GetLoggerService());

            return _tokenValidationService;
        }

        public static IRoutesService GetRoutesService()
        {
            if (_routesService == null)
            _routesService = new RoutesService(GetSettingService(), GetLoggerService());

            return _routesService;
        }

        public static IUserService GetUserService()
        {
            if (_userService == null)
            _userService = new UserService(GetSettingService());

            return _userService;
        }
    }
}
