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
    
    // use for pinterest wandering mode in CdxLocalClient app
    public IPage RandomBoardPage = null;    // use for opening random board page from the board search results page
    public List<IPage> RandomPinPages = new List<IPage>(); // use for opening random pin pages from currently-opened board page
    public HashSet<string> PinterestVisitedUrls = new HashSet<string>();

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

        // Inject script to update mouse coordinates
        page.EvaluateAsync(@"() => {
        document.addEventListener('mousemove', (event) => {
            window.mouseX = event.clientX;
            window.mouseY = event.clientY;
        });
    }").GetAwaiter().GetResult();

        Dictionary<string, object> result = page.EvaluateAsync<Dictionary<string, object>>(script).GetAwaiter().GetResult();

        float x = Convert.ToSingle(result["x"]);
        float y = Convert.ToSingle(result["y"]);

        return new Point { X = x, Y = y };
    }
    
    // needs to be asynchronous for gui-based app
    private async Task<Point> GetCurrentMousePositionAsync(IPage page)
    {
        string script = @"() => {
        return {
            x: window.mouseX || 0,
            y: window.mouseY || 0
        };
    }";

        // Inject script to update mouse coordinates
        await page.EvaluateAsync(@"() => {
        document.addEventListener('mousemove', (event) => {
            window.mouseX = event.clientX;
            window.mouseY = event.clientY;
        });
    }");

        Dictionary<string, object> result = await page.EvaluateAsync<Dictionary<string, object>>(script);

        float x = Convert.ToSingle(result["x"]);
        float y = Convert.ToSingle(result["y"]);

        return new Point { X = x, Y = y };
    }

    public void MoveMouseRandomly(IPage page)
    {
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
    
    // needs to be asynchronous for gui-based app
    public async Task MoveMouseRandomlyAsync(IPage page)
    {
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

        await page.Mouse.MoveAsync(startX, startY);
        await Task.Delay(_random.Next(100, 400));

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

            await page.Mouse.MoveAsync(x, y);
            await Task.Delay(_random.Next(100, 300));
        }

        startX = targetX;
        startY = targetY;

        await Task.Delay(_random.Next(300, 900));
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
    
    // needs to be asynchronous for gui-based app
    public async Task OpenPinterestDummyTabAsync(IPage page)
    {
        if (ExtractorHelper.IsValidUrl(DummyUrl))
        {
            DummyTab = await TabManager.OpenNewTabAsync();
            await GotoPageAsync(DummyUrl,0,DummyTab);
            await TabManager.SwitchActiveTabAsync(DummyTab);
            await SetRandomDelay(2, 5);

            for (int i = 0; i < _random.Next(3,5); i++) // number of actions
            {
                await ClosePinterestLogInDialogIfFoundAsync(DummyTab);
                int actionIndex = _random.Next(1,4);

                switch (actionIndex)
                {
                    case 1:
                        await ScrollDownRandomlyAsync(DummyTab);
                        break;
                    case 2:
                        await MoveMouseRandomlyAsync(DummyTab);
                        break;
                    case 3:
                        await ScrollUpRandomlyAsync(DummyTab);
                        break;
                }
                
                await SetRandomDelay(1, 3);
            }
            
            await TabManager.CloseTabAsync(DummyTab);
            await TabManager.SwitchActiveTabAsync(page);
            await SetRandomDelay(3, 6);
        }
    }
    
    public async Task OpenPinterestPersistentPinTabAsync(IPage parentPage, IPage childPage)
    {
        if (ExtractorHelper.IsValidUrl(DummyUrl))
        {
            childPage = await TabManager.OpenNewTabAsync();
            await TabManager.SwitchActiveTabAsync(childPage);
            await SetRandomDelay(1, 4);
            await GotoPageAsync(DummyUrl,0,childPage);
            RandomPinPages.Add(childPage);
            await SetRandomDelay(1, 4);
            await ClosePinterestLogInDialogIfFoundAsync(childPage);
            await SetRandomDelay(1, 4);
            await ScrollDownPageAsync(childPage);
            
            for (int i = 0; i < _random.Next(3,5); i++) // number of actions
            {
                int actionIndex = _random.Next(1,4);

                switch (actionIndex)
                {
                    case 1:
                        await ScrollDownRandomlyAsync(childPage);
                        break;
                    case 2:
                        await MoveMouseRandomlyAsync(childPage);
                        break;
                    case 3:
                        await ScrollUpRandomlyAsync(childPage);
                        break;
                }
                
                await SetRandomDelay(1, 4);
            }

            if (_random.Next(0, 10) % 2 == 0)
            {
                await TabManager.CloseTabAsync(childPage);
                RandomPinPages.RemoveAt(RandomPinPages.Count - 1);
                await SetRandomDelay(1, 4);
            }
            
            await TabManager.SwitchActiveTabAsync(parentPage);
            await SetRandomDelay(1, 4);
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
    
    public bool GotoPage(string url, int timeoutMs = 60000, IPage tabPage = null)
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
            response = tab.GotoAsync(url, new PageGotoOptions { WaitUntil = WaitUntilState.Load, Timeout = 0 }).GetAwaiter().GetResult();
            
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
    
    // needs to be asynchronous for gui-based app
    public async Task<bool> GotoPageAsync(string url, int timeoutMs = 60000, IPage tabPage = null)
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
            response = await tab.GotoAsync(url, new PageGotoOptions { WaitUntil = WaitUntilState.Load, Timeout = 0 });
            
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
            
            await SetRandomDelay(2, 5);
            await DoInitialScrollAsync(tab);
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
        bool anyCommon = true;
        IPage[] RandomVisitedPages2;

        do
        {
            RandomVisitedPages2 = TabManager.GotoRandomSites(MostVisitedSitesLoader.SelectRandomSitesFromFile(MostVisitedSitesPath, numberOfSites));
            
            if (RandomVisitedPages.Length == 0)
            {
                break;
            }
            
            // Find intersection elements
            IPage[] intersection = RandomVisitedPages.Intersect(RandomVisitedPages2).ToArray();
            // Check if any common elements exist
            anyCommon = intersection.Length > 0;
        } while (anyCommon);
        
        //IPage[] RandomVisitedPages2 = TabManager.GotoRandomSites(MostVisitedSitesLoader.SelectRandomSitesFromFile(MostVisitedSitesPath, numberOfSites));
        
        // Store original length of array
        int originalLength = RandomVisitedPages.Length;

        // Resize array to hold elements of both arrays
        Array.Resize(ref RandomVisitedPages, originalLength + RandomVisitedPages2.Length);

        // Copy RandomVisitedPages2 elements into the resized RandomVisitedPages starting at originalLength
        Array.Copy(RandomVisitedPages2, 0, RandomVisitedPages, originalLength, RandomVisitedPages2.Length);
        SetRandomDelay(3, 5).GetAwaiter().GetResult();
    }
    
    // needs to be asynchronous for gui-based app
    public async Task GotoRandomSitesAsync()
    {
        Random random = new Random();
        
        await SetRandomDelay(3, 5);
        int numberOfSites = random.Next(2, 4); // open random sites
        bool anyCommon = true;
        IPage[] RandomVisitedPages2;

        do
        {
            RandomVisitedPages2 = await TabManager.GotoRandomSitesAsync(MostVisitedSitesLoader.SelectRandomSitesFromFile(MostVisitedSitesPath, numberOfSites));
            
            if (RandomVisitedPages.Length == 0)
            {
                break;
            }
            
            // Find intersection elements
            IPage[] intersection = RandomVisitedPages.Intersect(RandomVisitedPages2).ToArray();
            // Check if any common elements exist
            anyCommon = intersection.Length > 0;
        } while (anyCommon);
        
        // Store original length of array
        int originalLength = RandomVisitedPages.Length;

        // Resize array to hold elements of both arrays
        Array.Resize(ref RandomVisitedPages, originalLength + RandomVisitedPages2.Length);

        // Copy RandomVisitedPages2 elements into the resized RandomVisitedPages starting at originalLength
        Array.Copy(RandomVisitedPages2, 0, RandomVisitedPages, originalLength, RandomVisitedPages2.Length);
        await SetRandomDelay(3, 5);
    }

    public void DoInitialScroll(IPage tabPage)
    {
        Random random = new Random();
        tabPage.Mouse.WheelAsync(0, random.Next(100,200)).GetAwaiter().GetResult();
    }
    
    // needs to be asynchronous for gui-based app
    public async Task DoInitialScrollAsync(IPage tabPage)
    {
        Random random = new Random();
        await tabPage.Mouse.WheelAsync(0, random.Next(100,200));
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
    }
    
    // needs to be asynchronous for gui-based app
    public async Task ClosePinterestLogInDialogIfFoundAsync(IPage tabPage)
    {
        try
        {
            // Use a short timeout to avoid long waits if the element is not present
            ILocator signupCloseButton = tabPage.Locator("[data-test-id='full-page-signup-close-button'] button");
        
            if (await signupCloseButton.IsVisibleAsync(new LocatorIsVisibleOptions()))
            {
                await SetRandomDelay(1, 3);
                await signupCloseButton.ClickAsync();
            }
        }
        catch (Exception)
        {
        }

        try
        {
            ILocator closeButton = tabPage.Locator("button[aria-label='Close Bottom Right Upsell']");
            if (await closeButton.IsVisibleAsync(new LocatorIsVisibleOptions() { Timeout = 1000 }))
            {
                await SetRandomDelay(1, 3);
                await closeButton.ClickAsync();
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
            step = random.Next(5,13);
            Tab.Mouse.WheelAsync(0, step).GetAwaiter().GetResult();

            if (ReachedBottom(Tab))
            {
                break;
            }

            steps += step;
            Thread.Sleep(random.Next(50, 150));
        }

        return true;
    }
    
    // needs to be asynchronous for gui-based app
    public async Task<bool> ScrollDownPageAsync(IPage page = null)
    {
        Random random = new();
        float vertScrollValue = random.Next(1, 3 + 1) * random.Next(55,97);
        float steps = 0;
        float step;
        IPage tab;

        if (page == null)
        {
            tab = Tab;
        }
        else
        {
            tab = page;
        }

        while (steps < vertScrollValue)
        {
            step = random.Next(5,13);
            await tab.Mouse.WheelAsync(0, step);

            if (await ReachedBottomAsync(tab))
            {
                break;
            }

            steps += step;
            await Task.Delay(random.Next(50, 150));
        }

        return true;
    }
    
    // needs to be asynchronous for gui-based app
    public async Task<bool> ScrollUpPageAsync(IPage page = null)
    {
        Random random = new();
        float vertScrollValue = random.Next(1, 3 + 1) * random.Next(55,97);
        float steps = 0;
        float step;
        IPage tab;

        if (page == null)
        {
            tab = Tab;
        }
        else
        {
            tab = page;
        }

        while (steps < vertScrollValue)
        {
            step = random.Next(5,13);
            await tab.Mouse.WheelAsync(0, -step);

            if (await ReachedBottomAsync(tab))
            {
                break;
            }

            steps += step;
            await Task.Delay(random.Next(50, 150));
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
    
    // needs to be asynchronous for gui-based app
    public async Task<bool> ScrollDownRandomlyAsync(IPage tabPage)
    {
        Random random = new();
        float vertScrollValue = random.Next(1, 3 + 1) * random.Next(55,97);
        float steps = 0;
        float step;

        while (steps < vertScrollValue)
        {
            step = random.Next(1, 5 + 1) * random.Next(1,2);
            await tabPage.Mouse.WheelAsync(0, step);

            if (await ReachedBottomAsync(tabPage))
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
    
    // needs to be asynchronous for gui-based app
    public async Task<bool> ScrollUpRandomlyAsync(IPage tabPage)
    {
        Random random = new();
        float vertScrollValue = random.Next(1, 3 + 1) * random.Next(30,45);
        float steps = 0;
        float step;

        while (steps < vertScrollValue)
        {
            step = random.Next(1, 5 + 1) * random.Next(1,2);
            await tabPage.Mouse.WheelAsync(0, -step);

            if (await ReachedTopAsync(tabPage))
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
    
    // needs to be asynchronous for gui-based app
    public async Task<string> GetPageDOMAsync(IPage page)
    {
        string dom = await page.ContentAsync();
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
    
    // needs to be asynchronous for gui-based app
    public async Task<bool> ReachedBottomAsync(IPage page)
    {
        // Check if the viewport has reached the bottom of the page
        return await page.EvaluateAsync<bool>(@"
            () => {
                const scrollPosition = window.scrollY + window.innerHeight;
                const pageHeight = document.documentElement.scrollHeight;
                return scrollPosition >= pageHeight;
            }
            ");
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
    
    // needs to be asynchronous for gui-based app
    public async Task<bool> ReachedTopAsync(IPage page)
    {
        // Check if the viewport has reached the top of the page
        return await page.EvaluateAsync<bool>(@"
            () => {
                const scrollPosition = window.scrollY - window.innerHeight;
                const pageHeight = document.documentElement.scrollHeight;
                return scrollPosition < pageHeight;
            }
            ");
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