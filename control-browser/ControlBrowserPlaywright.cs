using System;
using System.Threading.Tasks;
using LogUtility;
using Microsoft.Playwright;
using PageRequestInterface;
using PlaywrightController;
using utility;

namespace ControlBrowser;

public class ControlBrowserPlaywright
{
    public PlaywrightTabManager TabManager;
    public IPage Tab;
    public IPage TabFull;
    
    public bool StopScraping = false;
    private static double PrevDelay = 0;
    public ScrapingResultStat ResultStat = new();

    public ControlBrowserNavigationHistory ControlBrowserNavigationHistory = new();

    public ControlBrowserPlaywright(PlaywrightTabManager tabManager)
    {
        TabManager = tabManager;
        Tab = TabManager.OpenNewTab();
    }
    
    public bool GotoPage(string url, int timeoutMs = 60000, IPage tabPage = null,  PageGotoOptions options = null)
    {
        IPage tab = tabPage;
        
        if (tabPage == null)
        {
            tab = Tab;
        }
        
        TabManager.AssertActive(tab);
    
        IResponse response;
    
        try
        {
            if (options == null)
            {
                response = tab.GotoAsync(url, new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded, Timeout = timeoutMs }).GetAwaiter().GetResult();
            }
            else
            {
                response = tab.GotoAsync(url, options).GetAwaiter().GetResult();
            }
            
            if (response == null)
            {
                LibLog.LogError($"No response received when attempting to navigate {url}");
                return false;
            }
    
            // Check for non-successful HTTP status code
            if (response.Status >= 400)
            {
                LibLog.LogError($"Navigating to {url} returned bad status code: {response.Status} {response.StatusText}");
                return false;
            }
        }
        catch (Exception ex)
        {
            LibLog.LogError($"Failed to navigate {url}: {ex.Message}");
            return false;
        }
        
        ControlBrowserNavigationHistory.AddUrl(url);
        return true;
    }

    public bool GoBack()
    {
        if (!ControlBrowserNavigationHistory.CanGoBack())
        {
            return false;
        }
        
        return GotoPage(ControlBrowserNavigationHistory.GoBack());
    }

    public bool GoForward()
    {
        if (!ControlBrowserNavigationHistory.CanGoForward())
        {
            return false;
        }
        
        return GotoPage(ControlBrowserNavigationHistory.GoForward());
    }

    public bool Refresh(PageReloadOptions options = null)
    {
        TabManager.AssertActive(Tab);

        Task<IResponse> reloadTask;
        if (options == null)
        {
            reloadTask = Tab.ReloadAsync();
        }
        else
        {
            reloadTask = Tab.ReloadAsync(options);
        }

        bool isSuccessful = reloadTask.Wait(60000);
        if (!isSuccessful)
        {
            return false;
        }

        IResponse response = reloadTask.Result;
        if (response == null || response.Status < 200 || response.Status >= 300)
        {
            return false;
        }

        return true;
    }

    public string GetCurrentUrl()
    {
        return ControlBrowserNavigationHistory.CurrentUrl();
    }

    public bool ScrollDownPage()
    {
        Random random = new();
        float vertScrollValue = random.Next(1, 5 + 1) * 100;
        float steps = 0;
        float step;

        while (steps < vertScrollValue)
        {
            step = random.Next(1, 5 + 1) * 2;
            Tab.Mouse.WheelAsync(0, step).GetAwaiter().GetResult();

            if (ReachedBottom(Tab)) break;

            steps += step;
        }

        return true;
    }
    
    public string GetPageDOM()
    {
        TabManager.AssertActive(Tab);
        string dom = Tab.ContentAsync().GetAwaiter().GetResult();
        return dom;
    }

    public bool ReachedBottom(IPage page)
    {
        // Check if the viewport has reached the bottom of the page
        return page.EvaluateAsync<bool>(@"
            () => {
                const scrollPosition = window.scrollY + window.innerHeight;
                const pageHeight = document.documentElement.scrollHeight;
                return scrollPosition >= pageHeight;
            }
            ").GetAwaiter().GetResult();
    }
    
    // randomizes delay specifying a range in seconds to simulate human
    public async Task SetRandomDelay(int minSeconds, int maxSeconds)
    {
        int n = 0;
        Random random = new();

        do
        {
            n = random.Next(minSeconds, maxSeconds + 1);
        } while (n == PrevDelay);

        PrevDelay = n;
        await Task.Delay(n * 1000); // Upper bound is exclusive, so add 1
    }
}