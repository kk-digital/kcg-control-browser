using LogUtility;
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
    
    public async Task<IPage> OpenNewTabAsync()
    {
        IPage page = await _context.Context.NewPageAsync();
        _tabs.Add(page);

        AssertActiveTabIsTracked(page);
        return page;
    }
    
    // randomly navigate to most visited sites in the U.S.
    public IPage[] GotoRandomSites(string[] urls)
    {
        List<IPage> successfulPages = new List<IPage>();
        List<IPage> allPages = new List<IPage>();
        List<Task<IResponse>> navigationTasks = new List<Task<IResponse>>();
        Random random = new Random();

        PageGotoOptions pageGotoOptions = new PageGotoOptions
        {
            WaitUntil = WaitUntilState.Load,
            Timeout = 15000 // 15 seconds timeout
        };

        for (int i = 0; i < urls.Length; i++)
        {
            IPage page = null;
            try
            {
                page = _context.Context.NewPageAsync().GetAwaiter().GetResult();
                Task<IResponse> navigationTask = page.GotoAsync(urls[i], pageGotoOptions);
                allPages.Add(page);
                navigationTasks.Add(navigationTask);
            }
            catch
            {
                if (page != null)
                {
                    page.CloseAsync().GetAwaiter().GetResult();
                }
            }
            int delayMs = random.Next(3000, 5000);
            Thread.Sleep(delayMs);
        }
        
        Thread.Sleep(10000); // 10 seconds delay

        // Await all navigations and check results
        for (int i = 0; i < navigationTasks.Count; i++)
        {
            try
            {
                IResponse response = navigationTasks[i].GetAwaiter().GetResult();
                if (response != null && response.Ok)
                {
                    successfulPages.Add(allPages[i]);
                }
                else
                {
                    allPages[i].CloseAsync().GetAwaiter().GetResult();
                }
            }
            catch
            {
                allPages[i].CloseAsync().GetAwaiter().GetResult();
            }
        }

        return successfulPages.ToArray();
    }
    
    public async Task<IPage[]> GotoRandomSitesAsync(string[] urls)
    {
        List<IPage> successfulPages = new List<IPage>();
        List<IPage> allPages = new List<IPage>();
        List<Task<IResponse>> navigationTasks = new List<Task<IResponse>>();
        Random random = new Random();

        PageGotoOptions pageGotoOptions = new PageGotoOptions
        {
            WaitUntil = WaitUntilState.Load,
            Timeout = 15000 // 15 seconds timeout
        };

        for (int i = 0; i < urls.Length; i++)
        {
            IPage page = null;
            try
            {
                page = await _context.Context.NewPageAsync();
                Task<IResponse> navigationTask = page.GotoAsync(urls[i], pageGotoOptions);
                allPages.Add(page);
                navigationTasks.Add(navigationTask);
            }
            catch
            {
                if (page != null)
                {
                    await page.CloseAsync();
                }
            }
            int delayMs = random.Next(3000, 5000);
            await Task.Delay(delayMs);
        }
        
        await Task.Delay(10000); // 10 seconds delay

        // Await all navigations and check results
        for (int i = 0; i < navigationTasks.Count; i++)
        {
            try
            {
                IResponse response = await navigationTasks[i];
                if (response != null && response.Ok)
                {
                    successfulPages.Add(allPages[i]);
                }
                else
                {
                    await allPages[i].CloseAsync();
                }
            }
            catch
            {
                await allPages[i].CloseAsync();
            }
        }

        return successfulPages.ToArray();
    }
    
    // simulate human-like mouse actions on the page
    public void PerformRandomMouseActions(IPage[] pages)
    {
        Random random = new Random();

        // Optional: wait for all pages to be stable before starting
        for (int i = 0; i < pages.Length; i++)
        {
            IPage page = pages[i];
            try
            {
                page.WaitForLoadStateAsync(LoadState.NetworkIdle, new() { Timeout = 10000 }).GetAwaiter().GetResult();
            }
            catch
            {
                // Ignore timeout or errors here, proceed anyway
            }
        }

        // Shuffle pages using Fisher-Yates
        List<IPage> shuffledPages = new List<IPage>(pages);
        for (int i = shuffledPages.Count - 1; i > 0; i--)
        {
            int j = random.Next(0, i + 1);
            IPage temp = shuffledPages[i];
            shuffledPages[i] = shuffledPages[j];
            shuffledPages[j] = temp;
        }

        // Delay before starting actions (3 to 5 seconds)
        Thread.Sleep(random.Next(3000, 5000));

        for (int i = 0; i < shuffledPages.Count; i++)
        {
            IPage page = shuffledPages[i];

            try
            {
                page.BringToFrontAsync().GetAwaiter().GetResult();

                int width;
                int height;

                if (page.ViewportSize != null)
                {
                    width = page.ViewportSize.Width;
                    height = page.ViewportSize.Height;
                }
                else
                {
                    width = 1280;
                    height = 720;
                }

                // Move mouse to a random position
                int x = random.Next(0, width);
                int y = random.Next(0, height);
                page.Mouse.MoveAsync(x, y, new MouseMoveOptions { Steps = random.Next(15, 35) }).GetAwaiter().GetResult();

                Thread.Sleep(random.Next(800, 2000));

                // 50% chance to click
                if (random.NextDouble() > 0.5)
                {
                    try
                    {
                        // Wait for navigation if it happens, otherwise just click
                        Task navigationTask = page.WaitForNavigationAsync(new() { Timeout = 3000 });
                        page.Mouse.ClickAsync(x, y).GetAwaiter().GetResult();
                        try
                        {
                            navigationTask.GetAwaiter().GetResult();
                        }
                        catch
                        {
                            // Ignore if navigation does not happen
                        }
                    }
                    catch
                    {
                        // Ignore click/navigation errors
                    }

                    Thread.Sleep(random.Next(3000, 5000));
                }

                // Perform random scroll
                int scrollAmount = random.Next(100, 800);
                if (random.NextDouble() <= 0.5)
                {
                    scrollAmount = -scrollAmount;
                }

                int currentScrollY = 0;
                try
                {
                    currentScrollY = page.EvaluateAsync<int>("() => window.scrollY").GetAwaiter().GetResult();
                }
                catch
                {
                    // Ignore evaluation errors
                }

                int newScrollY = currentScrollY + scrollAmount;
                if (newScrollY < 0)
                {
                    newScrollY = 0;
                }

                int steps = random.Next(10, 30);
                int stepSize;
                if (steps != 0)
                {
                    stepSize = (newScrollY - currentScrollY) / steps;
                }
                else
                {
                    stepSize = 0;
                }

                for (int j = 1; j <= steps; j++)
                {
                    int scrollY = currentScrollY + stepSize * j;
                    try
                    {
                        page.EvaluateAsync("window.scrollTo(0, " + scrollY + ")").GetAwaiter().GetResult();
                    }
                    catch
                    {
                        // Ignore scroll errors
                    }
                    Thread.Sleep(random.Next(30, 80));
                }

                Thread.Sleep(random.Next(2000, 5000));

                // Move mouse to another random spot after scrolling
                int x2 = random.Next(0, width);
                int y2 = random.Next(0, height);
                page.Mouse.MoveAsync(x2, y2, new MouseMoveOptions { Steps = random.Next(10, 25) }).GetAwaiter().GetResult();

                Thread.Sleep(random.Next(800, 1800));
            }
            catch (PlaywrightException ex)
            {
                LibLog.LogError("Playwright error on page: " + ex.Message);
            }
            catch (System.Exception ex)
            {
                LibLog.LogError("Unexpected error on page: " + ex.Message);
            }
        }
    }
    
    // needs to be asynchronous for gui-based app
    public async Task PerformRandomMouseActionsAsync(IPage[] pages)
    {
        Random random = new Random();

        // Optional: wait for all pages to be stable before starting
        for (int i = 0; i < pages.Length; i++)
        {
            IPage page = pages[i];
            try
            {
                await page.WaitForLoadStateAsync(LoadState.NetworkIdle, new() { Timeout = 10000 });
            }
            catch
            {
                // Ignore timeout or errors here, proceed anyway
            }
        }

        // Shuffle pages using Fisher-Yates
        List<IPage> shuffledPages = new List<IPage>(pages);
        for (int i = shuffledPages.Count - 1; i > 0; i--)
        {
            int j = random.Next(0, i + 1);
            IPage temp = shuffledPages[i];
            shuffledPages[i] = shuffledPages[j];
            shuffledPages[j] = temp;
        }

        // Delay before starting actions (3 to 5 seconds)
        await Task.Delay(random.Next(3000, 5000));

        for (int i = 0; i < shuffledPages.Count; i++)
        {
            IPage page = shuffledPages[i];

            try
            {
                await page.BringToFrontAsync();

                int width;
                int height;

                if (page.ViewportSize != null)
                {
                    width = page.ViewportSize.Width;
                    height = page.ViewportSize.Height;
                }
                else
                {
                    width = 1280;
                    height = 720;
                }

                // Move mouse to a random position
                int x = random.Next(0, width);
                int y = random.Next(0, height);
                await page.Mouse.MoveAsync(x, y, new MouseMoveOptions { Steps = random.Next(15, 35) });

                await Task.Delay(random.Next(800, 2000));

                // 50% chance to click
                if (random.NextDouble() > 0.5)
                {
                    try
                    {
                        // Wait for navigation if it happens, otherwise just click
                        Task navigationTask = page.WaitForNavigationAsync(new() { Timeout = 3000 });
                        await page.Mouse.ClickAsync(x, y);
                        try
                        {
                            await navigationTask;
                        }
                        catch
                        {
                            // Ignore if navigation does not happen
                        }
                    }
                    catch
                    {
                        // Ignore click/navigation errors
                    }

                    await Task.Delay(random.Next(3000, 5000));
                }

                // Perform random scroll
                int scrollAmount = random.Next(100, 800);
                if (random.NextDouble() <= 0.5)
                {
                    scrollAmount = -scrollAmount;
                }

                int currentScrollY = 0;
                try
                {
                    currentScrollY = await page.EvaluateAsync<int>("() => window.scrollY");
                }
                catch
                {
                    // Ignore evaluation errors
                }

                int newScrollY = currentScrollY + scrollAmount;
                if (newScrollY < 0)
                {
                    newScrollY = 0;
                }

                int steps = random.Next(10, 30);
                int stepSize;
                if (steps != 0)
                {
                    stepSize = (newScrollY - currentScrollY) / steps;
                }
                else
                {
                    stepSize = 0;
                }

                for (int j = 1; j <= steps; j++)
                {
                    int scrollY = currentScrollY + stepSize * j;
                    try
                    {
                        await page.EvaluateAsync("window.scrollTo(0, " + scrollY + ")");
                    }
                    catch
                    {
                        // Ignore scroll errors
                    }
                    await Task.Delay(random.Next(30, 80));
                }

                await Task.Delay(random.Next(2000, 5000));

                // Move mouse to another random spot after scrolling
                int x2 = random.Next(0, width);
                int y2 = random.Next(0, height);
                await page.Mouse.MoveAsync(x2, y2, new MouseMoveOptions { Steps = random.Next(10, 25) });
                await Task.Delay(random.Next(800, 1800));
            }
            catch (PlaywrightException ex)
            {
                LibLog.LogError("Playwright error on page: " + ex.Message);
            }
            catch (System.Exception ex)
            {
                LibLog.LogError("Unexpected error on page: " + ex.Message);
            }
        }
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
    
    public async Task SwitchActiveTabAsync(IPage page)
    {
        await page.BringToFrontAsync();
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
    
    public async Task CloseTabAsync(IPage page)
    {
        if (ActiveTab == page)
        {
            ActiveTab = null;
            ActiveTabIndex = -1;
        }

        await page.CloseAsync();
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
