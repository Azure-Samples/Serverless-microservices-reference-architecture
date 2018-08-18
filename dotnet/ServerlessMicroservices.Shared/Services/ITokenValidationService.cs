using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace ServerlessMicroservices.Shared.Services
{
    public interface ITokenValidationService
    {
        Task<ClaimsPrincipal> AuthenticateRequest(HttpRequest request);
    }
}
