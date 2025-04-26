namespace Utility;

/// <summary>
/// use by EventTriggered class to have a consistent format for event entries into mongoDb
/// </summary>
public enum EventType
{
    FOUND,
    SCRAPE_STARTED,
    SCRAPE_SUCCESSFUL,
    SCRAPE_FAILED
}