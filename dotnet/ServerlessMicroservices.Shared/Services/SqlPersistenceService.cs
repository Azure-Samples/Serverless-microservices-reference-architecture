using ServerlessMicroservices.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServerlessMicroservices.Shared.Services
{
    public class SqlPersistenceService : IPersistenceService
    {
        public const string LOG_TAG = "SqlPersistenceService";

        private ISettingService _settingService;
        private ILoggerService _loggerService;
        private IAnalyticService _analyticService;

        public SqlPersistenceService(ISettingService setting, ILoggerService logger, IAnalyticService anayltic)
        {
            _settingService = setting;
            _loggerService = logger;
            _analyticService = anayltic;
        }

        public Task<DriverItem> RetrieveDriver(string code)
        {
            throw new NotImplementedException();
        }

        public Task<List<DriverItem>> RetrieveDrivers(int max = 20)
        {
            throw new NotImplementedException();
        }

        public Task<int> RetrieveDriversCount()
        {
            throw new NotImplementedException();
        }

        public Task<string> UpsertDriver(DriverItem driver, bool isIgnoreChangeFeed = false)
        {
            throw new NotImplementedException();
        }

        public Task<string> UpsertDriverLocation(DriverLocationItem driver, bool isIgnoreChangeFeed = false)
        {
            throw new NotImplementedException();
        }

        public Task<List<DriverLocationItem>> RetrieveDriverLocations(string code, int max = 20)
        {
            throw new NotImplementedException();
        }

        public Task DeleteDriver(string code)
        {
            throw new NotImplementedException();
        }
    }
}
