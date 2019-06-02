# Gateway using API Management

- [Gateway using API Management](#gateway-using-api-management)
  - [Next steps](#next-steps)

There are many benefits to using an API manager. In the case the Rideshare solution, there are really four major benefits:

1. **Security**: the API manager layer verifies the incoming requests' [JWT](https://jwt.io/) token against the B2C Authority URL. This is accomplished via an inbound policy that intercepts each call:

```xml
<validate-jwt header-name="Authorization" failed-validation-httpcode="401" failed-validation-error-message="Unauthorized. Access token is missing or invalid.">
    <openid-config url="<--your_own_authorization_url-->" />
    <audiences>
        <audience><-- your_own_app_id --></audience>
    </audiences>
</validate-jwt>
```

**Please note** that Relecloud considered using Azure Functions [Filters](https://github.com/Azure/azure-webjobs-sdk/tree/dev/src/Microsoft.Azure.WebJobs.Host/Filters) to intercept HTTP calls and validate the JWT token in code instead of relying on an APIM layer. This has the advantage of applying security validation regardless of whether an APIM is used or not.

Here is the `Attribute` that was created:

```csharp
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
```

It can then be used to decorate a specific function like this:

```csharp
[B2cValidation]
[FunctionName("GetTrips")]
public static async Task<IActionResult> GetTrips([HttpTrigger(AuthorizationLevel.Function, "get", Route = "trips")] HttpRequest req,
    ILogger log)
{
...
}
```

It is very elegant and it actually does work! But unfortunately, it seems that it can only throw exceptions. Relecloud was not able to find a way to abort the HTTP request and throw a 401 status code. If an exception is thrown in the filter pipeline, the caller gets a 500 Internal Service Error which is hardly descriptive of the problem.

Eventually, Relecloud received an input from a security expert who advised that the `JWT Validation` be added to the code instead of APIM for the very same reason that the the HTTP endpoints will be protected regardless of whether APIM is used or not. To support this, the reference implementation includes a utility method that can be used to check the validation:

```csharp
public static async Task ValidateToken(HttpRequest request)
{
    var validationService = ServiceFactory.GetTokenValidationService();
    if (validationService.AuthEnabled)
    {
        var user = await validationService.AuthenticateRequest(request);
        if (user == null)
            throw new Exception(Constants.SECURITY_VALITION_ERROR);
    }
}
```

This method is used in each API Function to validate tokens and it throws a `known` exception. Upon exception, the function examines the exception to determine whether to send 401 (security check) or 400 (bad request) as shown here:

```csharp
[FunctionName("GetDrivers")]
public static async Task<IActionResult> GetDrivers([HttpTrigger(AuthorizationLevel.Function, "get",
        Route = "drivers")] HttpRequest req,
    ILogger log)
{
    log.LogInformation("GetDrivers triggered....");

    try
    {
        await Utilities.ValidateToken(req);
        var persistenceService = ServiceFactory.GetPersistenceService();
        return (ActionResult)new OkObjectResult(await persistenceService.RetrieveDrivers());
    }
    catch (Exception e)
    {
        var error = $"GetDrivers failed: {e.Message}";
        log.LogError(error);
        if (error.Contains(Constants.SECURITY_VALITION_ERROR))
            return new StatusCodeResult(401);
        else
            return new BadRequestObjectResult(error);
    }
}
```

**Please note** that the token validation is enforced only if the `AuthEnabled` setting is set to true.

2. **Documentation**: the API manager provides developers writing applications against RideShare APIs with a complete development portal for documentation and testing

3. **Usage Stats**: the API manager provides usage stats on all API calls (and report failures) which makes it really convenient to assess the API performance

4. **Rate Limiting**: the API manager can be configured to rate limit APIs based on IP origin, access, etc. This can be useful to prevent DOD attacks or provide different tiers of access based on users.

**Please note** that, in the case of Azure Functions, while the APIs are front-ended with an API manager (and hence shielded, protected, and rate limited), the APIs are still publicly available. This means that a DDOS attack or other attacks can still happen against the bare APIs if someone discovers them in the wild. However, you can restrict access to your Function Apps by [only allowing access to your API Management's static IP](https://docs.microsoft.com/azure/app-service/app-service-ip-restrictions) address. When you do this, only traffic that flows through API Management will be able to access your functions.

## Next steps

[Create the API Management Service](setup.md#create-the-api-management-service), then configure it:

- [Add APIM Products and APIs](setup.md#add-apim-products-and-apis)
  - [Drivers API](setup.md#drivers-api)
  - [Trips API](setup.md#trips-api)
  - [Passengers API](setup.md#passengers-api)

Read about Relecloud's chosen data storage solution:

- [Data storage](data-storage.md)
