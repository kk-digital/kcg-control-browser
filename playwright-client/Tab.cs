using Microsoft.Playwright;

namespace PlaywrightClient;

public class Tab
{
    // fields
    static int _nextTabID = 0;
    private IBrowserContext _context;
    //===================================================================================================================
    // properties
    public int Id { get; set; }
    public string Url { get; set; }
    public IPage Page { get; set; }
    public HashSet<IRequest> PendingRequest { get; set; }
    public HashSet<IRequest> AllRequest { get; set; }
    public bool IsLoading { get; set; } = false;
    //===================================================================================================================
    // constructor
    public Tab(IBrowserContext context)
    {
        _context = context;
        PendingRequest = new HashSet<IRequest>();
        AllRequest = new HashSet<IRequest>();
        _nextTabID++;
        Id = _nextTabID;
        //InitializePage();
    }
    //===================================================================================================================
    // reset tab Id generator back to 0
    public void ResetInitialTabId()
    {
        _nextTabID = 0;
    }
    //===================================================================================================================
    async void InitializePage()
    {
        // Open a new tab (page) in the browser
        Page = await _context.NewPageAsync();

        // subscribe to Request and Response events
        Page.Request += OnRequest;
        Page.Response += OnResponse;

        IsLoading = true;
        // Event listener to detect when the page finishes loading
        Page.Load += (_, _) => IsLoading = false; //Page has finished loading
    }
    //===================================================================================================================
    // handles requests
    void OnRequest(object sender, IRequest request)
    {
        lock (PendingRequest)
        {
            PendingRequest.Add(request); // update list of active requests
        }

        lock (AllRequest)
        {
            AllRequest.Add(request); // update list of all requests
        }
    }
    //===================================================================================================================
    // handles response
    void OnResponse(object sender, IResponse response)
    {
        lock (PendingRequest)
        {
            PendingRequest.Remove(response.Request);
        }
    }
    //===================================================================================================================
}