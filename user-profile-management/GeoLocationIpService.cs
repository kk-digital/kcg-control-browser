using System.Text.Json;

namespace user_profile_management;

public class GeoLocationIpService
{
    // Static HttpClient instance to reuse for all requests (best practice)
    private static readonly HttpClient _httpClient = new HttpClient();

    // Retrieves geolocation information for the specified IP address by calling a public API
    public ProxyIpGeoLocation GetGeoIpInfoAsync(string ipAddress)
    {
        // Construct the API URL with the given IP address
        string url = $"https://reallyfreegeoip.org/json/{ipAddress}";

        // Make a synchronous HTTP GET request to the API
        HttpResponseMessage response = _httpClient.GetAsync(url).GetAwaiter().GetResult();

        // Check if the response status indicates success
        if (!response.IsSuccessStatusCode)
        {
            // Log failure status and return null if request failed
            Console.WriteLine($"GeoIP lookup failed for {ipAddress}: {response.StatusCode}");
            return null;
        }

        // Read the response content as a JSON string
        string json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

        // Deserialize the JSON string into a ProxyIpGeoLocation object
        ProxyIpGeoLocation proxyIpGeoInfo = JsonSerializer.Deserialize<ProxyIpGeoLocation>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true  // Ignore case when matching JSON properties to C# properties
        });

        // Return the deserialized geolocation info object
        return proxyIpGeoInfo!;
    }
}