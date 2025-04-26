using Microsoft.Playwright;

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

    public PlaywrightController(string chromiumPath, string proxyUrl = "", string proxyUrlUsername = "", string proxyUrlPassword = "", bool headless = false)
    {
        ChromiumPath = chromiumPath;
        ProxyUrl = proxyUrl;
        ProxyUrlUsername = proxyUrlUsername;
        ProxyUrlPassword = proxyUrlPassword;
        Headless = headless;
    }

    public void InitializeBrowser(string userDataDir = "")
    {
        IPlaywright playWright = Playwright.CreateAsync().GetAwaiter().GetResult();
        BrowserTypeLaunchPersistentContextOptions opt = new();
        opt.ExecutablePath = ChromiumPath;
        opt.Headless = Headless;
        opt.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36";

        if (ProxyUrl != "")
        {
            Proxy proxy = new();
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
}
