namespace ServerlessMicroservices.Shared.Helpers
{
    public class Constants
    {
        // Equates
        public const int MAX_RETRIEVE_DOCS = 20;
        public const string SECURITY_VALITION_ERROR = "SECURIY_VALIDATION_ERROR";

        // Internal Events (consumed by the orchestrators)
        public const string TRIP_DRIVER_ACCEPT_EVENT = "AcceptResult";

        // Event Grid Event Types
        public const string EVG_EVENT_TYPE_MANAGER_TRIP = "Manager Externalized Trip";
        public const string EVG_EVENT_TYPE_MONITOR_TRIP = "Monitor Externalized Trip";

        // Event Grid Event Subjects
        public const string EVG_SUBJECT_TRIP_DRIVERS_NOTIFIED = "Drivers notified!";
        public const string EVG_SUBJECT_TRIP_DRIVER_PICKED = "Driver picked :-)";
        public const string EVG_SUBJECT_TRIP_STARTING = "Trip starting :-)";
        public const string EVG_SUBJECT_TRIP_RUNNING = "Trip running...";
        public const string EVG_SUBJECT_TRIP_COMPLETED = "Trip completed :-)";
        public const string EVG_SUBJECT_TRIP_ABORTED = "Trip aborted :-(";
    }
}
