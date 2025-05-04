using System.Text.Json;

namespace user_profile_management;

public class UserProfileManager
{
    public static ProxyIpWithProfiles LoadUserProfilesByIp(string proxyIp, string baseFolder)
    {
        string safeIp = proxyIp.Replace('.', '_');
        string proxyFolder = Path.Combine(baseFolder, "proxy_" + safeIp);

        if (!Directory.Exists(proxyFolder))
        {
            throw new DirectoryNotFoundException($"Proxy folder not found for IP {proxyIp} at '{proxyFolder}'");
        }
        List<BrowserProfile> profiles = new List<BrowserProfile>();

        string[] profileFolders = Directory.GetDirectories(proxyFolder, "profile_*");

        for (int i = 0; i < profileFolders.Length; i++)
        {
            string folder = profileFolders[i];
            string profileJsonPath = Path.Combine(folder, "profile.json");
            
            if (File.Exists(profileJsonPath))
            {
                BrowserProfile profile = JsonSerializer.Deserialize<BrowserProfile>(File.ReadAllText(profileJsonPath));
                profiles.Add(profile);
            }
        }

        return new ProxyIpWithProfiles
        {
            ProxyIp = proxyIp,
            Profiles = profiles
        };
    }
}