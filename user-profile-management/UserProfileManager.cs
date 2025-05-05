using System.Runtime.CompilerServices;
using System.Text.Json;
using UtilityIO;

namespace user_profile_management;

public class UserProfileManager
{
    public static string UserProfilesBaseFolder;

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
    
    public static void GenerateProfilesWithProxies(string proxiesGeoLocationDataPath, string userAgentsPath, string baseFolder)
    {
        //PathUtils.GetFullPath("data/user-profiles");
        ProxyIpGeoLocation[] proxyIpGeoLocations = ProxyIpGeoLocation.LoadFromJson(File.ReadAllText(proxiesGeoLocationDataPath));
        string[] userAgents = UserAgentsLoader.LoadUserAgentsFromFile(userAgentsPath);
        int profilesPerProxy = userAgents.Length / proxyIpGeoLocations.Length;
        
        // Clean base folder
        if (Directory.Exists(baseFolder))
        {
            Directory.Delete(baseFolder, true);
        }

        Directory.CreateDirectory(baseFolder);
        int uniqueProfileCounter = 1;
        int userAgentIndex = 0;
        Random random = new Random();
        string prevAcceptLanguage = "";
        

        for (int p = 0; p < proxyIpGeoLocations.Length; p++)
        {
            string proxyIp = proxyIpGeoLocations[p].Ip;
            string ipName = proxyIp.Replace('.', '_');
            string proxyFolder = PathUtils.Combine(baseFolder, "proxy_" + ipName);

            Directory.CreateDirectory(proxyFolder);
            
            for (int i = 0; i < profilesPerProxy; i++)
            {
                string profileName = "profile_" + uniqueProfileCounter.ToString("D3");
                string profileFolder = PathUtils.Combine(proxyFolder, profileName);
                
                Directory.CreateDirectory(profileFolder);
                string userAgent = userAgents[userAgentIndex++];
                BrowserProfile profile;

                do
                {
                    profile = GetProfileByTimeZone(proxyIpGeoLocations[p].TimeZone);
                } while (profile.AcceptLanguage == prevAcceptLanguage);
                
                prevAcceptLanguage = profile.AcceptLanguage;
                
                profile.StorageStateFilePath = PathUtils.Combine(profileFolder, "storage-state.json");
                profile.Latitude = proxyIpGeoLocations[p].Latitude;
                profile.Longitude = proxyIpGeoLocations[p].Longitude;
                profile.UserAgent = userAgent;
                
                (int screenWidth, int screenHeight, int windowWidth, int windowHeight, int viewportWidth, int viewportHeight) dimensions = RealisticViewportGenerator.GetRandomWindowAndViewport();
                
                profile.DisplayResolution = new DisplayResolution()
                {
                    Width = dimensions.screenWidth,
                    Height = dimensions.screenHeight,
                };
                
                profile.BrowserWindowSize = new BrowserWindowSize()
                {
                    Width = dimensions.windowWidth,
                    Height = dimensions.windowHeight,
                };
                
                profile.BrowserContextViewportSize = new BrowserContextViewportSize()
                {
                    Width = dimensions.viewportWidth,
                    Height = dimensions.viewportHeight,
                };
                
                File.WriteAllText(PathUtils.Combine(profileFolder, "profile.json"),
                    JsonSerializer.Serialize(profile, new JsonSerializerOptions { WriteIndented = true }));

                uniqueProfileCounter++;
            }
        }

        Console.WriteLine($"✅ Generated {proxyIpGeoLocations.Length * profilesPerProxy} profiles grouped into proxy folders under '{baseFolder}'");
    }
    
    public static BrowserProfile GetProfileByTimeZone(string timeZone)
    {
        List<BrowserProfile> profiles = new List<BrowserProfile>
        {
            // en-US, q=0.9
            new BrowserProfile { AcceptLanguage = "en-US,en;q=0.9", Locale = "en-US", Timezone = "America/New_York" },
            new BrowserProfile { AcceptLanguage = "en-US,en;q=0.9", Locale = "en-US", Timezone = "America/Chicago" },
            new BrowserProfile { AcceptLanguage = "en-US,en;q=0.9", Locale = "en-US", Timezone = "America/Denver" },
            new BrowserProfile { AcceptLanguage = "en-US,en;q=0.9", Locale = "en-US", Timezone = "America/Los_Angeles" },
            new BrowserProfile { AcceptLanguage = "en-US,en;q=0.9", Locale = "en-US", Timezone = "America/Anchorage" },
            new BrowserProfile { AcceptLanguage = "en-US,en;q=0.9", Locale = "en-US", Timezone = "Pacific/Honolulu" },

            // en-US, q=0.8
            new BrowserProfile { AcceptLanguage = "en-US,en;q=0.8", Locale = "en-US", Timezone = "America/New_York" },
            new BrowserProfile { AcceptLanguage = "en-US,en;q=0.8", Locale = "en-US", Timezone = "America/Chicago" },
            new BrowserProfile { AcceptLanguage = "en-US,en;q=0.8", Locale = "en-US", Timezone = "America/Denver" },
            new BrowserProfile { AcceptLanguage = "en-US,en;q=0.8", Locale = "en-US", Timezone = "America/Los_Angeles" },
            new BrowserProfile { AcceptLanguage = "en-US,en;q=0.8", Locale = "en-US", Timezone = "America/Anchorage" },
            new BrowserProfile { AcceptLanguage = "en-US,en;q=0.8", Locale = "en-US", Timezone = "Pacific/Honolulu" },

            // es-US, q=0.9
            new BrowserProfile { AcceptLanguage = "es-US,es;q=0.9,en;q=0.7", Locale = "es-US", Timezone = "America/New_York" },
            new BrowserProfile { AcceptLanguage = "es-US,es;q=0.9,en;q=0.7", Locale = "es-US", Timezone = "America/Chicago" },
            new BrowserProfile { AcceptLanguage = "es-US,es;q=0.9,en;q=0.7", Locale = "es-US", Timezone = "America/Denver" },
            new BrowserProfile { AcceptLanguage = "es-US,es;q=0.9,en;q=0.7", Locale = "es-US", Timezone = "America/Los_Angeles" },
            new BrowserProfile { AcceptLanguage = "es-US,es;q=0.9,en;q=0.7", Locale = "es-US", Timezone = "America/Anchorage" },
            new BrowserProfile { AcceptLanguage = "es-US,es;q=0.9,en;q=0.7", Locale = "es-US", Timezone = "Pacific/Honolulu" }
        };

        ProfileSelector selector = new ProfileSelector(profiles);
        return selector.GetProfileByTimezone(timeZone);
    }
}