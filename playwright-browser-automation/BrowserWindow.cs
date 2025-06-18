using XlibUid;
using Microsoft.Playwright;

namespace playwright_browser_automation;

// Represents a browser window which contains multiple tabs (pages) within a single browser context.
// Each window has a unique identifier and manages its collection of tabs using an internal array.
public class BrowserWindow
{
    // Unique identifier (UUID) for this browser window.
    // Allows stable identification independent of window order or position.
    public Uid64 Id { get; }

    // The Playwright browser context associated with this window.
    // Provides isolation for cookies, cache, and storage shared by all tabs in this window.
    public IBrowserContext Context { get; set; }

    // Internal array holding all tabs (BrowserTab instances) opened in this window.
    private BrowserTab[] _tabs = Array.Empty<BrowserTab>();

    // Public read-only property exposing the tabs array.
    public BrowserTab[] Tabs
    {
        get { return _tabs; }
    }

    // The unique identifier of the currently active tab in this window.
    // Uid64.Default indicates that no tab is currently active.
    public Uid64 ActiveTabId = Uid64.Default;
    
    // manage network requests
    RequestManager RequestManager = new RequestManager();

    // constructor
    public BrowserWindow()
    {
        Id = Uid64.CreateNewUid();
    }

    // Adds a new tab to this window.
    // Resizes the internal array to accommodate the new tab.
    public void AddTab(BrowserTab tab)
    {
        BrowserTab[] newTabs = new BrowserTab[_tabs.Length + 1];
        for (int i = 0; i < _tabs.Length; i++)
        {
            newTabs[i] = _tabs[i];
        }
        newTabs[_tabs.Length] = tab;
        _tabs = newTabs;
    }

    // Removes a tab with the specified tabId from this window.
    // Resizes the internal array to exclude the removed tab.
    // If the removed tab was active, clears the active tab ID.
    public void RemoveTab(Uid64 tabId)
    {
        int index = -1;
        for (int i = 0; i < _tabs.Length; i++)
        {
            if (_tabs[i].Id == tabId)
            {
                index = i;
                break;
            }
        }
        if (index == -1)
        {
            // Tab not found, no action needed.
            return;
        }

        BrowserTab[] newTabs = new BrowserTab[_tabs.Length - 1];
        int newIndex = 0;
        for (int i = 0; i < _tabs.Length; i++)
        {
            if (i != index)
            {
                newTabs[newIndex++] = _tabs[i];
            }
        }
        _tabs = newTabs;

        // Clear active tab if it was the one removed
        if (ActiveTabId == tabId)
        {
            ActiveTabId = Uid64.Default;
        }
    }

    // Finds and returns the tab with the specified tabId.
    // Returns null if no matching tab is found.
    public BrowserTab FindTab(Uid64 tabId)
    {
        for (int i = 0; i < _tabs.Length; i++)
        {
            if (_tabs[i].Id == tabId)
            {
                return _tabs[i];
            }
        }
        return null;
    }
}
