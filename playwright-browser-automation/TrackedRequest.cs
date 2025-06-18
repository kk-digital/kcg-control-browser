using XlibUid;
using Microsoft.Playwright;

namespace playwright_browser_automation;

// Represents a tracked network request with details and status.
public class TrackedRequest
{
    // Unique identifier for this request.
    public Uid64 Id { get; }

    // The URL requested.
    public string Url { get; set; }

    // The HTTP method used (GET, POST, DELETE, etc).
    public string Method { get; set; }

    // The request post data or query parameters (if any).
    public string PostData { get; set; }

    // The HTTP status code from the response (e.g., 200, 404).
    // Zero if no response yet.
    public int StatusCode { get; set; }

    // The time the request was initiated.
    public DateTimeOffset StartTime { get; set; }

    // The time the request finished or failed.
    public DateTimeOffset EndTime { get; set; }

    // The current status of the request.
    public RequestStatus Status { get; set; }

    // The response body as string (if available).
    public string ResponseBody { get; set; }

    // Internal Playwright request reference (optional, for advanced use).
    public IRequest PlaywrightRequest { get; set; }

    // Constructor initializes the tracked request from a Playwright IRequest.
    public TrackedRequest(IRequest request)
    {
        PlaywrightRequest = request;
        Url = request.Url;
        Method = request.Method;
        PostData = request.PostData;
        StatusCode = 0;
        Status = RequestStatus.Pending;
        StartTime = DateTimeOffset.Now;
        EndTime = DateTimeOffset.MinValue;
        ResponseBody = null;
        Id = Uid64.CreateNewUid();
    }

    // Marks the request as finished with status code and optional response body.
    public void MarkFinished(IResponse response, string responseBody)
    {
        StatusCode = response.Status;
        Status = RequestStatus.Finished;
        EndTime = DateTimeOffset.Now;
        ResponseBody = responseBody;
    }

    // Marks the request as failed.
    public void MarkFailed()
    {
        Status = RequestStatus.Failed;
        EndTime = DateTimeOffset.Now;
    }
}