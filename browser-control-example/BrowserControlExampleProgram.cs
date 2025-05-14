using LogUtility;
using Microsoft.Playwright;
using playwright_browser_automation; // Adjust this to your actual namespace

namespace browser_control_example
{
    class BrowserControlExampleProgram
    {
        static async Task Main(string[] args)
        {
            Console.Clear();

            // Open browser
            using IPlaywright playwright = await Playwright.CreateAsync();

            BrowserTypeLaunchOptions options = new BrowserTypeLaunchOptions();
            options.Headless = false;
            options.ExecutablePath = @"/chrome.exe"; // put here actual path to chrome.exe

            IBrowser browser = await playwright.Chromium.LaunchAsync(options);
            IBrowserContext context = await browser.NewContextAsync();
            IPage page = await context.NewPageAsync();

            // Create a request manager to track resource requests on the tab
            RequestManager requestManager = new RequestManager();
            requestManager.AttachToPage(page);

            // Go to Wikipedia main page
            await page.GotoAsync("https://en.wikipedia.org/wiki/Main_Page", new PageGotoOptions()
            {
                WaitUntil = WaitUntilState.Load,
                Timeout = 0
            });

            Random random = new Random();

            // Repeat 10 times: click a random article link every 5 seconds
            for (int iteration = 0; iteration < 10; iteration++)
            {
                // 3. Get all article links on the current page
                IReadOnlyList<IElementHandle> links = await page.QuerySelectorAllAsync("a[href^='/wiki/']:not([href*=':'])");

                // Extract href attributes into a list of strings using for-loop
                List<string> hrefs = new List<string>();
                
                for (int i = 0; i < links.Count; i++)
                {
                    IElementHandle link = links[i];
                    string href = await link.GetAttributeAsync("href");
                    if (!string.IsNullOrEmpty(href))
                    {
                        hrefs.Add(href);
                    }
                }

                if (hrefs.Count == 0)
                {
                    LibLog.LogInfo("No article links found on the page.");
                    break;
                }

                // 4. Pick a random article link href
                int index = random.Next(hrefs.Count);
                string randomHref = hrefs[index];

                // 5. Navigate to the random article URL
                string url = "https://en.wikipedia.org" + randomHref;
                LibLog.LogInfo($"Navigating to: {url}");
                await page.GotoAsync(url);

                // Wait for DOM content loaded
                await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

                // 6. Print recent resource requests from tab into terminal (last 5)
                List<TrackedRequest> requests = requestManager.GetAllRequests();
                LibLog.LogInfo("Recent resource requests:");
                int printedCount = 0;
                
                for (int j = requests.Count - 1; j >= 0 && printedCount < 5; j--)
                {
                    TrackedRequest req = requests[j];
                    LibLog.LogInfo($"  {req.PlaywrightRequest.Method} {req.Url} - Status: {req.Status} Code: {req.StatusCode}");
                    printedCount++;
                }

                // 7. Wait 5 seconds before clicking next random article
                await Task.Delay(5000);
            }

            await browser.CloseAsync();
        }
    }
}

