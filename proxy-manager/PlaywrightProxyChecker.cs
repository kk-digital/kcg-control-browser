using UtilityNetwork;

namespace PlaywrightProxyManager;

public class PuppeteerProxyChecker
{
    //============================================================================================================================
    public static async Task<bool> IsProxyWorking(string proxyAddress, int proxyPort, string username, string password)
    {
        string proxyUrl = $"http://{proxyAddress}:{proxyPort}";

        // Create NetworkClient with proxy settings
        NetworkClient client = new(proxyUrl: proxyUrl, proxyUsername: username, proxyPassword: password);

        try
        {
            // Send a test request
            NetworkResponse response = await client.GetAsync("http://www.example.com");

            // Check if the response was successful
            return response.IsSuccess;
        }
        catch (Exception)
        {
            // If an error occurs, the proxy is likely not working
            return false;
        }
    }
    //============================================================================================================================
}