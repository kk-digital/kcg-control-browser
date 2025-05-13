using System.Text.Json;
using System.Text.Json.Serialization;

namespace user_profile_management;

// represents the geographic metadata of an IP address
public class ProxyIpGeoLocation
{
    [JsonPropertyName("ip")] 
    public string Ip { get; set; }          // represents the geographic metadata of an IP address

    [JsonPropertyName("time_zone")]
    public string TimeZone { get; set; }    //The timezone associated with the IP's geographic region (e.g., "America/New_York").
                                            //Useful for synchronizing with browser profile

    [JsonPropertyName("latitude")]
    public double Latitude { get; set; }    // The geographic latitude. Helps when spoofing navigator.geolocation

    [JsonPropertyName("longitude")]         
    public double Longitude { get; set; }   // The geographic longitude. Paired with latitude for location emulation
    
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