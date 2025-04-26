using HtmlAgilityPack;
using Microsoft.Playwright;
using System.Collections.Concurrent;
using System.Text;
using LogUtility;
using Utility;

namespace PlaywrightClient;

// store the page DOM
public class DOMStorage
{
    private ConcurrentQueue<HtmlDocument> _pinPages;
    private ConcurrentQueue<HtmlDocument> _pinSearchResultPages;
    private ConcurrentQueue<HtmlDocument> _boardSearchResultPages;
    private ConcurrentQueue<HtmlDocument> _youtubeChannelVideos;
    private ConcurrentQueue<HtmlDocument> _youtubeChannels;
    private ConcurrentQueue<StringBuilder> _warc;

    private string _pinPageHash;
    private string _pinSearchResultPageHash;
    private string _boardSearchResultPageHash;
    private string _youtubeChannelVideoHash;
    private string _youtubeChannelHash;
    private string _warcHash;

    private IPage _page;
    //=================================================================================
    public enum DOMtype
    {
        PIN_SEARCH_RESULTS_PAGE,
        PIN_PAGE,
        BOARD_SEARCH_RESULTS_PAGE,
        YOUTUBE_CHANNELS,
        YOUTUBE_CHANNEL_VIDEOS
    }
    //=================================================================================
    // constructor
    public DOMStorage(IPage page)
    {
        _page = page;
        _pinPages = new ConcurrentQueue<HtmlDocument>();
        _pinSearchResultPages = new ConcurrentQueue<HtmlDocument>();
        _boardSearchResultPages = new ConcurrentQueue<HtmlDocument>();
        _youtubeChannelVideos = new ConcurrentQueue<HtmlDocument>();
        _youtubeChannels = new ConcurrentQueue<HtmlDocument>();
        _warc = new ConcurrentQueue<StringBuilder>();

        _pinPageHash = "";
        _pinSearchResultPageHash = "";
        _boardSearchResultPageHash = "";
        _youtubeChannelVideoHash = "";
        _youtubeChannelHash = "";
        _warcHash = "";
    }
    //=================================================================================
    public IPage GetPage()
    {
        return _page;
    }
    //=================================================================================
    public HtmlDocument GetDOM(DOMtype domType)
    {
        try
        {
            switch (domType)
            {
                case DOMtype.PIN_PAGE:
                    _pinPages.TryDequeue(out HtmlDocument doc);
                    return doc;

                case DOMtype.PIN_SEARCH_RESULTS_PAGE:
                    _pinSearchResultPages.TryDequeue(out HtmlDocument doc2);
                    return doc2;

                case DOMtype.BOARD_SEARCH_RESULTS_PAGE:
                    _boardSearchResultPages.TryDequeue(out HtmlDocument doc4);
                    return doc4;

                case DOMtype.YOUTUBE_CHANNELS:
                    _youtubeChannels.TryDequeue(out HtmlDocument doc3);
                    return doc3;

                case DOMtype.YOUTUBE_CHANNEL_VIDEOS:
                    _youtubeChannelVideos.TryDequeue(out HtmlDocument doc5);
                    return doc5;
            }
            return null;
        }
        catch (Exception ex)
        {
            LibLog.LogError(ex.Message);
            return null;
        }
    }
    //=================================================================================
    public bool StoreDOM(DOMtype domType, IPage page = null)
    {
        string hash = "";
        bool success = false;

        try
        {
            string content;

            if (page != null) content = page.ContentAsync().GetAwaiter().GetResult();
            else content = _page.ContentAsync().GetAwaiter().GetResult();

            HtmlDocument doc = new ();

            doc.LoadHtml(content);
            content = "";
            string s = ExtractorHelper.NormalizeHtml(ref doc);
            hash = ExtractorHelper.ComputeMD5(s);
            s = "";

            switch (domType)
            {
                case DOMtype.PIN_PAGE:
                    if (hash != _pinPageHash)
                    {
                        _pinPageHash = hash;
                        _pinPages.Enqueue(doc);
                        success = true;
                        break;
                    }

                    success = false;
                    break;

                case DOMtype.PIN_SEARCH_RESULTS_PAGE:

                    if (hash != _pinSearchResultPageHash)
                    {
                        _pinSearchResultPageHash = hash;
                        _pinSearchResultPages.Enqueue(doc);
                        success = true;
                        break;
                    }

                    success = false;
                    break;

                case DOMtype.BOARD_SEARCH_RESULTS_PAGE:

                    if (hash != _boardSearchResultPageHash)
                    {
                        _boardSearchResultPageHash = hash;
                        _boardSearchResultPages.Enqueue(doc);
                        success = true;
                        break;
                    }

                    success = false;
                    break;

                case DOMtype.YOUTUBE_CHANNELS:

                    if (hash != _youtubeChannelHash)
                    {
                        _youtubeChannelHash = hash;
                        _youtubeChannels.Enqueue(doc);
                        success = true;
                        break;
                    }

                    success = false;
                    break;

                case DOMtype.YOUTUBE_CHANNEL_VIDEOS:

                    if (hash != _youtubeChannelVideoHash)
                    {
                        _youtubeChannelVideoHash = hash;
                        _youtubeChannelVideos.Enqueue(doc);
                        success = true;
                        break;
                    }

                    success = false;
                    break;
            }

            return success;
        }
        catch (Exception ex)
        {
            LibLog.LogError(ex.Message);
            return false;
        }
    }
    //=================================================================================
    public void storeWARC(StringBuilder warc)
    {
        try
        {
            _warc.Enqueue(warc);
        }
        catch (Exception ex)
        {
            LibLog.LogError(ex.Message);
        }
    }
    //=================================================================================
    public StringBuilder GetWARC()
    {
        try
        {
            _warc.TryDequeue(out StringBuilder warc);
            return warc;
        }
        catch (Exception ex)
        {
            LibLog.LogError(ex.Message);
            return null;
        }
    }
    //=================================================================================
    public static void SaveToFile(string domContent, string filePath)
    {
        byte[] byteArray = Encoding.UTF8.GetBytes(domContent);
        FileLib.FileUtils.WriteAllBytes(filePath, byteArray);
    }
    //=================================================================================
}