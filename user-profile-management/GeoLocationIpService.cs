using System.Text.Json;

namespace user_profile_management;

public class GeoLocationIpService
{
    private static readonly HttpClient _httpClient = new HttpClient();
    public ProxyIpGeoLocation GetGeoIpInfoAsync(string ipAddress)
    {
        string url = $"https://reallyfreegeoip.org/json/{ipAddress}";
        HttpResponseMessage response = _httpClient.GetAsync(url).GetAwaiter().GetResult();

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"GeoIP lookup failed for {ipAddress}: {response.StatusCode}");
            return null;
        }

        string json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

        ProxyIpGeoLocation proxyIpGeoInfo = JsonSerializer.Deserialize<ProxyIpGeoLocation>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return proxyIpGeoInfo!;
    }
}