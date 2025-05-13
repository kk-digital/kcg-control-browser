namespace user_profile_management;

// represents a full browser identity or configuration.
public class BrowserProfile
{
    public string UserAgent { get; set; } // The browser’s user-agent string (e.g., Chrome on Windows, Firefox on Linux). Used to mimic specific browsers or devices.
    public string AcceptLanguage { get; set; } // Sets the Accept-Language HTTP header, e.g., "en-US,en;q=0.9". Affects site language and behavior.
    public string Locale { get; set; } // OS or browser locale settings (e.g., "en-US")
    public string Timezone { get; set; } // The browser’s timezone (e.g., "America/New_York"). Websites can detect mismatches with IP-based geolocation.
    public double Latitude { get; set; } // The geolocation values returned by navigator.geolocation. Often used by sites to validate regional access or delivery
    public double Longitude { get; set; }
    public DisplayResolution DisplayResolution { get; set; } // Represents the full screen resolution of the device (e.g., 1920x1080). Often matched with device emulation
    public BrowserWindowSize BrowserWindowSize { get; set; } // Size of the browser's outer window (including UI chrome). Important for simulating real browsing.
    public BrowserContextViewportSize BrowserContextViewportSize { get; set; } // The actual viewport area used to render pages. This is the inner window that websites can measure
    public string StorageStateFilePath { get; set; }    // Path to a JSON file storing the browser's cookies, localStorage, and sessionStorage, which can be loaded with Playwright using
                                                        // Browser.NewContextAsync(new() { StorageStatePath = path })
}
