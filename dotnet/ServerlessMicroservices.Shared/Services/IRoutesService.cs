using ServerlessMicroservices.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServerlessMicroservices.Shared.Services
{
    public interface IRoutesService
    {
        Task<List<TripLocation>> RetrieveRouteItems(TripLocation source, TripLocation destination);
    }
}
