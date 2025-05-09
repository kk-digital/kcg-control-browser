using LogUtility;
using Microsoft.Playwright;

namespace utility;

public class CloudflareDetector
{
    public static async Task<bool> ContainsCloudflareChallengeAsync(IPage page, int maxRetries = 3)
    {
        int retryCount = 0;
        bool challengeDetected = false;

        // Try multiple times if Cloudflare challenge persists
        while (retryCount < maxRetries)
        {
            try
            {
                // Check for common Cloudflare Turnstile CAPTCHA script
                ILocator turnstileScript = page.Locator("script[src*='challenges.cloudflare.com/turnstile']");

                // Check for known Cloudflare challenge container
                ILocator challengeDiv = page.Locator("div[id*='cf-challenge'], div[class*='cf-challenge']");

                // Check for interstitial title or fingerprinting
                ILocator title = page.Locator("title");
                string pageTitle = await title.InnerTextAsync();

                // Check for visibility of Cloudflare elements
                bool hasTurnstileScript = await turnstileScript.IsVisibleAsync();
                bool hasChallengeDiv = await challengeDiv.IsVisibleAsync();
                bool isCloudflareTitle = pageTitle.Contains("Just a moment...") || pageTitle.Contains("Checking your browser");

                // If Cloudflare challenge is detected, wait for 30 seconds and retry
                if (hasTurnstileScript || hasChallengeDiv || isCloudflareTitle)
                {
                    LibLog.LogInfo("Cloudflare challenge detected. Waiting for 30 seconds...");
                    await Task.Delay(30000); // Asynchronous wait for 30 seconds
                    retryCount++;
                    challengeDetected = true;
                    LibLog.LogInfo($"Wait period over. Retry attempt {retryCount}...");
                }
                else
                {
                    // No challenge detected, proceed with scraping
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        // If Cloudflare challenge persists after retries
        if (challengeDetected)
        {
            LibLog.LogInfo("Cloudflare challenge persisted after multiple retries.");
        }

        // Return true if challenge is detected after all retries
        return true;
    }

    public static async Task<bool> ContainsPinterestCloudflareChallengeAsync(IPage page, int maxRetries = 3)
    {
        int retryCount = 0;
        bool challengeDetected = false;

        // Try multiple times if challenge persists
        while (retryCount < maxRetries)
        {
            try
            {
                // Check for Turnstile script from Cloudflare
                ILocator turnstileScript = page.Locator("script[src*='challenges.cloudflare.com/turnstile']");

                // Check for challenge container divs used by Cloudflare
                ILocator challengeContainer = page.Locator("div[id*='cf-challenge'], div[class*='cf-challenge']");

                // Check for common challenge titles Cloudflare uses
                ILocator title = page.Locator("title");
                string pageTitle = await title.InnerTextAsync();
                bool hasSuspiciousTitle = pageTitle.Contains("Just a moment") || pageTitle.Contains("Checking your browser");

                // Check if Turnstile CAPTCHA is present visually
                bool turnstileVisible = await turnstileScript.IsVisibleAsync();
                bool challengeVisible = await challengeContainer.IsVisibleAsync();

                // If Cloudflare challenge is detected, wait for 30 seconds
                if (turnstileVisible || challengeVisible || hasSuspiciousTitle)
                {
                    LibLog.LogInfo("Cloudflare challenge detected. Waiting for 30 seconds...");
                    await Task.Delay(30000); // Asynchronous wait for 30 seconds
                    retryCount++;
                    challengeDetected = true;
                    LibLog.LogInfo($"Wait period over. Retry attempt {retryCount}...");
                }
                else
                {
                    // No challenge detected, proceed with scraping
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        // If we exhausted retries and the challenge persists, return true or handle as needed
        if (challengeDetected)
        {
            LibLog.LogInfo("Cloudflare challenge persisted after multiple retries.");
            return true;
        }

        return false;
    }
}
