namespace ServerlessMicroservices.FunctionApp.Orchestrators
{
    public class Constants
    {
		// Periods
		public const int WAIT_FOR_DRIVERS_PERIOD_IN_SECONDS = 120;
		public const int TRIP_UPDATE_INTERVAL_IN_SECONDS = 5;
		
		// Events
        public const string TRIP_DRIVER_ACKNOWLEDGE = "AcknowledgeResult";
    }
}
