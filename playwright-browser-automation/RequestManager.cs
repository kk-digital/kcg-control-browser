using Microsoft.Playwright;

namespace playwright_browser_automation
{
    // Manages network requests for a browser tab or window.
    // Tracks all requests, their status, and provides query APIs.
    public class RequestManager
    {
        // Internal list of tracked requests.
        private List<TrackedRequest> _requests = new List<TrackedRequest>();

        // Lock object for thread safety.
        private readonly object _lock = new object();

        // Attach event handlers to a Playwright page to track requests.
        public void AttachToPage(IPage page)
        {
            // When a request is issued, add it to the list.
            page.Request += (sender, request) =>
            {
                lock (_lock)
                {
                    _requests.Add(new TrackedRequest(request));
                }
            };

            // When a response is received, update the tracked request.
            page.Response += async (sender, response) =>
            {
                lock (_lock)
                {
                    TrackedRequest tracked = FindTrackedRequest(response.Request);
                    if (tracked != null)
                    {
                        // We cannot await inside lock, so mark status here and fetch body outside.
                        tracked.StatusCode = response.Status;
                    }
                }

                // Fetch response body outside lock to avoid blocking.
                string body = null;
                try
                {
                    body = await response.TextAsync();
                }
                catch
                {
                    // Ignore errors reading body.
                }

                lock (_lock)
                {
                    TrackedRequest tracked = FindTrackedRequest(response.Request);
                    if (tracked != null)
                    {
                        tracked.MarkFinished(response, body);
                    }
                }
            };

            // When request finishes successfully.
            page.RequestFinished += (sender, request) =>
            {
                lock (_lock)
                {
                    TrackedRequest tracked = FindTrackedRequest(request);
                    if (tracked != null && tracked.Status != RequestStatus.Finished)
                    {
                        tracked.Status = RequestStatus.Finished;
                        tracked.EndTime = DateTimeOffset.Now;
                    }
                }
            };

            // When request fails.
            page.RequestFailed += (sender, request) =>
            {
                lock (_lock)
                {
                    TrackedRequest tracked = FindTrackedRequest(request);
                    if (tracked != null)
                    {
                        tracked.MarkFailed();
                    }
                }
            };
        }

        // Finds the tracked request corresponding to a Playwright IRequest.
        private TrackedRequest FindTrackedRequest(IRequest request)
        {
            for (int i = 0; i < _requests.Count; i++)
            {
                if (_requests[i].PlaywrightRequest == request)
                {
                    return _requests[i];
                }
            }
            return null;
        }

        // Returns a snapshot list of all tracked requests.
        public List<TrackedRequest> GetAllRequests()
        {
            lock (_lock)
            {
                return new List<TrackedRequest>(_requests);
            }
        }

        // Returns the number of requests currently tracked.
        public int GetRequestCount()
        {
            lock (_lock)
            {
                return _requests.Count;
            }
        }
    }
}
