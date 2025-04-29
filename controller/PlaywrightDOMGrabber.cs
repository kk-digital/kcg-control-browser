using System.Text;
using LogUtility;
using Microsoft.Playwright;
using playwright_web_proxy_server;

namespace PlaywrightController;

// use for DOM related operations
public class PlaywrightDOMGrabber
{
    //========================================================================================================================================
    // grab DOM and return as string
    public static async Task<string> GrabDOMasString(ServerRunner sr)
    {
        byte[] bytDOM = await GrabDOMasBytes(sr);

        if (bytDOM != null) return Encoding.UTF8.GetString(bytDOM);
        else return "";
    }
    //========================================================================================================================================
    // grab DOM and return as byte array
    public static async Task<byte[]> GrabDOMasBytes(ServerRunner sr)
    {
        if (sr != null)
        { // make sure proxy server is running
            IPage pg = sr.GetPage(); // get a reference to the browser page on which to simulate the mouse scroll action

            // only execute mouse scroll if the browser is open
            if (pg == null)
            {
                Console.WriteLine("Make sure the browser is open. Press any key to continue");
                Console.ReadKey();
                return null;
            }
            else
            {
                try
                {
                    // Wait for any possible DOM changes (optional depending on the site)
                    await pg.WaitForTimeoutAsync(3000); // Adjust timeout based on expected changes

                    // Grab the entire DOM of the page 
                    string domContent = await pg.ContentAsync();
                    return Encoding.UTF8.GetBytes(domContent); //return DOM as byte array
                }
                catch (Exception ex)
                {
                    LibLog.LogError(ex.Message);
                    Console.WriteLine("Press any key to continue");
                    Console.ReadKey();
                    return null;
                }
            }
        }
        else
        {
            Console.WriteLine("Make sure the browser is open. Press any key to continue");
            Console.ReadKey(true);
            return null;
        }
    }
    //========================================================================================================================================
    // grab DOM and save to file
    public static async Task<bool> GrabDOMToFile(ServerRunner sr, string sFilePath)
    {
        try
        {
            byte[] dom = await GrabDOMasBytes(sr);
            FileLib.FileUtils.WriteAllText(sFilePath, Encoding.UTF8.GetString(dom));
            return true;
        }
        catch (Exception ex)
        {
            LibLog.LogError(ex.Message);
            Console.ReadKey(true);
            return false;
        }

    }
    //========================================================================================================================================
    public static async Task<bool> GrabDOMstringToFile(ServerRunner sr, string sFilePath)
    {
        try
        {
            byte[] dom = await GrabDOMasBytes(sr);
            FileLib.FileUtils.WriteAllText(sFilePath, BitConverter.ToString(dom));
            return true;
        }
        catch (Exception ex)
        {
            LibLog.LogError(ex.Message);
            Console.ReadKey(true);
            return false;
        }

    }
    //========================================================================================================================================
}
