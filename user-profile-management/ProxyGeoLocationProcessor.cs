using System.Text.Json;
using LogUtility;

namespace user_profile_management;

public class ProxyGeoLocationProcessor
{
    // Reads proxy IPs from a .txt file, retrieves geolocation data for each IP,
    // and saves the collected data to a .json file
    public static bool RetrieveGeoLocationDataForProxies(string proxiesFilePath, string outputPath)
    {
        Random random = new Random();
        
        // Check if the proxy file exists; log error and return false if not found
        if (!File.Exists(proxiesFilePath))
        {
            LibLog.LogError("Proxies file not found: " + proxiesFilePath);
            return false;
        }

        // Create an instance of the GeoLocationIpService to fetch geo info
        GeoLocationIpService geoService = new GeoLocationIpService();
        
        // List to hold geolocation info for each proxy IP
        List<ProxyIpGeoLocation> geoInfoList = new List<ProxyIpGeoLocation>();

        // Read all lines (proxy entries) from the input file
        string[] lines = File.ReadAllLines(proxiesFilePath);

        // Check if file is empty; log error and return false if so
        if (lines == null || lines.Length == 0)
        {
            LibLog.LogError("Proxies file is empty: " + proxiesFilePath);
            return false;
        }
        
        LibLog.LogInfo($"Generating Geo Location data for the proxies file: {proxiesFilePath}");

        // Process each line in the proxies file
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            
            // Skip empty or whitespace-only lines
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            // Split the line by ':' to separate IP and port (if present)
            string[] parts = line.Split(':');
            
            // Skip if no IP part found
            if (parts.Length < 1)
            {
                continue;
            }

            // Extract the IP address (first part)
            string ip = parts[0];

            // Retrieve geolocation info for the IP address (blocking call)
            ProxyIpGeoLocation proxyIpGeoInfo = geoService.GetGeoIpInfoAsync(ip);

            // Sleep for 1-2 seconds randomly to avoid hammering the API
            Thread.Sleep(random.Next(1000, 2000));

            // If valid geolocation info was retrieved, add it to the list
            if (proxyIpGeoInfo != null)
            {
                geoInfoList.Add(proxyIpGeoInfo);
            }
        }

        // Configure JSON serializer options to format output with indentation
        JsonSerializerOptions jsonOptions = new JsonSerializerOptions();
        jsonOptions.WriteIndented = true;

        // Serialize the list of geolocation info objects to JSON string
        string json = JsonSerializer.Serialize(geoInfoList, jsonOptions);

        // Write the JSON string to the specified output file
        File.WriteAllText(outputPath, json);

        // Log successful completion
        LibLog.LogInfo("Geo location data written to " + outputPath);

        // Indicate success
        return true;
    }
}
