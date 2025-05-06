using System.Text.Json;
using context_storage_state_schema;
using Microsoft.Playwright;
using user_profile_management;
using UtilityIO;

namespace PlaywrightController;
public class PlaywrightController
{
    public const string ChromiumUserDataPath = "data/chromium-user-data";
    public string ProxyUrl { get; set; } = "";
    public string ChromiumPath { get; set; }
    private string ProxyUrlUsername { get; set; }
    private string ProxyUrlPassword { get; set; }
    public IBrowserContext Context { get; set; }
    public bool Headless { get; set; } = false;
    
    public string ContextStorageStateFilePath { get; set; } // path of the .json file that stores the storage state data for the browser context

    public PlaywrightController(string chromiumPath, string proxyUrl = "", string proxyUrlUsername = "", string proxyUrlPassword = "", bool headless = false)
    {
        ChromiumPath = chromiumPath;
        ProxyUrl = proxyUrl;
        ProxyUrlUsername = proxyUrlUsername;
        ProxyUrlPassword = proxyUrlPassword;
        Headless = headless;
    }

    public void InitializeBrowserOld(string userDataDir = "")
    {
        IPlaywright playWright = Playwright.CreateAsync().GetAwaiter().GetResult();
        BrowserTypeLaunchPersistentContextOptions opt = new BrowserTypeLaunchPersistentContextOptions();
        opt.ExecutablePath = ChromiumPath;
        opt.Headless = Headless;
        opt.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36";

        if (ProxyUrl != "")
        {
            Proxy proxy = new Proxy();
            proxy.Server = ProxyUrl;
            proxy.Username = ProxyUrlUsername;
            proxy.Password = ProxyUrlPassword;

            opt.Proxy = proxy;
        }

        opt.Args = new[] {
                        "--disable-blink-features=AutomationControlled",
                        "--disable-web-security"
                        };
        
        if (string.IsNullOrEmpty(userDataDir))
            Context = playWright.Chromium.LaunchPersistentContextAsync(ChromiumUserDataPath, opt).GetAwaiter().GetResult();
        else
            Context = playWright.Chromium.LaunchPersistentContextAsync(userDataDir, opt).GetAwaiter().GetResult();
    }
    
    public void InitializeBrowser()
    {
        IPlaywright playwright = Playwright.CreateAsync().GetAwaiter().GetResult();
        BrowserTypeLaunchOptions opt = new BrowserTypeLaunchOptions();
        opt.ExecutablePath = ChromiumPath;
        opt.Headless = Headless;
        opt.Args = new[] {
            "--disable-blink-features=AutomationControlled",
            "--disable-web-security"
        };

        IBrowser browser = playwright.Chromium.LaunchAsync(opt).GetAwaiter().GetResult();
        string ip = ProxyUrl.Split(":").First();
        ProxyIpWithProfiles profiles = UserProfileManager.LoadUserProfilesByIp(ip, UserProfileManager.UserProfilesBaseFolder);
        BrowserProfile profile = profiles.GetProfileToUse();
        BrowserNewContextOptions newContextOptions = new BrowserNewContextOptions();
        ContextStorageStateFilePath = profile.StorageStateFilePath;
        
        if (!string.IsNullOrWhiteSpace(ContextStorageStateFilePath) && File.Exists(ContextStorageStateFilePath))
        {
            newContextOptions.StorageStatePath = ContextStorageStateFilePath;
        }
        
        Proxy proxy = new Proxy();
        proxy.Server = ProxyUrl;
        proxy.Username = ProxyUrlUsername;
        proxy.Password = ProxyUrlPassword;
        
        newContextOptions.Proxy = proxy;
        newContextOptions.UserAgent = profile.UserAgent;
        
        ViewportSize viewportSize = new ViewportSize();
        viewportSize.Height = profile.BrowserContextViewportSize.Height;
        viewportSize.Width = profile.BrowserContextViewportSize.Width;
        newContextOptions.ViewportSize = viewportSize;
        
        Geolocation geolocation = new Geolocation();
        geolocation.Latitude = (float)profile.Latitude;
        geolocation.Longitude = (float)profile.Longitude;
        newContextOptions.Geolocation = geolocation;
        
        newContextOptions.Locale = profile.Locale;
        
        ScreenSize screenSize = new ScreenSize();
        screenSize.Height = profile.BrowserWindowSize.Height;
        screenSize.Width = profile.BrowserWindowSize.Width;
        newContextOptions.ScreenSize = screenSize;
        
        newContextOptions.TimezoneId = profile.Timezone;
        newContextOptions.BypassCSP = true;
        
        Context = browser.NewContextAsync(newContextOptions).GetAwaiter().GetResult();
        
        string spoofScript = SpoofScriptGenerator.GenerateSpoofScript(profile.DisplayResolution, UsTimeZoneExtensions.Resolve(profile.Timezone),
            profile.Locale,profile.UserAgent,profile.BrowserContextViewportSize);

        Context.AddInitScriptAsync(spoofScript).GetAwaiter().GetResult();
    }
    
    public void CloseBrowser()
    {
        try
        {
            if (Context != null)
            {
                Context.CloseAsync().GetAwaiter().GetResult();
            }
        }
        catch (Exception)
        {
        }
    }
    
    public void CloseBrowserAfterSavedStorageState(string storageStateFilePath)
    {
        try
        {
            if (Context != null)
            {
                // Assume jsonString is your compact JSON string
                string jsonString = Context.StorageStateAsync().GetAwaiter().GetResult();
                ContextStorageState contextStorageState = JsonSerializer.Deserialize<ContextStorageState>(jsonString);

                JsonSerializerOptions options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };
                string prettyJson = JsonSerializer.Serialize(contextStorageState, options);
                File.WriteAllText(storageStateFilePath, prettyJson);
                Context.CloseAsync().GetAwaiter().GetResult();
            }
        }
        catch (Exception)
        {
        }
    }
}
