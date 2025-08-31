using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using System.Xml;
using static Newtonsoft.Json.JsonConvert;

namespace ELibrary.Core.Helpers
{
    public interface IApiClient
    {
        Task<T> GetAsync<T>(string url);
        Task<T> PostAsync<T>(string url, object data, Dictionary<string, string> headers = null);
    }

    public class ApiClient : IApiClient
    {
        private readonly HttpClient _httpClient;

        public ApiClient()
        {
            HttpClientHandler httpClientHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (source, certificate, chain, sslPolicyError) =>
                {
                    return true;
                }
            };
            _httpClient = new HttpClient(httpClientHandler);

        }

        public async Task<T> GetAsync<T>(string url)
        {
            try
            {
                //_httpClient.BaseAddress = new Uri(url);
                var resp = await _httpClient.GetAsync(url);
                resp.EnsureSuccessStatusCode();

                var responseString = await resp.Content.ReadAsStringAsync();
                if (responseString.Contains("DOCTYPE"))
                {
                    return (T)(object)responseString;
                }
                return DeserializeObject<T>(responseString);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

        }

        public async Task<T> PostAsync<T>(string url, object data, Dictionary<string, string> headers = null)
        {
            string error = null;
            try
            {
                _httpClient.BaseAddress = new Uri(url);
                var content = CreateHttpContent(data);

                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                if (headers != null && headers.Count > 0)
                {
                    foreach (var header in headers)
                    {
                        _httpClient.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
                    }
                }
                var resp = await _httpClient.PostAsync(_httpClient.BaseAddress, content);
                var responseString = await resp.Content.ReadAsStringAsync();
                error = responseString;
                resp.EnsureSuccessStatusCode();
                return DeserializeObject<T>(responseString);
            }
            catch (Exception e)
            {
                throw new Exception(error ?? e.Message);
            }
        }

        private static HttpContent CreateHttpContent(object content)
        {
            DefaultContractResolver contractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            };

            var json = SerializeObject(content, new JsonSerializerSettings
            {
                ContractResolver = contractResolver,
                Formatting = Newtonsoft.Json.Formatting.Indented
            });
            return new StringContent(json, Encoding.UTF8, "application/json");
        }
    }
}
