using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace ServerlessMicroservices.Shared.Services
{
    public interface ITokenValidationService
    {
        /// <summary>
        /// Configured by the function app settings. If false, validation is skipped.
        /// </summary>
        /// <returns></returns>
        bool AuthEnabled { get; }
        Task<ClaimsPrincipal> AuthenticateRequest(HttpRequest request);
    }
}
