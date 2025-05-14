using Microsoft.Playwright;
using playwright_browser_automation;

namespace browser_control_example;

class BrowserControlExampleProgram
{
    public static async Task Main()
    {
        // Initialize Playwright and launch browser
        using var playwright = await Playwright.CreateAsync();
        BrowserTypeLaunchOptions options = new();
        options.Headless = false;
        options.ExecutablePath = @"\chrome.exe";
        await using IBrowser browser = await playwright.Chromium.LaunchAsync(options);

        // Create a new browser context and page
        IBrowserContext context = await browser.NewContextAsync();
        IPage page = await context.NewPageAsync();

        // Create an instance of your RequestManager
        RequestManager requestManager = new RequestManager();

        // Attach the RequestManager to the page to track network requests
        requestManager.AttachToPage(page);

        // Navigate to a website
        await page.GotoAsync("https://www.wikipedia.org");

        // Wait a bit to allow requests to complete
        await page.WaitForTimeoutAsync(3000);

        // Retrieve all tracked requests
        List<TrackedRequest> requests = requestManager.GetAllRequests();

        // Print summary of each tracked request
        Console.Clear();
        
        for (int i = 0; i < requests.Count; i++)
        {
            TrackedRequest req = requests[i];
            Console.WriteLine("Method: " + req.PlaywrightRequest.Method + ", URL: " + req.Url);
            Console.WriteLine("Status: " + req.Status + ", HTTP Status Code: " + req.StatusCode);
            Console.WriteLine("Started: " + req.StartTime + ", Ended: " + req.EndTime);
            int responseLength = (req.ResponseBody != null) ? req.ResponseBody.Length : 0;
            Console.WriteLine("Response body length: " + responseLength + " characters");
            Console.WriteLine(new string('-', 80));
        }

        // Close browser
        await browser.CloseAsync();
    }
}