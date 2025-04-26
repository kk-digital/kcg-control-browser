using System.Collections.Generic;

namespace PageRequestInterface;

/// <summary>
/// A class that manages navigation history, allowing back and forward navigation between URLs.
/// </summary>
public class ControlBrowserNavigationHistory
{
    /// <summary>
    /// A list to store the history of visited URLs.
    /// </summary>
    private List<string> UrlHistory = new List<string>();

    /// <summary>
    /// The current index in the navigation history.
    /// </summary>
    private int CurrentIndex = -1;

    /// <summary>
    /// Adds a new URL to the navigation history. Removes any "forward" history if needed.
    /// </summary>
    /// <param name="url">The URL to add to the history.</param>
    public void AddUrl(string url)
    {
        // When adding a new URL, remove any "forward" history
        if (CurrentIndex < UrlHistory.Count - 1)
        {
            UrlHistory.RemoveRange(CurrentIndex + 1, UrlHistory.Count - CurrentIndex - 1);
        }
    
        UrlHistory.Add(url);
        CurrentIndex = UrlHistory.Count - 1;
    }

    /// <summary>
    /// Navigates back in the history, if possible.
    /// </summary>
    /// <returns>The previous URL, or null if back navigation is not possible.</returns>
    public string GoBack()
    {
        if (CurrentIndex > 0)
        {
            CurrentIndex--;
            return UrlHistory[CurrentIndex];
        }
    
        return null; // No previous URL
    }

    /// <summary>
    /// Navigates forward in the history, if possible.
    /// </summary>
    /// <returns>The next URL, or null if forward navigation is not possible.</returns>
    public string GoForward()
    {
        if (CurrentIndex < UrlHistory.Count - 1)
        {
            CurrentIndex++;
            return UrlHistory[CurrentIndex];
        }
    
        return null; // No next URL
    }

    /// <summary>
    /// Checks if back navigation is possible.
    /// </summary>
    /// <returns>True if a previous URL exists in the history, otherwise false.</returns>
    public bool CanGoBack()
    {
        return CurrentIndex > 0;
    }

    /// <summary>
    /// Checks if forward navigation is possible.
    /// </summary>
    /// <returns>True if a next URL exists in the history, otherwise false.</returns>
    public bool CanGoForward()
    {
        return CurrentIndex < UrlHistory.Count - 1;
    }

    /// <summary>
    /// Gets the current URL in the history.
    /// </summary>
    /// <returns>The current URL, or null if the history is empty.</returns>
    public string CurrentUrl()
    {
        if (CurrentIndex < 0)
        {
            return null;
        }

        return UrlHistory[CurrentIndex];
    }
}

