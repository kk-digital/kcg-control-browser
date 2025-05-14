using lib;
using Microsoft.Playwright;

namespace playwright_browser_automation
{
    // Manages multiple browser windows, each with its own context and tabs.
    // Tracks active window and provides APIs to create, switch, list, and close windows and tabs.
    public class BrowserWindowManager
    {
        // Internal array holding all managed browser windows.
        private BrowserWindow[] _windows = Array.Empty<BrowserWindow>();

        // The unique identifier of the currently active window.
        // Uid64.Default indicates no active window is set.
        private Uid64 _activeWindowId = Uid64.Default;

        // Flag indicating whether an active window is currently set.
        private bool _hasActiveWindow = false;

        // The Playwright browser instance used to create new contexts and pages.
        private readonly IBrowser _browser;

        // The tab manager responsible for creating and managing tabs across windows.
        private readonly BrowserTabManager _tabManager;

        // Constructor initializes the manager with the Playwright browser and tab manager.
        public BrowserWindowManager(IBrowser browser, BrowserTabManager tabManager)
        {
            _browser = browser;
            _tabManager = tabManager;
        }

        // Adds a new BrowserWindow to the internal array, resizing it to fit the new window.
        private void AddWindow(BrowserWindow window)
        {
            BrowserWindow[] newWindows = new BrowserWindow[_windows.Length + 1];
            for (int i = 0; i < _windows.Length; i++)
            {
                newWindows[i] = _windows[i];
            }
            newWindows[_windows.Length] = window;
            _windows = newWindows;
        }

        // Removes the BrowserWindow with the specified windowId from the internal array.
        // Resizes the array to exclude the removed window.
        // Clears active window tracking if the removed window was active.
        private void RemoveWindow(Uid64 windowId)
        {
            int index = -1;
            for (int i = 0; i < _windows.Length; i++)
            {
                if (_windows[i].Id == windowId)
                {
                    index = i;
                    break;
                }
            }
            if (index == -1)
            {
                // Window not found, no action needed.
                return;
            }

            BrowserWindow[] newWindows = new BrowserWindow[_windows.Length - 1];
            int newIndex = 0;
            for (int i = 0; i < _windows.Length; i++)
            {
                if (i != index)
                {
                    newWindows[newIndex++] = _windows[i];
                }
            }
            _windows = newWindows;

            // If the removed window was active, clear active window tracking.
            if (_hasActiveWindow && _activeWindowId == windowId)
            {
                _activeWindowId = Uid64.Default;
                _hasActiveWindow = false;
            }
        }

        // Creates a new browser window with a fresh browser context and initial tab.
        // Adds the window to the internal array and sets it as active.
        public async Task<BrowserWindow> CreateWindowAsync()
        {
            IBrowserContext context = await _browser.NewContextAsync();
            BrowserWindow window = new BrowserWindow();
            window.Context = context;

            // Create the initial tab in this new window.
            IPage page = await context.NewPageAsync();
            BrowserTab tab = _tabManager.CreateTab(page);

            window.AddTab(tab);
            window.ActiveTabId = tab.Id;
            AddWindow(window);

            // Set the newly created window as the active window.
            _activeWindowId = window.Id;
            _hasActiveWindow = true;

            return window;
        }

        // Gets the currently active BrowserWindow.
        // Returns null if no active window is set.
        public BrowserWindow GetActiveWindow()
        {
            if (!_hasActiveWindow)
            {
                return null;
            }

            for (int i = 0; i < _windows.Length; i++)
            {
                if (_windows[i].Id == _activeWindowId)
                {
                    return _windows[i];
                }
            }
            return null;
        }

        // Lists all managed browser windows.
        public BrowserWindow[] ListWindows()
        {
            return _windows;
        }

        // Switches the active window to the window with the specified windowId.
        // If the windowId is not found, no change is made.
        public Task SwitchToWindowAsync(Uid64 windowId)
        {
            for (int i = 0; i < _windows.Length; i++)
            {
                if (_windows[i].Id == windowId)
                {
                    _activeWindowId = windowId;
                    _hasActiveWindow = true;
                    break;
                }
            }
            return Task.CompletedTask;
        }

        // Sets the active window programmatically by windowId.
        // Returns true if the window was found and set active, false otherwise.
        public bool SetActiveWindow(Uid64 windowId)
        {
            for (int i = 0; i < _windows.Length; i++)
            {
                if (_windows[i].Id == windowId)
                {
                    _activeWindowId = windowId;
                    _hasActiveWindow = true;
                    return true;
                }
            }
            return false;
        }

        // Creates a new tab in the currently active window.
        // Returns the newly created tab or null if no active window is set.
        public async Task<BrowserTab> CreateTabInActiveWindowAsync()
        {
            BrowserWindow window = GetActiveWindow();
            if (window == null)
            {
                return null;
            }

            IPage page = await window.Context.NewPageAsync();
            BrowserTab tab = _tabManager.CreateTab(page);
            window.AddTab(tab);
            window.ActiveTabId = tab.Id;
            return tab;
        }

        // Switches the active tab in the active window to the tab with the specified tabId.
        // Brings the tab's page to the front if found.
        public async Task SwitchToTabAsync(Uid64 tabId)
        {
            BrowserWindow window = GetActiveWindow();
            if (window == null)
            {
                return;
            }

            BrowserTab tab = window.FindTab(tabId);

            if (tab != null && tab.Page != null)
            {
                await tab.Page.BringToFrontAsync();
                window.ActiveTabId = tabId;
            }
        }

        // Closes the browser window with the specified windowId.
        // Closes all tabs in the window and disposes the browser context.
        public async Task CloseWindowAsync(Uid64 windowId)
        {
            for (int i = 0; i < _windows.Length; i++)
            {
                if (_windows[i].Id == windowId)
                {
                    BrowserWindow window = _windows[i];
                    BrowserTab[] tabs = window.Tabs;
                    for (int j = 0; j < tabs.Length; j++)
                    {
                        BrowserTab tab = tabs[j];
                        _tabManager.CloseTab(tab.Id);
                    }
                    await window.Context.CloseAsync();
                    RemoveWindow(windowId);
                    break;
                }
            }
        }
    }
}
