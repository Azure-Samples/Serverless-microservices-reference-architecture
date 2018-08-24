using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using ServerlessMicroservices.Shared.Services;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ServerlessMicroservices.Shared.Helpers
{
    public class Utilities
    {
        private static Random rnd = new Random();

        public static double GenerateRandomAmount(double max = 3500)
        {
            //return rnd.NextDouble() * (maximum - minimum) + minimum; 
            double range = rnd.NextDouble();
            double rndValue = range * max;
            return rndValue;
        }

        public static int GenerateRandomInteger(int max = 10)
        {
            int rndValue = rnd.Next(0, max);
            return rndValue;
        }

        public static string GenerateRandomName(int nLength)
        {
            char[] chars = { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' };
            int charsNo = 26;
            int length = nLength;
            String rndString = "";

            for (int i = 0; i < length; i++)
                rndString += chars[rnd.Next(charsNo)];

            return rndString;
        }

        public static string GenerateRandomNumber(int nLength)
        {
            char[] chars = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
            int charsNo = 9;
            int length = nLength;
            String rndString = "";

            for (int i = 0; i < length; i++)
                rndString += chars[rnd.Next(charsNo)];

            return rndString;
        }

        public static string GenerateRandomAlphaNumeric(int nLength)
        {
            char[] chars = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' };
            int charsNo = 9 + 26;
            int length = nLength;
            String rndString = "";

            for (int i = 0; i < length; i++)
                rndString += chars[rnd.Next(charsNo)];

            return rndString.ToUpper();
        }

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

        public static async Task TriggerEventGridTopic<T>(HttpClient httpClient, T request, string eventType, string eventSubject, string eventGridTopicUrl, string eventGridTopicApiKey)
        {
            var error = "";

            try
            {
                if (string.IsNullOrEmpty(eventType) || string.IsNullOrEmpty(eventSubject) || string.IsNullOrEmpty(eventGridTopicUrl) || string.IsNullOrEmpty(eventGridTopicApiKey))
                    return;

                var events = new List<dynamic>
                {
                    new
                    {
                        EventType = eventType,
                        EventTime = DateTime.UtcNow,
                        Id = Guid.NewGuid().ToString(),
                        Subject = eventSubject,
                        Data = request
                    }
                };

                var headers = new Dictionary<string, string>() {
                    { "aeg-sas-key", eventGridTopicApiKey }
                };

                await Post<dynamic, dynamic>(httpClient, events, eventGridTopicUrl, headers);
            }
            catch (Exception ex)
            {
                error = ex.Message;
                throw ex;
            }
        }

        public static async Task<TResponse> Get<TResponse>(HttpClient httpClient, string url, Dictionary<string, string> headers, string userId = null, string password = null)
        {
            var error = "";
            HttpClient client = httpClient; 
            TResponse responseObject;
            bool isDispose = httpClient == null ? true : false;

            try
            {
                if (string.IsNullOrEmpty(url))
                    throw new Exception("No URL provided!");

                // If the client is provided, we assume the headers are pre-set
                if (client == null)
                {
                    client = new HttpClient();

                    if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(password))
                    {
                        var base64stuff = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{userId}:{password}"));
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64stuff);
                    }
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    if (headers != null)
                    {
                        foreach (KeyValuePair<string, string> entry in headers)
                        {
                            client.DefaultRequestHeaders.Add(entry.Key, entry.Value);
                        }
                    }
                }

                HttpResponseMessage response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                    responseObject = JsonConvert.DeserializeObject<TResponse>(await response.Content.ReadAsStringAsync());
                else
                    throw new Exception($"Bad return code: {response.StatusCode} - detail: {await response.Content.ReadAsStringAsync()}");
            }
            catch (Exception ex)
            {
                error = ex.Message;
                throw ex;
            }
            finally
            {
                if (isDispose && client != null) client.Dispose();
            }

            return responseObject;
        }

        public static async Task<TResponse> Post<TRequest, TResponse>(HttpClient httpClient, TRequest requestObject, string url, Dictionary<string, string> headers, string userId = null, string password = null)
        {
            var error = "";
            HttpClient client = httpClient; 
            TResponse responseObject;
            bool isDispose = httpClient == null ? true : false;

            try
            {
                if (string.IsNullOrEmpty(url))
                    throw new Exception("No URL provided!");

                var postData = JsonConvert.SerializeObject(requestObject,
                                                      new JsonSerializerSettings()
                                                      {
                                                          NullValueHandling = NullValueHandling.Ignore,
                                                          Formatting = Formatting.Indented, // for readability, change to None for compactness
                                                          ContractResolver = new CamelCasePropertyNamesContractResolver(),
                                                          DateTimeZoneHandling = DateTimeZoneHandling.Utc
                                                      });

                HttpContent httpContent = new StringContent(postData, Encoding.UTF8, "application/json");

                // If the client is provided, we assume the headers are pre-set
                if (client == null)
                {
                    client = new HttpClient();

                    if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(password))
                    {
                        var base64stuff = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{userId}:{password}"));
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64stuff);
                    }
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    if (headers != null)
                    {
                        foreach (KeyValuePair<string, string> entry in headers)
                        {
                            client.DefaultRequestHeaders.Add(entry.Key, entry.Value);
                        }
                    }
                }

                HttpResponseMessage response = await client.PostAsync(url, httpContent);

                if (response.IsSuccessStatusCode)
                    responseObject = JsonConvert.DeserializeObject<TResponse>(await response.Content.ReadAsStringAsync());
                else
                    throw new Exception($"Bad return code: {response.StatusCode} - detail: {await response.Content.ReadAsStringAsync()}");
            }
            catch (Exception ex)
            {
                error = ex.Message;
                throw ex;
            }
            finally
            {
                if (isDispose && client != null) client.Dispose();
            }

            return responseObject;
        }

        public static async Task<TResponse> Put<TRequest, TResponse>(HttpClient httpClient, TRequest requestObject, string url, Dictionary<string, string> headers, string userId = null, string password = null)
        {
            var error = "";
            HttpClient client = httpClient; 
            TResponse responseObject;
            bool isDispose = httpClient == null ? true : false;

            try
            {
                if (string.IsNullOrEmpty(url))
                    throw new Exception("No URL provided!");

                var putData = JsonConvert.SerializeObject(requestObject,
                                                      new JsonSerializerSettings()
                                                      {
                                                          NullValueHandling = NullValueHandling.Ignore,
                                                          Formatting = Formatting.Indented, // for readability, change to None for compactness
                                                          ContractResolver = new CamelCasePropertyNamesContractResolver(),
                                                          DateTimeZoneHandling = DateTimeZoneHandling.Utc
                                                      });
                HttpContent httpContent = new StringContent(putData, Encoding.UTF8, "application/json");

                // If the client is provided, we assume the headers are pre-set
                if (client == null)
                {
                    client = new HttpClient();

                    if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(password))
                    {
                        var base64stuff = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{userId}:{password}"));
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64stuff);
                    }
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    if (headers != null)
                    {
                        foreach (KeyValuePair<string, string> entry in headers)
                        {
                            client.DefaultRequestHeaders.Add(entry.Key, entry.Value);
                        }
                    }
                }

                HttpResponseMessage response = await client.PutAsync(url, httpContent);

                if (response.IsSuccessStatusCode)
                    responseObject = JsonConvert.DeserializeObject<TResponse>(await response.Content.ReadAsStringAsync());
                else
                    throw new Exception($"Bad return code: {response.StatusCode} - detail: {await response.Content.ReadAsStringAsync()}");
            }
            catch (Exception ex)
            {
                error = ex.Message;
                throw ex;
            }
            finally
            {
                if (isDispose && client != null) client.Dispose();
            }

            return responseObject;
        }

        public static async Task<TResponse> Delete<TResponse>(HttpClient httpClient, string url, Dictionary<string, string> headers, string userId = null, string password = null)
        {
            var error = "";
            HttpClient client = httpClient; 
            TResponse responseObject;
            bool isDispose = httpClient == null ? true : false;

            try
            {
                if (string.IsNullOrEmpty(url))
                    throw new Exception("No URL provided!");

                // If the client is provided, we assume the headers are pre-set
                if (client == null)
                {
                    client = new HttpClient();

                    if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(password))
                    {
                        var base64stuff = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{userId}:{password}"));
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64stuff);
                    }
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    if (headers != null)
                    {
                        foreach (KeyValuePair<string, string> entry in headers)
                        {
                            client.DefaultRequestHeaders.Add(entry.Key, entry.Value);
                        }
                    }
                }

                HttpResponseMessage response = await client.DeleteAsync(url);

                if (response.IsSuccessStatusCode)
                    responseObject = JsonConvert.DeserializeObject<TResponse>(await response.Content.ReadAsStringAsync());
                else
                    throw new Exception($"Bad return code: {response.StatusCode} - detail: {await response.Content.ReadAsStringAsync()}");
            }
            catch (Exception ex)
            {
                error = ex.Message;
                throw ex;
            }
            finally
            {
                if (isDispose && client != null) client.Dispose();
            }

            return responseObject;
        }
    }
}
