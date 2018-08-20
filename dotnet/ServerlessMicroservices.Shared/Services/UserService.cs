using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;

namespace ServerlessMicroservices.Shared.Services
{
    public class UserService : IUserService
    {
        const string GraphBaseUrl = "https://graph.windows.net/";
        const string GraphVersionQueryString = "?" + GraphVersion;
        const string GraphVersion = "api-version=1.6";

        private readonly AuthenticationContext _authContext;
        private readonly ClientCredential _clientCreds;
        private readonly string _graphUrl;

        public UserService(ISettingService settingService)
            : this(settingService.GetGraphTenantId(), settingService.GetGraphClientId(), settingService.GetGraphClientSecret())
        {
        }

        public UserService(string tenantId, string clientId, string clientSecret)
        {
            _graphUrl = GraphBaseUrl + tenantId;

            var authority = "https://login.microsoftonline.com/" + tenantId;
            _authContext = new AuthenticationContext(authority);
            _clientCreds = new ClientCredential(clientId, clientSecret);
        }

        async Task<string> GetAccessToken()
        {
            var authResult = await _authContext.AcquireTokenAsync(GraphBaseUrl, _clientCreds);
            return authResult.AccessToken;
        }

        public async Task<(User, string error)> CreateUser(CreateUser newUser)
        {
            var url = _graphUrl + "/users" + GraphVersionQueryString;

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
            var url = _graphUrl + "/users" + GraphVersionQueryString;

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

            var url = _graphUrl + "/users/" + userId + GraphVersionQueryString;

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
