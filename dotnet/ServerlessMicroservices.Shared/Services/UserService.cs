using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Identity.Client;
using Newtonsoft.Json;

namespace ServerlessMicroservices.Shared.Services
{
    public class UserService : IUserService
    {
        const string GraphBaseUrl = "https://graph.microsoft.com/v1.0";
        const string GraphVersionQueryString = "?";

        private string[] _scopes = new[] { "https://graph.microsoft.com/.default" };
        private IConfidentialClientApplication _app;
        private readonly string _authority = "https://login.microsoftonline.com/";



        public UserService(ISettingService settingService)
            : this(settingService.GetGraphTenantId(), settingService.GetGraphClientId(), settingService.GetGraphClientSecret())
        {
        }

        public UserService(string tenantId, string clientId, string clientSecret)
        {
            if (string.IsNullOrEmpty(tenantId)) throw new ArgumentNullException(nameof(tenantId), "GraphTenantId environment variable must be set before instantiating UserService.");

            _authority = _authority + tenantId;

            _app = ConfidentialClientApplicationBuilder.Create(clientId)
                                          .WithClientSecret(clientSecret)
                                          .WithAuthority(new Uri(_authority))
                                          .Build();
        }

        async Task<string> GetAccessToken()
        {
            var result = await _app.AcquireTokenForClient(_scopes)
                  .ExecuteAsync();
            return result.AccessToken;
        }

        public async Task<(User, string error)> CreateUser(CreateUser newUser)
        {
            var url = GraphBaseUrl + "/users";

            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GetAccessToken());

            var payload = new StringContent(JsonConvert.SerializeObject(newUser), Encoding.UTF8, "application/json");
            var response = await client.PostAsync(url, payload);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var user = JsonConvert.DeserializeObject<User>(json);
                return (user, null);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var json = await response.Content.ReadAsStringAsync();
                var badRequest = JsonConvert.DeserializeObject<BadRequestResponse>(json);
                return (null, badRequest.ErrorMessage);
            }
            else
            {
                return (null, "Error Creating User. HTTP Status Code: " + (int)response.StatusCode);
            }
        }

        public async Task<(IEnumerable<User>, string error)> GetUsers()
        {
            var url = GraphBaseUrl + "/users";

            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GetAccessToken());

            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<UsersResult>(json);
                return (result.Value, null);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var json = await response.Content.ReadAsStringAsync();
                var badRequest = JsonConvert.DeserializeObject<BadRequestResponse>(json);
                return (null, badRequest.ErrorMessage);
            }
            else
            {
                return (null, "Error Getting Users. HTTP Status Code: " + (int)response.StatusCode);
            }
        }

        public async Task<(User, string error)> GetUserById(string userId)
        {
            if (String.IsNullOrWhiteSpace(userId)) throw new ArgumentNullException(nameof(userId));

            var url = GraphBaseUrl + "/users/" + userId;

            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GetAccessToken());

            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<User>(json);
                return (result, null);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var json = await response.Content.ReadAsStringAsync();
                var badRequest = JsonConvert.DeserializeObject<BadRequestResponse>(json);
                return (null, badRequest.ErrorMessage);
            }
            else
            {
                return (null, "Error Getting User. HTTP Status Code: " + (int)response.StatusCode);
            }
        }

    }
}
