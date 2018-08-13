using System;

namespace ServerlessMicroservices.Models
{
    public class Driver
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public virtual Car Car { get; set; }
    }
}
