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
    
    // randomly navigate to most visited sites in the U.S.
    public IPage[] GotoRandomSites(string[] urls)
    {
        List<IPage> successfulPages = new List<IPage>();
        Random random = new Random();

        for (int i = 0; i < urls.Length; i++)
        {
            IPage page = null;
            try
            {
                page = _context.Context.NewPageAsync().GetAwaiter().GetResult();
                IResponse response = page.GotoAsync(urls[i]).GetAwaiter().GetResult();

                // Only add the page if navigation was successful (response is not null and Ok)
                if (response != null && response.Ok)
                {
                    successfulPages.Add(page);
                }
                else
                {
                    page.CloseAsync().GetAwaiter().GetResult();
                }
            }
            catch
            {
                if (page != null)
                {
                    page.CloseAsync().GetAwaiter().GetResult();
                }
                // Optionally log the failure here
            }

            // Add a random delay between 1.5 to 4 seconds (except after the last URL)
            if (i < urls.Length - 1)
            {
                int delayMs = random.Next(1500, 4000);
                Thread.Sleep(delayMs);
            }
        }

        return successfulPages.ToArray();
    }
    
    // simulate human-like mouse actions on the page
    public void PerformRandomMouseActions(IPage[] pages)
    {
        Random random = new Random();
        
        // Delay before starting actions (e.g., 3 to 5 seconds)
        Thread.Sleep(random.Next(3000, 5000));

        // Copy pages to a list for shuffling
        List<IPage> shuffledPages = new List<IPage>(pages);

        // Fisher-Yates shuffle
        for (int i = shuffledPages.Count - 1; i > 0; i--)
        {
            int j = random.Next(0, i + 1);
            IPage temp = shuffledPages[i];
            shuffledPages[i] = shuffledPages[j];
            shuffledPages[j] = temp;
        }

        for (int i = 0; i < shuffledPages.Count; i++)
        {
            IPage page = shuffledPages[i];

            // Switch to this tab (bring to front)
            page.BringToFrontAsync().GetAwaiter().GetResult();

            // Assume viewport size is always available
            int width = page.ViewportSize.Width;
            int height = page.ViewportSize.Height;

            // Move mouse to a random position
            int x = random.Next(0, width);
            int y = random.Next(0, height);
            page.Mouse.MoveAsync(x, y, new MouseMoveOptions { Steps = random.Next(15, 35) }).GetAwaiter().GetResult();

            // Random delay
            Thread.Sleep(random.Next(800, 2000));

            // Randomly decide to click (50% chance)
            double clickChance = random.NextDouble();
            if (clickChance > 0.5)
            {
                page.Mouse.ClickAsync(x, y).GetAwaiter().GetResult();
                Thread.Sleep(random.Next(500, 1500));
            }

            // Perform random scroll
            int scrollAmount = random.Next(100, 800);
            double directionChance = random.NextDouble();
            if (directionChance <= 0.5)
            {
                scrollAmount = -scrollAmount;
            }

            // Get current scroll position (evaluate JS)
            int currentScrollY = page.EvaluateAsync<int>("() => window.scrollY").GetAwaiter().GetResult();

            // Calculate new scroll position, clamp to >= 0
            int newScrollY = currentScrollY + scrollAmount;
            if (newScrollY < 0)
            {
                newScrollY = 0;
            }

            // Smooth scroll simulation: scroll in small steps
            int steps = random.Next(10, 30);
            int stepSize = 0;
            if (steps != 0)
            {
                stepSize = (newScrollY - currentScrollY) / steps;
            }

            for (int j = 1; j <= steps; j++)
            {
                int scrollY = currentScrollY + stepSize * j;
                page.EvaluateAsync($"window.scrollTo(0, {scrollY})").GetAwaiter().GetResult();
                Thread.Sleep(random.Next(30, 80)); // small delay between scroll steps
            }

            // Random delay after scrolling
            Thread.Sleep(random.Next(2000, 5000));

            // Move mouse to another random spot after scrolling
            int x2 = random.Next(0, width);
            int y2 = random.Next(0, height);
            page.Mouse.MoveAsync(x2, y2, new MouseMoveOptions { Steps = random.Next(10, 25) }).GetAwaiter().GetResult();

            Thread.Sleep(random.Next(800, 1800));
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
