using System;
using System.Threading.Tasks;
using LogUtility;
using Microsoft.Playwright;
using PageRequestInterface;
using PlaywrightController;
using user_profile_management;
using utility;
using Utility;

namespace ControlBrowser;

public class ControlBrowserPlaywright
{
    public PlaywrightTabManager TabManager;
    public IPage Tab;
    public IPage TabFull;
    public IPage DummyTab;
    
    public bool StopScraping = false;
    private static double PrevDelay = 0;
    private static readonly Random _random = new Random();
    public ScrapingResultStat ResultStat = new();

    public ControlBrowserNavigationHistory ControlBrowserNavigationHistory = new();
    public string DummyUrl = string.Empty;
    public IPage[] RandomVisitedPages = Array.Empty<IPage>();
    public string MostVisitedSitesPath = string.Empty;

    public ControlBrowserPlaywright(PlaywrightTabManager tabManager)
    {
        TabManager = tabManager;
        Tab = TabManager.OpenNewTab();
    }
    
    // Define a simple Point struct
    public struct Point
    {
        public float X { get; set; }
        public float Y { get; set; }
    }
    
    // Helper method to get current mouse position using JS
    private Point GetCurrentMousePosition(IPage page)
    {
        string script = @"() => {
        return {
            x: window.mouseX || 0,
            y: window.mouseY || 0
        };
    }";

        //Inject script to update mouse coordinates
        page.EvaluateAsync(@"() => {
        document.addEventListener('mousemove', (event) => {
            window.mouseX = event.clientX;
            window.mouseY = event.clientY;
        });
    }").GetAwaiter().GetResult();
    
        // Evaluate the JS function to get the current mouse coordinates
        var result = page.EvaluateAsync<Dictionary<string, object>>(script).GetAwaiter().GetResult();
        float x = Convert.ToSingle(result["x"]);
        float y = Convert.ToSingle(result["y"]);

        return new Point { X = x, Y = y };
    }

    public void MoveMouseRandomly(IPage page)
    {
        int movements = _random.Next(1, 4); // randomly set 1 to 3 mouse movements
        int viewportWidth = 1280;
        int viewportHeight = 720;

        PageViewportSizeResult size = page.ViewportSize;
        if (size != null)
        {
            viewportWidth = size.Width;
            viewportHeight = size.Height;
        }

        float startX = _random.Next(0, viewportWidth);
        float startY = _random.Next(0, viewportHeight);

        page.Mouse.MoveAsync(startX, startY).GetAwaiter().GetResult();
        Task.Delay(_random.Next(100, 400)).GetAwaiter().GetResult();

        for (int i = 0; i < movements; i++)
        {
            float targetX = _random.Next(0, viewportWidth);
            float targetY = _random.Next(0, viewportHeight);

            int steps = _random.Next(30, 60);
            
            for (int step = 1; step <= steps; step++)
            {
                float progress = (float)step / steps;

                // Ease in-out
                float easedProgress = (float)(0.5 * (1 - Math.Cos(progress * Math.PI)));

                float x = startX + (targetX - startX) * easedProgress + _random.Next(-1, 2); // jitter
                float y = startY + (targetY - startY) * easedProgress + _random.Next(-1, 2);

                page.Mouse.MoveAsync(x, y).GetAwaiter().GetResult();
                Task.Delay(_random.Next(100, 300)).GetAwaiter().GetResult();
            }

            startX = targetX;
            startY = targetY;

            Task.Delay(_random.Next(300, 900)).GetAwaiter().GetResult();
        }
    }

    public void OpenPinterestDummyTab()
    {
        if (_random.Next(0, 10) % 3 != 0) // give 33% chance to open random scraped pin in a new tab
        {
            return;
        }
        
        if (ExtractorHelper.IsValidUrl(DummyUrl))
        {
            DummyTab = TabManager.OpenNewTab();
            GotoPage(DummyUrl,0,DummyTab);
            TabManager.SwitchActiveTab(DummyTab);
            SetRandomDelay(2, 5).GetAwaiter().GetResult();

            for (int i = 0; i < _random.Next(3,5); i++) // number of actions
            {
                ClosePinterestLogInDialogIfFound(DummyTab);
                int actionIndex = _random.Next(1,4);

                switch (actionIndex)
                {
                    case 1:
                        ScrollDownRandomly(DummyTab);
                        break;
                    case 2:
                        MoveMouseRandomly(DummyTab);
                        break;
                    case 3:
                        ScrollUpRandomly(DummyTab);
                        break;
                }
                
                SetRandomDelay(1, 3).GetAwaiter().GetResult();
            }
            
            TabManager.CloseTab(DummyTab);
            TabManager.SwitchActiveTab(Tab);
            SetRandomDelay(3, 6).GetAwaiter().GetResult();
        }
    }
    
    public void OpenYoutubeDummyTab()
    {
        if (_random.Next(0, 10) % 2 == 0) // give 33% chance to open random scraped pin in a new tab
        {
            return;
        }
        
        if (ExtractorHelper.IsValidUrl(DummyUrl))
        {
            DummyTab = TabManager.OpenNewTab();
            GotoPage(DummyUrl,0,DummyTab);
            TabManager.SwitchActiveTab(DummyTab);
            SetRandomDelay(2, 5).GetAwaiter().GetResult();

            for (int i = 0; i < _random.Next(2,4); i++) // number of actions
            {
                ClickYoutubeSkipAdButtonIfFound(DummyTab);
                CloseYoutubeLogInDialogIfFound(DummyTab);
                ClickYoutubeDismissButtonIfFound(DummyTab);
                MoveMouseRandomly(DummyTab);
                SetRandomDelay(1, 3).GetAwaiter().GetResult();
            }
            
            TabManager.CloseTab(DummyTab);
            TabManager.SwitchActiveTab(Tab);
            SetRandomDelay(3, 6).GetAwaiter().GetResult();
        }
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
            
            SetRandomDelay(2, 5).GetAwaiter().GetResult();
            DoInitialScroll(tab);
        }
        catch (Exception ex)
        {
            LibLog.LogError($"Failed to navigate {url}: {ex.Message}");
            return false;
        }
        
        ControlBrowserNavigationHistory.AddUrl(url);
        return true;
    }

    // navigate to a number of random sites
    public void GotoRandomSites()
    {
        Random random = new Random();
        
        SetRandomDelay(3, 5).GetAwaiter().GetResult();
        int numberOfSites = random.Next(1, 4); // open 1 to 3 random sites
        RandomVisitedPages = TabManager.GotoRandomSites(MostVisitedSitesLoader.SelectRandomSitesFromFile(MostVisitedSitesPath, numberOfSites));
        SetRandomDelay(3, 5).GetAwaiter().GetResult();
    }

    public void DoInitialScroll(IPage tabPage)
    {
        Random random = new Random();
        tabPage.Mouse.WheelAsync(0, random.Next(100,200)).GetAwaiter().GetResult();
    }

    public void ClosePinterestLogInDialogIfFound(IPage tabPage)
    {
        try
        {
            // Use a short timeout to avoid long waits if the element is not present
            ILocator signupCloseButton = tabPage.Locator("[data-test-id='full-page-signup-close-button'] button");
        
            if (signupCloseButton.IsVisibleAsync(new()).GetAwaiter().GetResult())
            {
                SetRandomDelay(1, 3).GetAwaiter().GetResult();
                signupCloseButton.ClickAsync().GetAwaiter().GetResult();
            }
        }
        catch (Exception)
        {
        }

        try
        {
            ILocator closeButton = tabPage.Locator("button[aria-label='Close Bottom Right Upsell']");
            if (closeButton.IsVisibleAsync(new() { Timeout = 1000 }).GetAwaiter().GetResult())
            {
                SetRandomDelay(1, 3).GetAwaiter().GetResult();
                closeButton.ClickAsync().GetAwaiter().GetResult();
            }
        }
        catch (Exception)
        {
        }
        
        // if language is in spanish
        try
        {
            ILocator closeButton = tabPage.Locator("button[aria-label='Cerrar abajo a la derecha Upsell']");
            if (closeButton.IsVisibleAsync(new() { Timeout = 1000 }).GetAwaiter().GetResult())
            {
                SetRandomDelay(1, 3).GetAwaiter().GetResult();
                closeButton.ClickAsync().GetAwaiter().GetResult();
            }
        }
        catch (Exception)
        {
        }
    }
    
    public void CloseYoutubeLogInDialogIfFound(IPage tabPage)
    {
        try
        {
            // Explicitly typed locator for the "No thanks" button
            ILocator noThanksButton = tabPage.GetByRole(AriaRole.Button, new() { Name = "No thanks" });
        
            if (noThanksButton.IsVisibleAsync(new()).GetAwaiter().GetResult())
            {
                SetRandomDelay(1, 3).GetAwaiter().GetResult();
                noThanksButton.ClickAsync().GetAwaiter().GetResult();
            }
        }
        catch (Exception)
        {
        }
        
        try
        {
            // Explicitly typed locator for the "No thanks" button
            ILocator noThanksButton = tabPage.GetByRole(AriaRole.Button, new() { Name = "No, gracias" });
        
            if (noThanksButton.IsVisibleAsync(new()).GetAwaiter().GetResult())
            {
                SetRandomDelay(1, 3).GetAwaiter().GetResult();
                noThanksButton.ClickAsync().GetAwaiter().GetResult();
            }
        }
        catch (Exception)
        {
        }
    }
    
    public void ClickYoutubeSkipAdButtonIfFound(IPage tabPage)
    {
        // YouTube's "Skip Ad" button usually has the class 'ytp-ad-skip-button' or 'ytp-ad-skip-button-container'
        try
        {
            ILocator skipAdButton = tabPage.Locator(".ytp-ad-skip-button, .ytp-ad-skip-button-container");
            
            if (skipAdButton.IsVisibleAsync(new()).GetAwaiter().GetResult())
            {
                SetRandomDelay(1, 3).GetAwaiter().GetResult();
                skipAdButton.ClickAsync().GetAwaiter().GetResult();
                SetRandomDelay(1, 3).GetAwaiter().GetResult();
            }
        }
        catch (Exception)
        {
        }
    }
    
    public void ClickYoutubeDismissButtonIfFound(IPage tabPage)
    {
        try
        {
            // YouTube's "Dismiss" button often uses the text 'Dismiss' as its accessible name
            ILocator dismissButton = tabPage.GetByRole(AriaRole.Button, new() { Name = "Dismiss" });

            if (dismissButton.IsVisibleAsync(new()).GetAwaiter().GetResult())
            {
                SetRandomDelay(1, 3).GetAwaiter().GetResult();
                dismissButton.ClickAsync().GetAwaiter().GetResult();
                SetRandomDelay(1, 3).GetAwaiter().GetResult();
            }
        }
        catch (Exception)
        {
            // Optionally log or ignore exceptions
        }
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
        float vertScrollValue = random.Next(1, 3 + 1) * random.Next(55,97);
        float steps = 0;
        float step;

        while (steps < vertScrollValue)
        {
            step = random.Next(1, 5 + 1) * random.Next(1,2);
            Tab.Mouse.WheelAsync(0, step).GetAwaiter().GetResult();

            if (ReachedBottom(Tab))
            {
                break;
            }

            steps += step;
        }

        return true;
    }
    
    public bool ScrollDownRandomly(IPage tabPage)
    {
        Random random = new();
        float vertScrollValue = random.Next(1, 3 + 1) * random.Next(55,97);
        float steps = 0;
        float step;

        while (steps < vertScrollValue)
        {
            step = random.Next(1, 5 + 1) * random.Next(1,2);
            tabPage.Mouse.WheelAsync(0, step).GetAwaiter().GetResult();

            if (ReachedBottom(tabPage))
            {
                break;
            }

            steps += step;
        }

        return true;
    }
    
    public bool ScrollUpRandomly(IPage tabPage)
    {
        Random random = new();
        float vertScrollValue = random.Next(1, 3 + 1) * random.Next(30,45);
        float steps = 0;
        float step;

        while (steps < vertScrollValue)
        {
            step = random.Next(1, 5 + 1) * random.Next(1,2);
            tabPage.Mouse.WheelAsync(0, -step).GetAwaiter().GetResult();

            if (ReachedTop(tabPage))
            {
                break;
            }

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
    
    public bool ReachedTop(IPage page)
    {
        // Check if the viewport has reached the top of the page
        return page.EvaluateAsync<bool>(@"
            () => {
                const scrollPosition = window.scrollY - window.innerHeight;
                const pageHeight = document.documentElement.scrollHeight;
                return scrollPosition < pageHeight;
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