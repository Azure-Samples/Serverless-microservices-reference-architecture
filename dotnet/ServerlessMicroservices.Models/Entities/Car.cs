using System;

namespace ServerlessMicroservices.Models.Entities
{
    public class Car
    {
        public Guid Id { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public string Color { get; set; }
        public string LicensePlate { get; set; }
    }
}
