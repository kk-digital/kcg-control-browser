using Microsoft.Playwright;
using System.Net;
using System.Reflection;
using LogUtility;
using Titanium.Web.Proxy;
using Titanium.Web.Proxy.EventArguments;
using Titanium.Web.Proxy.Models;
using Utility;
using UtilityAppSettings;
using UtilityWarcTools;

namespace playwright_web_proxy_server;

// use for proxy server operations
public class ServerRunner
{
    private bool bCloseBrowser;
    private string URL;
    private WARCfileWriter wf;
    private LogFileWriter lf;
    private string appDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
    private string warcFilePath;
    private string logFilePath;
    private ProxyServer proxyServer;
    private IPage page;
    //==============================================================================================================================
    // constructor accepts a URL navigate to
    public ServerRunner(string url)
    {
        URL = url;
    }
    //==============================================================================================================================
    // allow to get a reference to the displayed page for operations like mouse scroll and grab DOM
    public IPage GetPage() // use by other action commands to makes sure an page context exist
    {
        return page;
    }
    //==============================================================================================================================
    // start the proxy server and navigate to the URL passed to the constructor
    public void StartProxyServer()
    {
        // read values from settings file
        MySetting ms = MyAppSettings.ReadSettings();

        if (ms != null) // make sure the settings file is successfully read
        {
            proxyServer = new ProxyServer();

            //subscribe to events that allow capturing the web requests and responses
            proxyServer.BeforeRequest += OnBeforeRequest;
            proxyServer.BeforeResponse += OnBeforeResponse;

            // passes the proxy ip and port
            ExplicitProxyEndPoint explicitEndPoint = new (new IPAddress(IPAddress.Parse(ms.ProxyIPaddress).GetAddressBytes()), ms.Port, true);

            // add an endpoint to the server's collection of endpoints
            proxyServer.AddEndPoint(explicitEndPoint);

            // retrieve filenames for log file and warc file and setup corresponding paths
            warcFilePath = @$"{appDir}\data-out\{ms.WARCfile}";
            logFilePath = @$"{appDir}\data-out\{ms.Logfile}";

            // create instances for log file and warc file writers
            wf = new WARCfileWriter(warcFilePath);
            lf = new LogFileWriter(logFilePath);

            // start the proxy server
            proxyServer.Start();

            // initializes the Playwright object
            using IPlaywright playwright = Playwright.CreateAsync().GetAwaiter().GetResult();

            // attempt to prevent the automation from being detected
            BrowserTypeLaunchPersistentContextOptions opt = new BrowserTypeLaunchPersistentContextOptions();
            opt.ExecutablePath = ms.ChromiumPath;
            opt.Headless = false;
            opt.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36";
            opt.Args = new[] {
                "--proxy-server=" + ms.ProxyIPaddress + ":" + ms.Port.ToString(),
                "--disable-blink-features=AutomationControlled",  // Hide automation flag
                "--disable-web-security"
            };

            // Launch Chromium with the user data directory
            IBrowserContext browser = playwright.Chromium.LaunchPersistentContextAsync(ms.UserDataFolder, opt).GetAwaiter().GetResult();
            page = browser.NewPageAsync().GetAwaiter().GetResult();

            // Remove Playwright detection signs
            page.EvaluateAsync(@"() => {
                    delete navigator.__proto__.webdriver;
                    window.chrome = {
                        runtime: {}
                    };
                    Object.defineProperty(navigator, 'languages', { get: () => ['en-US', 'en'] });
                    Object.defineProperty(navigator, 'plugins', { get: () => [1, 2, 3, 4, 5] });
                }").GetAwaiter().GetResult();

            try
            {
                // navigate to URL
                page.GotoAsync(URL, new PageGotoOptions() { WaitUntil = WaitUntilState.DOMContentLoaded, Timeout = 0 }).GetAwaiter().GetResult();

                // Wait for the 'DOMContentLoaded' event
                page.WaitForLoadStateAsync(LoadState.DOMContentLoaded).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                page = null;
                wf = null;
                lf = null;
                explicitEndPoint = null;
                browser = null;

                try
                {
                    if (proxyServer != null)
                    {
                        //unsubscribe to events that allow capturing the web requests and responses
                        proxyServer.BeforeRequest -= OnBeforeRequest;
                        proxyServer.BeforeResponse -= OnBeforeResponse;
                        proxyServer.Stop();
                    }
                }
                catch (Exception)
                {
                }

                proxyServer = null;
                LibLog.LogError(ex.Message);
                return;
            }

            bCloseBrowser = false;

            while (true) // use to prevent the browser from auto closing
            {
                if (bCloseBrowser)
                {
                    break;
                }

                Task.Delay(5000);
            }
        }
        else
        {
            LibLog.LogError("Error reading settings file. Press any key to exit");
            Console.ReadKey(true);
            return;
        }
    }
    //==============================================================================================================================
    // fires when the proxy receives a request from a client before it is sent to the target server
    async Task OnBeforeRequest(object sender, SessionEventArgs e)
    {
        Guid id = Guid.NewGuid();
        await lf.LogRequest(e, id); // write requests to log file
        await wf.WriteWARCrequestRecord(e, id); // write request to warc file
    }
    //==============================================================================================================================
    // fires when the proxy receives a response from the server before sending it to the client
    async Task OnBeforeResponse(object sender, SessionEventArgs e)
    {
        Guid id = Guid.NewGuid(); // generate unique id for the record
        await lf.LogResponse(e, id); // write response data to log file
        await wf.WriteWARCresponseRecord(e.HttpClient.Request, e, id); // write response data to warc file
    }
    //==============================================================================================================================
    public void CloseBrowser() //close the browser
    {
        try
        {
            // unsubscribe to the events which intercepts requests and responses
            if (proxyServer != null)
            {
                proxyServer.BeforeRequest -= OnBeforeRequest;
                proxyServer.BeforeResponse -= OnBeforeResponse;
                proxyServer.Stop();
                bCloseBrowser = true;
            }

        }
        catch (Exception ex)
        {
            LibLog.LogError(ex.Message);
            Console.ReadKey(true);
        }
    }
    //==============================================================================================================================
}
