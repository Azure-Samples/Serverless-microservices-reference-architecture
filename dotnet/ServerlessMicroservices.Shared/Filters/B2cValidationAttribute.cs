using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using ServerlessMicroservices.Shared.Services;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ServerlessMicroservices.Shared.Filters
{
    /*
     * Filters are nice....but I could not find a way to abort the executing context if, for example, validation fails
     * I will leave this code as a reference.
     * 
     * To activate, we can decorate an API method with the [B2cValidation] attribute
     */
    public class B2cValidationAttribute : FunctionInvocationFilterAttribute
    {
        public override Task OnExecutingAsync(FunctionExecutingContext executingContext, CancellationToken cancellationToken)
        {
            var httpRequest = executingContext.Arguments.First().Value as HttpRequest;
            if (httpRequest == null)
                throw new ValidationException("Http Request is not the first argument!");

            var validationService = ServiceFactory.GetTokenValidationService();
            if (validationService.AuthEnabled)
            {
                //TODO: Not the best way to do this!!
                var user = validationService.AuthenticateRequest(httpRequest).Result;

                if (user == null)
                {
                    //httpRequest.HttpContext.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
                    //return Task.FromResult(0);
                    throw new ValidationException("Unauthorized!");
                }
            }

            return base.OnExecutingAsync(executingContext, cancellationToken);
        }
    }
}
