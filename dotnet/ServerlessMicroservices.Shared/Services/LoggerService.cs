using Microsoft.ApplicationInsights;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ServerlessMicroservices.Shared.Services
{
    public class LoggerService : ILoggerService
    {
        private ISettingService _settingService;

        public LoggerService(ISettingService setting)
        {
            _settingService = setting;
        }

        public void Log(string message)
        {
            GetAppInsightsClient().TrackTrace(message);
        }

        public void Log(string eventName, Dictionary<string, string> props, Dictionary<string, double> measurements = null)
        {
            GetAppInsightsClient().TrackEvent(eventName, props, measurements);
        }

        public void Log(string metricName, double metricValue)
        {
            GetAppInsightsClient().TrackMetric(metricName, metricValue);
        }

        public void Log(Exception e)
        {
            GetAppInsightsClient().TrackException(e);
        }

        // *** PRIVATE *** //
        private TelemetryClient _appInsightsClient;
        private TelemetryClient GetAppInsightsClient()
        {
            if (_appInsightsClient == null)
            {
                _appInsightsClient = new TelemetryClient();
                string key = _settingService.GetInsightsInstrumentationKey();

                // It is possible to create a Telemetry client without an iKey. But trying to set this property to null will throw a null argument exception
                if (!string.IsNullOrEmpty(key)) _appInsightsClient.InstrumentationKey = key;
            }

            return _appInsightsClient;
        }
    }
}
