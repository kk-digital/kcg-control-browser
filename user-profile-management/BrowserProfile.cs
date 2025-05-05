namespace user_profile_management;

public class BrowserProfile
{
    public string UserAgent { get; set; }
    public string AcceptLanguage { get; set; }
    public string Locale { get; set; }
    public string Timezone { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DisplayResolution DisplayResolution { get; set; }
    public BrowserWindowSize BrowserWindowSize { get; set; }
    public BrowserContextViewportSize BrowserContextViewportSize { get; set; }
    public string StorageStateFilePath { get; set; }    // file path of .json that stores cookies, localStorage, sessionStorage
}
