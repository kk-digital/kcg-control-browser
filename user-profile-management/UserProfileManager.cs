using System.Runtime.CompilerServices;
using System.Text.Json;
using LogUtility;
using UtilityIO;

namespace user_profile_management;

// use to manage browser user profiles

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
    
    public static void GenerateProfilesByProxies(string proxiesGeoLocationDataPath, string userAgentsPath, string baseFolder)
    {
        LibLog.LogInfo("Generating user profiles");
        
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

        LibLog.LogInfo($"✅ Generated {proxyIpGeoLocations.Length * profilesPerProxy} profiles grouped into proxy folders under '{baseFolder}'");
    }
    
    public static BrowserProfile GetProfileByTimeZone(string timeZone)
    {
        BrowserProfile[] profiles = new BrowserProfile[]
        {
            // en-US, q=0.9 - Mainland U.S. time zones
            new BrowserProfile { AcceptLanguage = "en-US,en;q=0.9", Locale = "en-US", Timezone = "America/New_York" },              // Eastern Time
            new BrowserProfile { AcceptLanguage = "en-US,en;q=0.9", Locale = "en-US", Timezone = "America/Detroit" },               // Eastern Time (Michigan)
            new BrowserProfile { AcceptLanguage = "en-US,en;q=0.9", Locale = "en-US", Timezone = "America/Kentucky/Louisville" },   // Eastern Time (Kentucky)
            new BrowserProfile { AcceptLanguage = "en-US,en;q=0.9", Locale = "en-US", Timezone = "America/Kentucky/Monticello" },   // Eastern Time (Kentucky)
            new BrowserProfile { AcceptLanguage = "en-US,en;q=0.9", Locale = "en-US", Timezone = "America/Indiana/Indianapolis" },  // Eastern Time (Indiana)
            new BrowserProfile { AcceptLanguage = "en-US,en;q=0.9", Locale = "en-US", Timezone = "America/Indiana/Vincennes" },     // Eastern Time (Indiana)
            new BrowserProfile { AcceptLanguage = "en-US,en;q=0.9", Locale = "en-US", Timezone = "America/Indiana/Winamac" },       // Eastern Time (Indiana)
            new BrowserProfile { AcceptLanguage = "en-US,en;q=0.9", Locale = "en-US", Timezone = "America/Indiana/Marengo" },       // Eastern Time (Indiana)
            new BrowserProfile { AcceptLanguage = "en-US,en;q=0.9", Locale = "en-US", Timezone = "America/Indiana/Petersburg" },    // Eastern Time (Indiana)
            new BrowserProfile { AcceptLanguage = "en-US,en;q=0.9", Locale = "en-US", Timezone = "America/Indiana/Vevay" },         // Eastern Time (Indiana)
            new BrowserProfile { AcceptLanguage = "en-US,en;q=0.9", Locale = "en-US", Timezone = "America/Chicago" },               // Central Time
            new BrowserProfile { AcceptLanguage = "en-US,en;q=0.9", Locale = "en-US", Timezone = "America/Indiana/Knox" },          // Central Time (Indiana)
            new BrowserProfile { AcceptLanguage = "en-US,en;q=0.9", Locale = "en-US", Timezone = "America/Menominee" },             // Central Time (Michigan)
            new BrowserProfile { AcceptLanguage = "en-US,en;q=0.9", Locale = "en-US", Timezone = "America/North_Dakota/Center" },   // Central Time (ND)
            new BrowserProfile { AcceptLanguage = "en-US,en;q=0.9", Locale = "en-US", Timezone = "America/North_Dakota/New_Salem" },// Central Time (ND)
            new BrowserProfile { AcceptLanguage = "en-US,en;q=0.9", Locale = "en-US", Timezone = "America/North_Dakota/Beulah" },   // Central Time (ND)
            new BrowserProfile { AcceptLanguage = "en-US,en;q=0.9", Locale = "en-US", Timezone = "America/Denver" },                // Mountain Time
            new BrowserProfile { AcceptLanguage = "en-US,en;q=0.9", Locale = "en-US", Timezone = "America/Boise" },                 // Mountain Time (Idaho)
            new BrowserProfile { AcceptLanguage = "en-US,en;q=0.9", Locale = "en-US", Timezone = "America/Phoenix" },               // Mountain Standard Time (Arizona - no DST)
            new BrowserProfile { AcceptLanguage = "en-US,en;q=0.9", Locale = "en-US", Timezone = "America/Los_Angeles" },           // Pacific Time
            new BrowserProfile { AcceptLanguage = "en-US,en;q=0.9", Locale = "en-US", Timezone = "America/Anchorage" },             // Alaska Time
            new BrowserProfile { AcceptLanguage = "en-US,en;q=0.9", Locale = "en-US", Timezone = "America/Juneau" },                // Alaska Time
            new BrowserProfile { AcceptLanguage = "en-US,en;q=0.9", Locale = "en-US", Timezone = "America/Nome" },                  // Alaska Time
            new BrowserProfile { AcceptLanguage = "en-US,en;q=0.9", Locale = "en-US", Timezone = "America/Sitka" },                 // Alaska Time
            new BrowserProfile { AcceptLanguage = "en-US,en;q=0.9", Locale = "en-US", Timezone = "America/Yakutat" },               // Alaska Time
            new BrowserProfile { AcceptLanguage = "en-US,en;q=0.9", Locale = "en-US", Timezone = "Pacific/Honolulu" },              // Hawaii-Aleutian Time

            // U.S. Territories
            new BrowserProfile { AcceptLanguage = "en-US,en;q=0.9", Locale = "en-US", Timezone = "America/Puerto_Rico" },           // Atlantic Time (Puerto Rico)
            new BrowserProfile { AcceptLanguage = "en-US,en;q=0.9", Locale = "en-US", Timezone = "America/St_Thomas" },             // Atlantic Time (U.S. Virgin Islands)
            new BrowserProfile { AcceptLanguage = "en-US,en;q=0.9", Locale = "en-US", Timezone = "Pacific/Samoa" },                 // Samoa Time (American Samoa)
            new BrowserProfile { AcceptLanguage = "en-US,en;q=0.9", Locale = "en-US", Timezone = "Pacific/Guam" },                  // Chamorro Time (Guam)
            new BrowserProfile { AcceptLanguage = "en-US,en;q=0.9", Locale = "en-US", Timezone = "Pacific/Marquesas" },             // Marquesas Time (U.S. Minor Outlying Islands)
            new BrowserProfile { AcceptLanguage = "en-US,en;q=0.9", Locale = "en-US", Timezone = "Pacific/Midway" },                // Samoa Time Zone (U.S. Minor Outlying Islands)

            // Repeat above with q=0.8 for AcceptLanguage
            new BrowserProfile { AcceptLanguage = "en-US,en;q=0.8", Locale = "en-US", Timezone = "America/New_York" },
            new BrowserProfile { AcceptLanguage = "en-US,en;q=0.8", Locale = "en-US", Timezone = "America/Detroit" },
            new BrowserProfile { AcceptLanguage = "en-US,en;q=0.8", Locale = "en-US", Timezone = "America/Kentucky/Louisville" },
            new BrowserProfile { AcceptLanguage = "en-US,en;q=0.8", Locale = "en-US", Timezone = "America/Kentucky/Monticello" },
            new BrowserProfile { AcceptLanguage = "en-US,en;q=0.8", Locale = "en-US", Timezone = "America/Indiana/Indianapolis" },
            new BrowserProfile { AcceptLanguage = "en-US,en;q=0.8", Locale = "en-US", Timezone = "America/Indiana/Vincennes" },
            new BrowserProfile { AcceptLanguage = "en-US,en;q=0.8", Locale = "en-US", Timezone = "America/Indiana/Winamac" },
            new BrowserProfile { AcceptLanguage = "en-US,en;q=0.8", Locale = "en-US", Timezone = "America/Indiana/Marengo" },
            new BrowserProfile { AcceptLanguage = "en-US,en;q=0.8", Locale = "en-US", Timezone = "America/Indiana/Petersburg" },
            new BrowserProfile { AcceptLanguage = "en-US,en;q=0.8", Locale = "en-US", Timezone = "America/Indiana/Vevay" },
            new BrowserProfile { AcceptLanguage = "en-US,en;q=0.8", Locale = "en-US", Timezone = "America/Chicago" },
            new BrowserProfile { AcceptLanguage = "en-US,en;q=0.8", Locale = "en-US", Timezone = "America/Indiana/Knox" },
            new BrowserProfile { AcceptLanguage = "en-US,en;q=0.8", Locale = "en-US", Timezone = "America/Menominee" },
            new BrowserProfile { AcceptLanguage = "en-US,en;q=0.8", Locale = "en-US", Timezone = "America/North_Dakota/Center" },
            new BrowserProfile { AcceptLanguage = "en-US,en;q=0.8", Locale = "en-US", Timezone = "America/North_Dakota/New_Salem" },
            new BrowserProfile { AcceptLanguage = "en-US,en;q=0.8", Locale = "en-US", Timezone = "America/North_Dakota/Beulah" },
            new BrowserProfile { AcceptLanguage = "en-US,en;q=0.8", Locale = "en-US", Timezone = "America/Denver" },
            new BrowserProfile { AcceptLanguage = "en-US,en;q=0.8", Locale = "en-US", Timezone = "America/Boise" },
            new BrowserProfile { AcceptLanguage = "en-US,en;q=0.8", Locale = "en-US", Timezone = "America/Phoenix" },
            new BrowserProfile { AcceptLanguage = "en-US,en;q=0.8", Locale = "en-US", Timezone = "America/Los_Angeles" },
            new BrowserProfile { AcceptLanguage = "en-US,en;q=0.8", Locale = "en-US", Timezone = "America/Anchorage" },
            new BrowserProfile { AcceptLanguage = "en-US,en;q=0.8", Locale = "en-US", Timezone = "America/Juneau" },
            new BrowserProfile { AcceptLanguage = "en-US,en;q=0.8", Locale = "en-US", Timezone = "America/Nome" },
            new BrowserProfile { AcceptLanguage = "en-US,en;q=0.8", Locale = "en-US", Timezone = "America/Sitka" },
            new BrowserProfile { AcceptLanguage = "en-US,en;q=0.8", Locale = "en-US", Timezone = "America/Yakutat" },
            new BrowserProfile { AcceptLanguage = "en-US,en;q=0.8", Locale = "en-US", Timezone = "Pacific/Honolulu" },
            new BrowserProfile { AcceptLanguage = "en-US,en;q=0.8", Locale = "en-US", Timezone = "America/Puerto_Rico" },
            new BrowserProfile { AcceptLanguage = "en-US,en;q=0.8", Locale = "en-US", Timezone = "America/St_Thomas" },
            new BrowserProfile { AcceptLanguage = "en-US,en;q=0.8", Locale = "en-US", Timezone = "Pacific/Samoa" },
            new BrowserProfile { AcceptLanguage = "en-US,en;q=0.8", Locale = "en-US", Timezone = "Pacific/Guam" },
            new BrowserProfile { AcceptLanguage = "en-US,en;q=0.8", Locale = "en-US", Timezone = "Pacific/Marquesas" },
            new BrowserProfile { AcceptLanguage = "en-US,en;q=0.8", Locale = "en-US", Timezone = "Pacific/Midway" }
        };

        ProfileSelector selector = new ProfileSelector(profiles);
        return selector.GetWeightedRandomProfileByTimeZone(timeZone);
    }
}