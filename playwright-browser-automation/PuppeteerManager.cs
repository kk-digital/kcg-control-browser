using Microsoft.Playwright;

namespace playwright_browser_automation;

// Example class demonstrating how to use the BrowserWindowManager and BrowserTabManager
// to automate browser windows and tabs using Playwright in C#.
public class PuppeteerManager
{
    // Playwright browser instance used to create browser contexts and pages.
    private IBrowser _browser;

    // Manager responsible for creating and managing browser tabs.
    private BrowserTabManager _tabManager;

    // Manager responsible for creating and managing browser windows.
    private BrowserWindowManager _windowManager;

    // Initializes Playwright, launches the Chromium browser, and sets up window and tab managers.
    public async Task InitializeAsync()
    {
        // Create Playwright instance
        IPlaywright playwright = await Playwright.CreateAsync();
        
        BrowserTypeLaunchOptions opt = new ();
        opt.ExecutablePath = @"";
        opt.Headless = false;
        opt.Args = new[] {
            "--disable-blink-features=AutomationControlled",  // Hide automation flag
            "--disable-web-security"
        };

        // Launch Chromium browser (headless by default)
        _browser = await playwright.Chromium.LaunchAsync(opt);

        // Initialize tab manager
        _tabManager = new BrowserTabManager();

        // Initialize window manager with the browser and tab manager
        _windowManager = new BrowserWindowManager(_browser, _tabManager);
    }

    // Demonstrates usage of the window and tab managers by creating windows and tabs,
    // navigating to URLs, switching tabs, and retrieving active window information.
    public async Task DemoUsage()
    {
        // Create first browser window with initial tab
        BrowserWindow window1 = await _windowManager.CreateWindowAsync();

        // Navigate the first tab in window1 to example.com
        await window1.Tabs[0].Page.GotoAsync("https://example.com");

        // Create a new tab in the active window (window1)
        BrowserTab tab2 = await _windowManager.CreateTabInActiveWindowAsync();

        // Navigate the new tab to google.com
        await tab2.Page.GotoAsync("https://google.com");

        // Create a second browser window with its own initial tab
        BrowserWindow window2 = await _windowManager.CreateWindowAsync();

        // Navigate the first tab in window2 to microsoft.com
        await window2.Tabs[0].Page.GotoAsync("https://microsoft.com");

        // Switch active tab in the active window to the second tab created earlier (tab2)
        await _windowManager.SwitchToTabAsync(tab2.Id);

        // Retrieve the currently active window
        BrowserWindow activeWindow = _windowManager.GetActiveWindow();

        if (activeWindow != null)
        {
            // Print the number of tabs in the active window
            Console.WriteLine("Active window has " + activeWindow.Tabs.Length + " tabs");
        }
    }
}
