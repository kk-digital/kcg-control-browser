using System.Text.Json;
using LogUtility;

namespace user_profile_management;

public class ProxyGeoLocationProcessor
{
    // reads proxies from .txt file, retrieve geo location data for each and save to .json file
    public static bool RetrieveGeoLocationDataForProxies(string proxiesFilePath, string outputPath)
    {
        Random random = new Random();
        
        if (!File.Exists(proxiesFilePath))
        {
            LibLog.LogError("Proxies file not found: " + proxiesFilePath);
            return false;
        }

        GeoLocationIpService geoService = new GeoLocationIpService();
        List<ProxyIpGeoLocation> geoInfoList = new List<ProxyIpGeoLocation>();

        string[] lines = File.ReadAllLines(proxiesFilePath);

        if (lines == null || lines.Length == 0)
        {
            LibLog.LogError("Proxies file is empty: " + proxiesFilePath);
            return false;
        }
        
        LibLog.LogInfo($"Generating Geo Location data for the proxies file: {proxiesFilePath}");

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            string[] parts = line.Split(':');
            
            if (parts.Length < 1)
            {
                continue;
            }

            string ip = parts[0];
            ProxyIpGeoLocation proxyIpGeoInfo = geoService.GetGeoIpInfoAsync(ip);
            Thread.Sleep(random.Next(1000,2000));

            if (proxyIpGeoInfo != null)
            {
                geoInfoList.Add(proxyIpGeoInfo);
            }
        }

        JsonSerializerOptions jsonOptions = new JsonSerializerOptions();
        jsonOptions.WriteIndented = true;

        string json = JsonSerializer.Serialize(geoInfoList, jsonOptions);
        File.WriteAllText(outputPath, json);

        LibLog.LogInfo("Geo location data written to " + outputPath);
        return true;
    }
}
