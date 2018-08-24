using ServerlessMicroservices.Models;
using System;
using System.Threading.Tasks;

namespace ServerlessMicroservices.Shared.Services
{
    public class PowerBIService : IPowerBIService
    {
        public const string LOG_TAG = "PowerBIService";

        private ISettingService _settingService;
        private ILoggerService _loggerService;

        public PowerBIService(ISettingService setting, ILoggerService logger)
        {
            _settingService = setting;
            _loggerService = logger;
        }

        public async Task UpsertTrip(TripItem trip)
        {
            var error = "";

            try
            {
                //TODO: Add PowerBI integration
            }
            catch (Exception ex)
            {
                error = ex.Message;
                throw new Exception(error);
            }
            finally
            {
                _loggerService.Log($"{LOG_TAG} - UpsertTrip - Error: {error}");
            }
        }
    }
}
