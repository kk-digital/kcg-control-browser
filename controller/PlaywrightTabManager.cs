using Microsoft.Playwright;

namespace PlaywrightController;

// Manages browser tabs (pages) directly as IPage objects.
public class PlaywrightTabManager
{
    //===================================================================================================================
    // fields
    private PlaywrightController _context;
    private static List<IPage> _tabs;

    public IPage ActiveTab { get; private set; } = null;
    public int ActiveTabIndex { get; private set; } = -1;
    //===================================================================================================================
    // constructor
    public PlaywrightTabManager(PlaywrightController context)
    {
        _tabs = new();
        _context = context;
    }
    //===================================================================================================================
    private void AssertActiveTabIsTracked(IPage defaultPage = null)
    {
        if (ActiveTab == null)
        {
            if (defaultPage == null) 
                defaultPage = _tabs[0];

            SwitchActiveTab(defaultPage);
            ActiveTab = defaultPage;
            ActiveTabIndex = _tabs.IndexOf(defaultPage);
        }
    }
    //===================================================================================================================
    public IPage OpenNewTab()
    {
        IPage page = _context.Context.NewPageAsync().GetAwaiter().GetResult();
        _tabs.Add(page);

        AssertActiveTabIsTracked(page);
        return page;
    }
    //===================================================================================================================
    public void AssertActive(IPage page)
    {
        ActiveTab = page;
        ActiveTabIndex = _tabs.IndexOf(page);
        page.BringToFrontAsync().GetAwaiter().GetResult();
    }
    //===================================================================================================================
    public int getPageIndex(IPage page)
    {
        return _tabs.IndexOf(page);
    }
    //===================================================================================================================
    public IPage GetPageByIndex(int index)
    {
        return _tabs[index];
    }
    //===================================================================================================================
    public List<IPage> GetAllTabs()
    {
        return _tabs;
    }
    //===================================================================================================================
    public void SwitchActiveTab(IPage page)
    {
        page.BringToFrontAsync().GetAwaiter().GetResult();
        ActiveTab = page;
        ActiveTabIndex = _tabs.IndexOf(page);
    }
    //===================================================================================================================
    public void CloseTab(IPage page)
    {
        if (ActiveTab == page)
        {
            ActiveTab = null;
            ActiveTabIndex = -1;
        }

        page.CloseAsync().GetAwaiter().GetResult();
        _tabs.Remove(page);
        AssertActiveTabIsTracked();
    }
    //===================================================================================================================
    public string GetTabTitle(IPage page)
    {
        return page.TitleAsync().GetAwaiter().GetResult();
    }

    public string GetTabUrl(IPage page)
    {
        return page.Url;
    }
    //===================================================================================================================
    public string GetTabDOM(IPage page)
    {
        return page.ContentAsync().GetAwaiter().GetResult();
    }
    //===================================================================================================================
}
