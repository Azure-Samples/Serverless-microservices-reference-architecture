using ServerlessMicroservices.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServerlessMicroservices.Shared.Services
{
    public class RoutesService : IRoutesService
    {
        private static Random _rnd = new Random();

        private ISettingService _settingService;
        private ILoggerService _loggerService;

        public RoutesService(ISettingService setting, ILoggerService logger)
        {
            _settingService = setting;
            _loggerService = logger;
        }

        public async Task<List<TripLocation>> RetrieveRouteItems(TripLocation source, TripLocation destination)
        {
            //TODO: Add Bing API here

            var routeLocations = new List<TripLocation>();
            var points = _rnd.Next(10);
            for (var i=0; i < points; i++)
                routeLocations.Add(source);

            routeLocations.Add(destination);
            return routeLocations;
        }
    }
}
