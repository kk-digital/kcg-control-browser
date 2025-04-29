using System.Data;
using System.Security.Cryptography.X509Certificates;
using LogUtility;
using Microsoft.Playwright;
using PlaywrightProxyManager;
using Titanium.Web.Proxy;

namespace PlaywrightController;
public class PlaywrightManager
{
    //=====================================================================================================
    private IBrowser _browser = null;
    private IBrowserContext _context = null;
    private PlaywrightClient.PlaywrightClient _playwrightClient;
    //=====================================================================================================
    // constructor
    public PlaywrightManager(PlaywrightClient.PlaywrightClient client)
    {
        _context = client.Context;
        _playwrightClient = client;
        _browser = client.Browser;
    }
    //=====================================================================================================
    public async Task ConnectToBrowser()
    {
        // create a new browser context
        _context = await _browser.NewContextAsync();
    }
    //=====================================================================================================
    public bool TestConnection()
    {
        // Check if the browser context is connected
        return _context.Browser != null;
    }
    //=====================================================================================================
    public async Task<IBrowserContext> GetClientInstance()
    {
        try
        {
            return _context;
        }
        catch (Exception ex)
        {
            LibLog.LogError(ex.Message);
            return null;
        }
    }
    //=====================================================================================================
    public void SendCommand(PlaywrightClient.PlaywrightClient.Command_Type command, IPage page)
    {
        _playwrightClient.ExecuteCommand(command, _context, page);
    }
    //=====================================================================================================
    public void SetBrowser(PlaywrightClient.PlaywrightClient client)
    {
        _playwrightClient = client;
        _browser = client.Browser;
    }
    //=====================================================================================================
    PlaywrightProxyInstance GetBrowserProxy()
    {
        return _playwrightClient.Proxy;
    }
    //=====================================================================================================
    // use custom certificate
    public bool SetSSLCertificates(ProxyServer proxyServer, string pfxPath, string pwd)
    {
        try
        {
            proxyServer.CertificateManager.LoadRootCertificate(pfxPath, pwd, false);
            return true;
        }
        catch (Exception ex)
        {
            LibLog.LogError(ex.Message);
            return false;
        }
    }
    //=====================================================================================================
    static bool IsRootCertificateInstalled(string subjectName)
    {
        using (X509Store store = new(StoreName.Root, StoreLocation.LocalMachine))
        {
            store.Open(OpenFlags.ReadOnly);

            // Search for certificates matching the given subject name
            bool certificateExists = store.Certificates
                .Cast<X509Certificate2>()
                .Any(cert => cert.Subject.Contains(subjectName, StringComparison.OrdinalIgnoreCase));

            return certificateExists;
        }
    }
    //=====================================================================================================
}

