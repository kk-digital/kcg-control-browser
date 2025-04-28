namespace utility;

// stores progress or result stats of any scraping task
public class ScrapingResultStat
{
    private string _scrapingErrorMessage = string.Empty;
    public int ScrapedItemsCount = 0;
    public int ScrapingErrorsCount = 0;

    public string ScrapingErrorMessage
    {
        get
        {
            return _scrapingErrorMessage;
        }
        set
        {
            _scrapingErrorMessage = value;
            
            if (string.IsNullOrEmpty(value))
            {
                ScrapingErrorsCount = 0;
            }
            else
            {
                ScrapingErrorsCount = 1;
            }
        }
    }

    public void Reset()
    {
        ScrapedItemsCount = 0;
        ScrapingErrorsCount = 0;
        ScrapingErrorMessage = string.Empty;
    }
}

