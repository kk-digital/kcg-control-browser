using LogUtility;
using Microsoft.Playwright;

namespace utility;

public class CaptchaDetector
{
    public static async Task<bool> ContainsYouTubeCaptchaAsync(IPage page, int maxRetries = 3)
    {
        int retryCount = 0;
        bool captchaDetected = false;

        // Try multiple times if CAPTCHA persists
        while (retryCount < maxRetries)
        {
            try
            {
                // Try to find common CAPTCHA-related elements
                ILocator recaptchaIframe = page.Locator("iframe[src*='google.com/recaptcha']");
                ILocator challengeForm = page.Locator("form[action*='sorry/index']"); // Used in some YouTube CAPTCHA redirects
                ILocator challengeScript = page.Locator("script[src*='recaptcha']");

                // Check if these elements are visible on the page
                bool hasRecaptcha = await recaptchaIframe.IsVisibleAsync();
                bool hasChallengeForm = await challengeForm.IsVisibleAsync();
                bool hasChallengeScript = await challengeScript.IsVisibleAsync();

                // If CAPTCHA is detected, wait for 30 seconds and retry
                if (hasRecaptcha || hasChallengeForm || hasChallengeScript)
                {
                    LibLog.LogInfo("CAPTCHA detected. Waiting for 30 seconds...");
                    await Task.Delay(30000); // Wait for 30 seconds
                    retryCount++;
                    captchaDetected = true;
                    LibLog.LogInfo($"Wait period over. Retry attempt {retryCount}...");
                }
                else
                {
                    // No CAPTCHA detected, proceed with scraping
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        // If CAPTCHA persists after retries
        if (captchaDetected)
        {
            LibLog.LogInfo("CAPTCHA persisted after multiple retries.");
        }

        // Return true if CAPTCHA is detected after all retries
        return true;
    }

    public static async Task<bool> ContainsPinterestCaptchaAsync(IPage page, int maxRetries = 3)
    {
        int retryCount = 0;
        bool captchaDetected = false;

        // Try multiple times if CAPTCHA persists
        while (retryCount < maxRetries)
        {
            try
            {
                // Check for image-based CAPTCHA
                ILocator captchaImage = page.Locator("img[src*='/captcha/']");
    
                // Check for CAPTCHA container
                ILocator captchaContainer = page.Locator("div[data-test-id*='captcha']");
    
                // Check for Cloudflare Turnstile CAPTCHA
                ILocator turnstileScript = page.Locator("script[src*='challenges.cloudflare.com/turnstile']");

                // Check if CAPTCHA-related elements are visible
                bool hasCaptchaImage = await captchaImage.IsVisibleAsync();
                bool hasCaptchaContainer = await captchaContainer.IsVisibleAsync();
                bool hasTurnstileScript = await turnstileScript.IsVisibleAsync();

                // If CAPTCHA is detected, wait for 30 seconds and retry
                if (hasCaptchaImage || hasCaptchaContainer || hasTurnstileScript)
                {
                    LibLog.LogInfo("CAPTCHA detected. Waiting for 30 seconds...");
                    await Task.Delay(30000); // Asynchronous wait for 30 seconds
                    retryCount++;
                    captchaDetected = true;
                    LibLog.LogInfo($"Wait period over. Retry attempt {retryCount}...");
                }
                else
                {
                    // No CAPTCHA detected, proceed with scraping
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        // If CAPTCHA persists after retries
        if (captchaDetected)
        {
            LibLog.LogInfo("CAPTCHA persisted after multiple retries.");
        }

        // Return true if CAPTCHA is detected after all retries
        return true;
    }


}
