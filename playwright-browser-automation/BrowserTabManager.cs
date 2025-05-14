using lib;
using Microsoft.Playwright;

namespace playwright_browser_automation;

// Manages browser tabs by creating, storing, retrieving, and closing them.
// Uses an internal array to hold BrowserTab instances and resizes it as needed.
public class BrowserTabManager
{
    // Internal array storing all browser tabs managed by this instance.
    private BrowserTab[] _tabs = Array.Empty<BrowserTab>();
    
    // Returns the full HTML content (DOM) of the tab
    public string DomContent { get; set; }

    // Creates a new BrowserTab for the given Playwright page,
    // adds it to the internal array, and returns the new tab.
    public BrowserTab CreateTab(IPage page)
    {
        BrowserTab tab = new BrowserTab();
        tab.Page = page;
        AddTab(tab);
        return tab;
    }

    // Adds a BrowserTab to the internal array, resizing the array to fit.
    private void AddTab(BrowserTab tab)
    {
        BrowserTab[] newTabs = new BrowserTab[_tabs.Length + 1];
        for (int i = 0; i < _tabs.Length; i++)
        {
            newTabs[i] = _tabs[i];
        }
        newTabs[_tabs.Length] = tab;
        _tabs = newTabs;
    }

    // Removes a BrowserTab with the specified tabId from the internal array.
    // Resizes the array to exclude the removed tab.
    private void RemoveTab(Uid64 tabId)
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
            // Tab not found, nothing to remove
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
    }

    // Closes the tab with the specified tabId by closing its Playwright page,
    // then removes it from the internal array.
    public void CloseTab(Uid64 tabId)
    {
        BrowserTab tab = GetTab(tabId);
        if (tab != null && tab.Page != null)
        {
            // Close the Playwright page synchronously
            tab.Page.CloseAsync().GetAwaiter().GetResult();
            RemoveTab(tabId);
        }
    }

    // Retrieves the BrowserTab object with the specified tabId.
    // Returns null if no matching tab is found.
    public BrowserTab GetTab(Uid64 tabId)
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

    // Returns the title of the tab with the specified tabId.
    public string GetTabTitle(Uid64 tabId)
    {
        BrowserTab tab = GetTab(tabId);
        if (tab != null)
        {
            return tab.Title;
        }
        else
        {
            return string.Empty;
        }
    }

    // Returns the URL of the tab with the specified tabId.
    public string GetTabUrl(Uid64 tabId)
    {
        BrowserTab tab = GetTab(tabId);
        if (tab != null)
        {
            return tab.Url;
        }
        else
        {
            return string.Empty;
        }
    }
    
    // Returns the full HTML content (DOM) of the tab identified by tabId.
    // Returns null if the tab or its page is not found.
    public async Task GetTabDomAsync(Uid64 tabId)
    {
        BrowserTab tab = GetTab(tabId);
        
        if (tab == null || tab.Page == null)
        {
            DomContent = string.Empty;
        }

        // Get the full HTML content of the page (DOM)
        DomContent = await tab.Page.ContentAsync();
    }
}
