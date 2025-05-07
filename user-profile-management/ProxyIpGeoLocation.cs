using System.Text.Json;
using System.Text.Json.Serialization;

namespace user_profile_management;

public class ProxyIpGeoLocation
{
    [JsonPropertyName("ip")]
    public string Ip { get; set; }

    [JsonPropertyName("time_zone")]
    public string TimeZone { get; set; }

    [JsonPropertyName("latitude")]
    public double Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double Longitude { get; set; }
    
    public static ProxyIpGeoLocation[] LoadFromJson(string jsonData)
    {
        JsonSerializerOptions options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        ProxyIpGeoLocation[] geoIpInfos = JsonSerializer.Deserialize<ProxyIpGeoLocation[]>(jsonData, options);
        return geoIpInfos;
    }
}