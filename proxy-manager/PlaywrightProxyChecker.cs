using System.Net;
using xlibHttpServer;
using StreamReader = System.IO.StreamReader;

namespace PlaywrightProxyManager;

public class PlaywrightProxyChecker
{
    public static bool IsProxyWorking(string proxyAddress, int proxyPort, string username, string password)
    {
        string proxyUrl = $"http://{proxyAddress}:{proxyPort}";

        // Create NetworkClient with proxy settings
        NetworkClient client = new(proxyUrl: proxyUrl, proxyUsername: username, proxyPassword: password);

        try
        {
            // Send a test request
            NetworkResponse response = client.GetAsync("http://www.example.com").GetAwaiter().GetResult();

            // Check if the response was successful
            return response.IsSuccess;
        }
        catch (Exception)
        {
            // If an error occurs, the proxy is likely not working
            return false;
        }
    }

    public static bool IsValidIpAddress(string ipAddress)
    {
        IPAddress ip;
        bool result = IPAddress.TryParse(ipAddress, out ip);

        return result && ip.ToString() == ipAddress;
    }

    public static bool IsValidPortNumber(string sPortNumber)
    {
        // port must be an integer between 1 and 65535
        if (!int.TryParse(sPortNumber, out int port) || port < 1 || port > 65535)
        {
            return false;
        }

        return true;
    }

    public static bool IsValidProxyEntry(string proxyEntry)
    {
        // eg. 168.111.222.132:4000:username:password
        if (string.IsNullOrEmpty(proxyEntry))
        {
            return false;
        }

        // Split the string by ':'
        string[] parts = proxyEntry.Split(':');
        if (parts.Length != 4)
        {
            return false;
        }

        string ipPart = parts[0];
        string portPart = parts[1];
        string userNamePart = parts[2];
        string passwordPart = parts[3];

        if (!IsValidIpAddress(ipPart))
        {
            return false;
        }

        if (!IsValidPortNumber(portPart))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(userNamePart))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(passwordPart))
        {
            return false;
        }

        // All checks passed
        return true;
    }

    // validates a proxy file
    public static bool IsValidProxyFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return false;
        }

        StreamReader reader = null;

        try
        {
            reader = new StreamReader(filePath);
            string line = reader.ReadLine();

            while (line != null)
            {
                if (!IsValidProxyEntry(line))
                {
                    return false; // Found invalid line, return false immediately
                }

                line = reader.ReadLine();
            }
        }
        finally
        {
            if (reader != null)
            {
                reader.Dispose();
            }
        }

        return true; // All lines are valid
    }
}