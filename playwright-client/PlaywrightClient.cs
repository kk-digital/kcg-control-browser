using LogUtility;
using Microsoft.Playwright;
using PlaywrightProxyManager;
using UtilityAppSettings;

namespace PlaywrightClient;
public class PlaywrightClient
{
    private MySetting _settings = null;
    private PlaywrightProxyInstance _proxyInstance = null;
    private PlaywrightTabManager _tabManager;
    public IBrowser Browser = null;
    public IBrowserContext Context = null;
    //=============================================================================================
    public struct Command_Type
    {
        public string command;
        public string args;
    }
    //=============================================================================================
    public PlaywrightProxyInstance Proxy
    {
        get
        {
            return _proxyInstance;  // Retrieve the value of the private field
        }
    }
    //=============================================================================================
    // constructor
    public PlaywrightClient(PlaywrightProxyInstance proxyInstance, MySetting settings)
    {
        _proxyInstance = proxyInstance;
        _settings = settings;
    }
    //=============================================================================================
    // launch browser
    public IBrowser LaunchBrowser()
    {
        try
        {
            // Initialize Playwright and launch the browser
            IPlaywright playWright = Playwright.CreateAsync().GetAwaiter().GetResult();

            // attempt to prevent the automation from being detected
            BrowserTypeLaunchOptions opt = new ();
            opt.ExecutablePath = _settings.ChromiumPath;
            opt.Headless = false;
            opt.Args = new[] {
                        "--proxy-server=" + _proxyInstance.IP + ":" + _proxyInstance.Port.ToString(),
                        "--disable-blink-features=AutomationControlled",  // Hide automation flag
                        "--disable-web-security"
                    };

            Browser = playWright.Chromium.LaunchAsync(opt).GetAwaiter().GetResult();
            Context = Browser.NewContextAsync().GetAwaiter().GetResult();
            _tabManager = new PlaywrightTabManager(Context);

            return Browser;
        }
        catch (Exception ex)
        {
            LibLog.LogError(ex.Message);
            return null;
        }
    }
    //=============================================================================================
    // launch browser using specified user data directory
    public IBrowserContext LaunchBrowser(string UserDataDir = "")
    {
        try
        {
            string userDataDir = "";

            if (string.IsNullOrEmpty(UserDataDir))
            {
                userDataDir = _settings.UserDataFolder;
            }
            else
            {
                userDataDir = UserDataDir;
            }

            // Initialize Playwright and launch the browser
            IPlaywright playWright = Playwright.CreateAsync().GetAwaiter().GetResult();

            // attempt to prevent the automation from being detected
            BrowserTypeLaunchPersistentContextOptions opt = new BrowserTypeLaunchPersistentContextOptions();
            opt.ExecutablePath = _settings.ChromiumPath;
            opt.Headless = false;
            opt.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36";

            if (!string.IsNullOrEmpty(_proxyInstance.Username) && !string.IsNullOrEmpty(_proxyInstance.Password))
            {
                opt.Proxy = new Proxy
                {
                    Server = $"{_proxyInstance.IP}:{_proxyInstance.Port.ToString()}",
                    Username = _proxyInstance.Username,
                    Password = _proxyInstance.Password
                };
            }
            else
            {
                opt.Proxy = new Proxy
                {
                    Server = $"{_proxyInstance.IP}:{_proxyInstance.Port.ToString()}"
                };
            }

            opt.Args = new[] {
                        "--disable-blink-features=AutomationControlled",
                        "--disable-web-security"
                        };

            // Launch Chromium with the user data directory
            Context = playWright.Chromium.LaunchPersistentContextAsync(userDataDir, opt).GetAwaiter().GetResult();
            _tabManager = new PlaywrightTabManager(Context);

            return Context;
        }
        catch (Exception ex)
        {
            LibLog.LogError(ex.Message);
            return null;
        }
    }
    //=============================================================================================
    // close the browser
    public void CloseBrowser()
    {
        try
        {
            // Close all browser contexts before closing the browser
            IBrowserContext context;
            
            for (int i = 0; i < Browser.Contexts.Count; i++)
            {
                context = Browser.Contexts[i];
                context.CloseAsync().GetAwaiter().GetResult();
            }

            Browser.CloseAsync().GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            LibLog.LogError(ex.Message);
        }
    }
    //=============================================================================================================================
    // close the browser
    public void CloseBrowser(IBrowserContext context)
    {
        try
        {
            context.CloseAsync().GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            LibLog.LogError(ex.Message);
        }
    }
    //=============================================================================================================================
    // connect and create a new browser context from exising browser
    public IBrowserContext Connect()
    {
        try
        {
            BrowserNewContextOptions opt = new ();
            opt.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36"; // Set the custom User-Agent
            IBrowserContext context = Browser.NewContextAsync(opt).GetAwaiter().GetResult();

            return context;
        }
        catch (Exception ex)
        {
            LibLog.LogError(ex.Message);
            return null;
        }
    }
    //=============================================================================================================================
    // close browser context and disconnect from browser
    public void Disconnect(IBrowserContext context)
    {
        try
        {
            context.CloseAsync().GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            LibLog.LogError(ex.Message);
        }
    }
    //=============================================================================================
    public void ExecuteCommand(Command_Type command, IBrowserContext context, IPage page)
    {
        //return async () => await PlaywrightDOMGrabber.GrabDOMasString(sr);
        //_action = () => { Console.WriteLine(); }; // just to make sure _action variable does not contain an empty action to avoid exiting the main loop
        //string[] words = command.Split(); //split the command using space character as delimiter

        PageGotoOptions opt = new ();

        opt.Timeout = 25000;
        opt.WaitUntil = WaitUntilState.NetworkIdle;

        switch (command.command) // determine which key command was sent
        {
            case "open in new tab": // eg. open in current tab <url>
                OpenInNewTab(command, context, opt);
                break;

            case "switch tab": // eg. open in current tab <url>
                SwitchTab(command);
                break;

            case "scroll": // eg. open in current tab <url>
                ScrollTab(command);
                break;

            case "get DOM": // eg. open in current tab <url>
                GetTabDOM(command, context, opt);
                break;
        }
    }
    //=============================================================================================
    string GetTabDOM(Command_Type command, IBrowserContext context, PageGotoOptions opt)
    {
        try
        {
            return _tabManager.GetTabDOM(Convert.ToInt32(command.args));
        }
        catch (Exception)
        {
            return null;
        }
    }
    //=============================================================================================
    void OpenInNewTab(Command_Type command, IBrowserContext context, PageGotoOptions opt)
    {
        _tabManager.OpenNewTab(command.args, context, opt);
    }
    //=============================================================================================
    void SwitchTab(Command_Type command)
    {
        _tabManager.SwitchActiveTab(Convert.ToInt32(command.args));
    }
    //=============================================================================================
    void ScrollTab(Command_Type command)
    {
        string[] tokens = command.args.Split(",");
        _tabManager.ScrollMouseWheel(Convert.ToInt32(tokens[0]), Convert.ToInt32(tokens[1]));
    }
    //=============================================================================================
}