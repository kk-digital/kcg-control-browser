using lib;
using Microsoft.Playwright;

namespace playwright_browser_automation;

// Represents a single browser tab with a unique ID and associated Playwright page,
// including properties to track the tab's current title and URL.
public class BrowserTab
{
    // Unique identifier (UUID) for this tab.
    // This allows stable identification independent of tab position or order.
    public Uid64 Id { get; } = Uid64.CreateNewUid();

    // The Playwright page instance representing the actual browser tab.
    // This provides access to navigation, content, and browser interactions.
    public IPage Page { get; set; }

    // The title of the web page loaded in this tab.
    // Should be kept in sync with the current page title.
    public string Title { get; set; }

    // The URL of the web page loaded in this tab.
    // Should be kept in sync with the current page URL.
    public string Url { get; set; }
}
