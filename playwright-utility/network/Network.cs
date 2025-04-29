using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using LogUtility;

namespace UtilityNetwork
{
    public class NetworkClient
    {
        private readonly HttpClient _httpClient;
        private readonly int _maxRetries;
        private readonly TimeSpan _retryDelay;

        public NetworkClient(int maxRetries = 3, TimeSpan? retryDelay = null, string proxyUrl = null, string proxyUsername = null, string proxyPassword = null)
        {
            HttpClientHandler handler = new();

            if (!string.IsNullOrEmpty(proxyUrl))
            {
                WebProxy proxy = new(proxyUrl);
                if (!string.IsNullOrEmpty(proxyUsername))
                {
                    proxy.Credentials = new NetworkCredential(proxyUsername, proxyPassword);
                }
                handler.Proxy = proxy;
                handler.UseProxy = true;
            }

            _httpClient = new HttpClient(handler);
            _maxRetries = maxRetries;

            if (retryDelay != null)
            {
                _retryDelay = (TimeSpan)retryDelay;
            }
            else
            {
                _retryDelay = TimeSpan.FromSeconds(5);
            }
        }

        public async Task<NetworkResponse> GetAsync(string url)
        {
            return await ExecuteWithRetry(async () =>
            {
                return await SafeExecute(async () =>
                {
                    HttpResponseMessage response = await _httpClient.GetAsync(url);
                    string content = await response.Content.ReadAsStringAsync();
                    return new NetworkResponse
                    {
                        IsSuccess = response.IsSuccessStatusCode,
                        Content = content,
                        StatusCode = Convert.ToInt32(response.StatusCode)
                    };
                });
            });
        }

        public async Task<NetworkResponse> PostAsync(string url, string content, string contentType = "application/json")
        {
            return await ExecuteWithRetry(async () =>
            {
                return await SafeExecute(async () =>
                {
                    StringContent stringContent = new(content, System.Text.Encoding.UTF8, contentType);
                    HttpResponseMessage response = await _httpClient.PostAsync(url, stringContent);
                    string responseContent = await response.Content.ReadAsStringAsync();
                    return new NetworkResponse
                    {
                        IsSuccess = response.IsSuccessStatusCode,
                        Content = responseContent
                    };
                });
            });
        }

        public async Task<NetworkResponse> PutAsync(string url, string content, string contentType = "application/json")
        {
            return await ExecuteWithRetry(async () =>
            {
                return await SafeExecute(async () =>
                {
                    StringContent stringContent = new(content, System.Text.Encoding.UTF8, contentType);
                    HttpResponseMessage response = await _httpClient.PutAsync(url, stringContent);
                    string responseContent = await response.Content.ReadAsStringAsync();
                    return new NetworkResponse
                    {
                        IsSuccess = response.IsSuccessStatusCode,
                        Content = responseContent
                    };
                });
            });
        }

        public async Task<NetworkResponse> DeleteAsync(string url)
        {
            return await ExecuteWithRetry(async () =>
            {
                return await SafeExecute(async () =>
                {
                    HttpResponseMessage response = await _httpClient.DeleteAsync(url);
                    string responseContent = await response.Content.ReadAsStringAsync();
                    return new NetworkResponse
                    {
                        IsSuccess = response.IsSuccessStatusCode,
                        Content = responseContent
                    };
                });
            });
        }

        public async Task<NetworkResponse> DownloadFileAsync(string url, string filePath)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();

                long? contentLength = response.Content.Headers.ContentLength;

                using (Stream contentStream = await response.Content.ReadAsStreamAsync())
                using (FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                {
                    await contentStream.CopyToAsync(fileStream);
                }

                return new NetworkResponse { IsSuccess = true, Content = $"File downloaded to {filePath}", ContentLength = contentLength };
            }
            catch (Exception ex)
            {
                LibLog.LogError($"An error occurred: {ex.Message}");
                return new NetworkResponse { IsSuccess = false, Content = ex.Message };
            }
        }

        public async Task<NetworkResponse> UploadFileAsync(string url, byte[] fileBytes, string contentType, Dictionary<string, string> headers = null)
        {
            try
            {
                using MultipartFormDataContent content = new();
                using ByteArrayContent fileContent = new(fileBytes);
                fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);
                content.Add(fileContent, "file", "filename"); // "file" is the parameter name expected by the server

                if (headers != null)
                {
                    List<KeyValuePair<string, string>> headerList = headers.ToList();  // Convert to list so we can index

                    for (int i = 0; i < headerList.Count; i++)
                    {
                        content.Headers.Add(headerList[i].Key, headerList[i].Value);
                    }
                }

                HttpResponseMessage response = await _httpClient.PostAsync(url, content);
                response.EnsureSuccessStatusCode();
                string responseContent = await response.Content.ReadAsStringAsync();

                return new NetworkResponse { IsSuccess = true, Content = responseContent };
            }
            catch (Exception ex)
            {
                LibLog.LogError($"An error occurred: {ex.Message}");
                return new NetworkResponse { IsSuccess = false, Content = ex.Message };
            }
        }

        private async Task<NetworkResponse> ExecuteWithRetry(Func<Task<NetworkResponse>> action)
        {
            int retryCount = 0;
            while (true)
            {
                try
                {
                    return await action();
                }
                catch (Exception ex)
                {
                    if (retryCount >= _maxRetries)
                    {
                        LibLog.LogError($"Failed to execute action after {retryCount} retries: {ex.Message}");
                        return new NetworkResponse { IsSuccess = false, Content = null };
                    }

                    retryCount++;
                    LibLog.LogError($"Attempt {retryCount} failed: {ex.Message}. Retrying in {_retryDelay.TotalSeconds} seconds...");
                    await Task.Delay(_retryDelay);
                }
            }
        }

        public async Task<bool> IsServerAvailable(string serverUrl)
        {
            try
            {
                string healthCheckUrl = $"{serverUrl}/status/health";
                HttpResponseMessage response = await _httpClient.GetAsync(healthCheckUrl);
                bool isSuccess = response.IsSuccessStatusCode;
                string content = await response.Content.ReadAsStringAsync();

                return isSuccess;
            }
            catch
            {
                return false;
            }
        }

        private async Task<NetworkResponse> SafeExecute(Func<Task<NetworkResponse>> action)
        {
            try
            {
                return await action();
            }
            catch (Exception ex)
            {
                LibLog.LogError($"An error occurred: {ex.Message}");
                return new NetworkResponse { IsSuccess = false, Content = null };
            }
        }

        // TODO(ersonp): we need to get rid of Generic type parameters <T> as it is not allowed
        private async Task<T> SafeExecute<T>(Func<Task<NetworkResponse>> action, T defaultValue)
        {
            try
            {
                NetworkResponse response = await action();
                if (response.IsSuccess)
                {
                    return (T)(object)response;
                }
                return defaultValue;
            }
            catch (Exception ex)
            {
                LibLog.LogError($"An error occurred: {ex.Message}");
                return defaultValue;
            }
        }

        public static string HtmlDecode(string html)
        {
            return WebUtility.HtmlDecode(html);
        }
    }

    public class NetworkResponse
    {
        public bool IsSuccess { get; set; }
        public string Content { get; set; }
        public int StatusCode { get; set; }
        public long? ContentLength { get; set; }
    }
}
