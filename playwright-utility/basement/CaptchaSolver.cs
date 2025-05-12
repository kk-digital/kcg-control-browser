// Explanation
// SolvedCaptchaAsync
// This method is designed to detect and solve reCAPTCHA v2 challenges by:
//
//      Waiting for the CAPTCHA iframe.
//
//      Extracting the site key.
//
//      Submitting to 2Captcha with method=userrecaptcha.
//
//      Injecting the returned token into the page.
//
// SolveRecaptchaV3OnPageAsync
// This method is designed to detect and solve reCAPTCHA v3 challenges by:
//
//      Finding the reCAPTCHA v3 element.
//
//      Extracting the site key and action.
//
//      Creating a JSON task with 2Captcha’s RecaptchaV3TaskProxyless method.
//
//      Polling for the token and injecting it.

// sample usage:

    // --- Solve reCAPTCHA v2 ---
    // bool v2Solved = await CaptchaSolver.SolvedPinterestCaptchaAsync(page);
    // if (v2Solved)
    // {
    //     Console.WriteLine("reCAPTCHA v2 solved successfully.");
    //     // Proceed with scraping or form submission
    // }
    // else
    // {
    //     Console.WriteLine("Failed to solve reCAPTCHA v2.");
    // }
    //
    // // --- Solve reCAPTCHA v3 ---
    // bool v3Solved = await CaptchaSolver.SolveRecaptchaV3OnPageAsync(page, minScore: 0.3f, action: "verify");
    // if (v3Solved)
    // {
    //     Console.WriteLine("reCAPTCHA v3 solved successfully.");
    //     // Proceed with scraping or form submission
    // }
    // else
    // {
    //     Console.WriteLine("Failed to solve reCAPTCHA v3.");
    // }

using System.Text;
using LogUtility;

namespace utility;

using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Playwright;
using System.Text.Json;

public class CaptchaSolver
{
    private static readonly HttpClient httpClient = new HttpClient();

    // from CMD:=> set APIKEY_2CAPTCHA = <your_actual_api_key_here>

    private static string twoCaptchaApiKey = Environment.GetEnvironmentVariable("APIKEY_2CAPTCHA");
    
    public static async Task<bool> SolvedCaptchaAsync(IPage page)
    {
        // Wait for the reCAPTCHA iframe to appear (adjust selector if needed)
        IElementHandle iframeElement = await page.WaitForSelectorAsync("iframe[src*='recaptcha']");
        
        if (iframeElement == null)
        {
            LibLog.LogError("CAPTCHA iframe not found.");
            return false;
        }

        // Get iframe src attribute
        string iframeSrc = await iframeElement.GetAttributeAsync("src");
        
        if (iframeSrc == null)
        {
            LibLog.LogError("CAPTCHA iframe src attribute missing.");
            return false;
        }

        // Extract sitekey from iframe src URL
        Uri uri = new Uri(iframeSrc);
        System.Collections.Specialized.NameValueCollection query = System.Web.HttpUtility.ParseQueryString(uri.Query);
        string siteKey = query.Get("k");
        
        if (string.IsNullOrEmpty(siteKey))
        {
            LibLog.LogError("CAPTCHA sitekey not found.");
            return false;
        }

        LibLog.LogInfo($"Found CAPTCHA sitekey: {siteKey}");

        // Solve CAPTCHA via 2Captcha
        string captchaToken = await GetSolvedRecaptchaTokenAsync(siteKey, page.Url);

        if (string.IsNullOrWhiteSpace(captchaToken))
        {
            return false;
        }
        
        LibLog.LogInfo("CAPTCHA solved, token received.");

        // Inject the token into the page
        // Google reCAPTCHA expects the token in textarea with id 'g-recaptcha-response'
        await page.EvaluateAsync(@"(token) => {
            document.querySelector('#g-recaptcha-response').style.display = 'block';
            document.querySelector('#g-recaptcha-response').value = token;
        }", captchaToken);

        LibLog.LogInfo("CAPTCHA token injected.");
        
        return true;
    }

    public static async Task<string> GetSolvedRecaptchaTokenAsync(string siteKey, string pageUrl)
    {
        // Submit CAPTCHA for solving
        string submitUrl = $"http://2captcha.com/in.php?key={twoCaptchaApiKey}&method=userrecaptcha&googlekey={siteKey}&pageurl={pageUrl}&json=1";
        HttpResponseMessage submitResponse = await httpClient.GetAsync(submitUrl);
        string submitContent = await submitResponse.Content.ReadAsStringAsync();
        JsonDocument submitJson = JsonDocument.Parse(submitContent);
        
        if (!submitJson.RootElement.GetProperty("status").GetBoolean())
        {
            LibLog.LogError("Failed to submit CAPTCHA: " + submitContent);
            return null;
        }

        string captchaId = submitJson.RootElement.GetProperty("request").GetString();

        // Poll for CAPTCHA result
        string resultUrl = $"http://2captcha.com/res.php?key={twoCaptchaApiKey}&action=get&id={captchaId}&json=1";
        string token = null;

        for (int i = 0; i < 20; i++) // try for up to ~100 seconds
        {
            await Task.Delay(5000); // wait 5 seconds before polling
            HttpResponseMessage resultResponse = await httpClient.GetAsync(resultUrl);
            string resultContent = await resultResponse.Content.ReadAsStringAsync();
            JsonDocument resultJson = JsonDocument.Parse(resultContent);

            if (resultJson.RootElement.GetProperty("status").GetBoolean())
            {
                token = resultJson.RootElement.GetProperty("request").GetString();
                break;
            }
            else if (resultJson.RootElement.GetProperty("request").GetString() != "CAPCHA_NOT_READY")
            {
                LibLog.LogError("Error solving CAPTCHA: " + resultContent);
                return null;
            }
        }

        if (token == null)
        {
            LibLog.LogError("CAPTCHA solving timed out.");
            return null;
        }

        return token;
    }
    
    public static async Task<string> SolveRecaptchaV3Async(
        string siteKey, 
        string pageUrl, 
        float minScore = 0.3f, 
        string action = "verify")
    {
        if (string.IsNullOrWhiteSpace(twoCaptchaApiKey))
        {
            LibLog.LogError("2Captcha API key not found in environment variables.");
            return null;
        }

        // Create JSON payload for RecaptchaV3TaskProxyless
        JsonSerializerOptions options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        object taskPayload = new
        {
            clientKey = twoCaptchaApiKey,
            task = new
            {
                type = "RecaptchaV3TaskProxyless",
                websiteUrl = pageUrl,
                websiteKey = siteKey,
                minScore = minScore,
                pageAction = action
            }
        };

        string jsonPayload = JsonSerializer.Serialize(taskPayload, options);
        StringContent content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        // Create task
        HttpResponseMessage createResponse = await httpClient.PostAsync(
            "https://api.2captcha.com/createTask", 
            content);
        
        string createResult = await createResponse.Content.ReadAsStringAsync();
        JsonDocument createJson = JsonDocument.Parse(createResult);
        
        if (createJson.RootElement.GetProperty("errorId").GetInt32() != 0)
        {
            LibLog.LogError("Failed to create task: " + createResult);
            return null;
        }

        int taskId = createJson.RootElement.GetProperty("taskId").GetInt32();

        // Poll for results
        for (int i = 0; i < 20; i++)
        {
            await Task.Delay(5000);
            
            HttpResponseMessage resultResponse = await httpClient.GetAsync(
                "https://api.2captcha.com/getTaskResult?clientKey=" + twoCaptchaApiKey + "&taskId=" + taskId);
            
            string resultContent = await resultResponse.Content.ReadAsStringAsync();
            JsonDocument resultJson = JsonDocument.Parse(resultContent);

            string status = resultJson.RootElement.GetProperty("status").GetString();
            int errorId = resultJson.RootElement.GetProperty("errorId").GetInt32();

            if (errorId != 0)
            {
                LibLog.LogError("Error solving CAPTCHA: " + resultContent);
                return null;
            }

            if (status == "ready")
            {
                string token = resultJson.RootElement
                    .GetProperty("solution")
                    .GetProperty("gRecaptchaResponse")
                    .GetString();
                return token;
            }
        }

        LibLog.LogError("CAPTCHA solving timed out.");
        return null;
    }

    public static async Task<bool> SolveRecaptchaV3OnPageAsync(
        IPage page, 
        float minScore = 0.3f, 
        string action = null)
    {
        // Extract reCAPTCHA v3 parameters from page
        IElementHandle recaptchaDiv = await page.WaitForSelectorAsync(
            "div[data-sitekey], div.g-recaptcha",
            new PageWaitForSelectorOptions { Timeout = 30000 });
        
        if (recaptchaDiv == null)
        {
            LibLog.LogError("reCAPTCHA v3 element not found on page");
            return false;
        }

        string siteKey = await recaptchaDiv.GetAttributeAsync("data-sitekey");
        if (string.IsNullOrEmpty(siteKey))
        {
            LibLog.LogError("Failed to extract sitekey from reCAPTCHA element");
            return false;
        }

        string finalAction;

        if (string.IsNullOrEmpty(action))
        {
            finalAction = await GetRecaptchaActionAsync(page);
            if (string.IsNullOrEmpty(finalAction))
            {
                finalAction = "verify";
            }
        }
        else
        {
            finalAction = action;
        }

        // Solve CAPTCHA
        string token = await SolveRecaptchaV3Async(
            siteKey, 
            page.Url, 
            minScore, 
            finalAction);

        if (string.IsNullOrEmpty(token))
        {
            return false;
        }

        // Inject token into page
        await page.EvaluateAsync(@"(token) => {
            let responseElement = document.querySelector('#g-recaptcha-response');
            if (!responseElement) {
                responseElement = document.createElement('textarea');
                responseElement.id = 'g-recaptcha-response';
                responseElement.style.display = 'none';
                document.body.appendChild(responseElement);
            }
            responseElement.value = token;
        }", token);

        LibLog.LogInfo("Successfully injected reCAPTCHA v3 token");
        return true;
    }

    private static async Task<string> GetRecaptchaActionAsync(IPage page)
    {
        string action = await page.EvaluateAsync<string>(@"() => {
            const scripts = Array.from(document.getElementsByTagName('script'));
            const recaptchaScript = scripts.find(s => s.innerHTML.includes('grecaptcha.execute'));
            
            if (recaptchaScript) {
                const match = recaptchaScript.innerHTML.match(/action:\s*['""]([^'""]+)['""]/);
                if (match) {
                    return match[1];
                }
            }
            return '';
        }");

        if (string.IsNullOrEmpty(action))
        {
            return "";
        }

        return action;
    }
}


