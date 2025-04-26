using LogUtility;
using Microsoft.Playwright;
using PlaywrightClient;

namespace PlaywrightClient;
    
/// <summary>
/// Manages browser connections and tab interactions.
/// </summary>
public class PlaywrightTabManager
{
// fields
private IBrowserContext _context;
static List<Tab> _tabs; // store list of currently open tabs
                        //===================================================================================================================
                        // properties
public int ActiveTabIndex { get; set; } = 0;
//===================================================================================================================
// constructor
public PlaywrightTabManager(IBrowserContext context)
{
    _tabs = new List<Tab>();
    _context = context;
}
//===================================================================================================================
public IPage OpenNewTab(string URL, IBrowserContext context, PageGotoOptions opt)
{
    try
    {
        IPage page;

        page = context.NewPageAsync().GetAwaiter().GetResult();
        page.GotoAsync(URL, opt).GetAwaiter().GetResult();

        Tab tab = new (context);
        tab.Page = page;
        _tabs.Add(tab); // add to list of opened tabs
        SwitchActiveTab(tab.Id); // switch active tab to newly opened tab
        return page;
    }
    catch (Exception ex)
    {
        LibLog.LogError(ex.Message.ToString());
        return null;
    }
}
//===================================================================================================================
public void ListOpenTabs()
{
    Tab tab;
    
    for (int i = 0; i < _tabs.Count; i++)
    {
        tab = _tabs[i];
        LibLog.LogInfo($"Tab Id: {tab.Id} Url: {tab.Url}");
    }
}
//===================================================================================================================
public List<Tab> GetAllTabs()
{
    return _tabs;
}
//===================================================================================================================
public void SwitchActiveTab(int tabID)
{
    int n = _tabs.FindIndex(t => t.Id == tabID);

    if (n > -1) // if found
    {
        _tabs[n].Page.BringToFrontAsync();
        ActiveTabIndex = tabID;
    }
}
//===================================================================================================================
public void SwitchActivePage(IPage page)
{
    try
    {
        page.BringToFrontAsync().GetAwaiter().GetResult();
    }
    catch (Exception)
    {
    }
}
//===================================================================================================================
public async Task CloseTab(int tabID)
{
    int n = _tabs.FindIndex(t => t.Id == tabID);

    if (n > -1) // if found
    {
        if (n == _tabs.Count - 1) //if closing the last tab
        {
            if (ActiveTabIndex == tabID) //if closing the active tab
            {
                if (n - 1 > -1) // if remaining tabs exists after closing this tab
                {
                    SwitchActiveTab(_tabs[n - 1].Id); // switch to the next tab on its left side
                }
            }
        }

        // if there will be no more open tabs after closing this one, reset tab ID generator back to 0.
        if (_tabs.Count <= 1) _tabs[n].ResetInitialTabId();
        await _tabs[n].Page.CloseAsync();
        _tabs.Remove(_tabs[n]);
    }
}
//===================================================================================================================
public string GetTabDOM(int tabID)
{
    int n = _tabs.FindIndex(t => t.Id == tabID);

    if (n > -1) return _tabs[n].Page.ContentAsync().GetAwaiter().GetResult(); // if found
    return null;
}
//===================================================================================================================
// scroll mouse wheel vertically <delta> pixels from current position.
// positive delta scrolls down, while negative delta scrolls up
public void ScrollMouseWheel(int tabID, int delta)
{
    int n = _tabs.FindIndex(t => t.Id == tabID);

    if (n > -1)
    {
        SwitchActiveTab(tabID);
        Task.Delay(3000);
        _tabs[n].Page.Mouse.WheelAsync(0, delta).GetAwaiter().GetResult(); // if found
    }
}
//===================================================================================================================
// automate page scroll for 1 window height
public void ScrollViewport(IPage page)
{
    page.EvaluateAsync(@"() => {
                        window.scrollBy(0, window.innerHeight);
                        }").GetAwaiter().GetResult();
}
//===================================================================================================================
bool IsTabLoading(int tabID)
{
    int n = _tabs.FindIndex(t => t.Id == tabID);

    if (n > -1) return _tabs[n].IsLoading; // if found
    return false;
}
//===================================================================================================================
// returns a list of pending requests on a particular tab
List<IRequest> GetPendingRequests(int tabID)
{
    int n = _tabs.FindIndex(t => t.Id == tabID);

    if (n > -1) return _tabs[n].PendingRequest.ToList<IRequest>(); // if found
    return null;
}
//===================================================================================================================
// returns a list of all requests on a particular tab
List<IRequest> GetAllRequests(int tabID)
{
    int n = _tabs.FindIndex(t => t.Id == tabID);

    if (n > -1) return _tabs[n].AllRequest.ToList<IRequest>(); // if found
    return null;
}
//===================================================================================================================
}
    
