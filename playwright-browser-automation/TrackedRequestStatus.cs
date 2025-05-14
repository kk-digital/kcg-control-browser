namespace playwright_browser_automation;

// Represents the status of a network request.
public enum RequestStatus
{
    Pending,        // Request has been issued but response not yet received.
    Partial,        // Partial response received (e.g., streaming).
    Finished,       // Response fully received.
    Failed          // Request failed (network error, timeout, etc).
}
