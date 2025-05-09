using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using assert;
using FileLib;
using libJson;
using OperatingSystem;
using OperatingSystemEnums;
using UtilityIO;

namespace PlaywrightProxyConfig;

// stores config data for worker and playwright proxy manager
public class PlaywrightProxyAndWorkerConfig
{
    [JsonInclude]
    [Required]
    public string OrchestrationUrl;

    [JsonInclude]
    [Required]
    public string ChromiumDirPath;

    public FilePath ChromiumFilePath;

    [JsonInclude]
    [Required]
    public int RetryDelaySeconds;

    [JsonInclude]
    [Required]
    public bool Headless;
    
    [JsonInclude]
    [Required]
    public bool UseProxy;

    [JsonInclude]
    [Required]
    [JsonConverter(typeof(FilePathJsonConverter))]
    public FilePath ProxiesPath;

    [JsonInclude]
    [Required]
    [JsonConverter(typeof(FilePathJsonConverter))]
    public FilePath GeoLocationDataPath;    // path to a .json file where proxies geo location data is stored

    [JsonInclude]
    [Required]
    [JsonConverter(typeof(FilePathJsonConverter))]
    public FilePath UserAgentsPath;         // path to a .txt file where browser user agents data is stored

    [JsonInclude]
    [Required]
    public string UserProfilesDirPath;      // path to a folder where browser context user profiles are stored

    [JsonInclude]
    [Required]
    [JsonConverter(typeof(FilePathJsonConverter))]
    public FilePath MostVisitedSitesPath;         // path to a .txt file containing a list of most visited sites in the U.S.

    [JsonInclude]
    [Required]
    public bool StoreCookies;

    [JsonInclude]
    [Required]
    public bool DownloadFullResolutionImage;

    public PlaywrightProxyAndWorkerConfig()
    {
        OrchestrationUrl = null;
        ChromiumDirPath = null;
        ChromiumFilePath = null;
        RetryDelaySeconds = 5; //default to 5 seconds sleep duration when not finding any pending scraping task
        Headless = false;
        ProxiesPath = null;
        DownloadFullResolutionImage = false;
        UserProfilesDirPath = null;
        GeoLocationDataPath = null;
        UserAgentsPath = null;
        MostVisitedSitesPath = null;
        StoreCookies = false;
        UseProxy = true;
    }

    public PlaywrightProxyAndWorkerConfig(FilePath filePath)
    {
        string result = filePath.ReadAllText();

        PlaywrightProxyAndWorkerConfig config = Json.Deserialize<PlaywrightProxyAndWorkerConfig>(result);

        Utils.Assert(config != null, "Configuration found has been improperly read.");
        Utils.Assert(config.OrchestrationUrl != null, "Orchestration URL not found in configuration.");
        Utils.Assert(config.RetryDelaySeconds >= 1, "Worker sleep duration must be at least 1 second.");
        Utils.Assert(config.ProxiesPath != null, "Proxies file path not found in configuration.");
        Utils.Assert(config.ChromiumDirPath != null, "Chromium directory path not found in configuration.");
        Utils.Assert(config.UserProfilesDirPath != null, "User Profiles directory path not found in configuration.");
        Utils.Assert(config.GeoLocationDataPath != null, "Geo Location Data file path not found in configuration.");
        Utils.Assert(config.UserAgentsPath != null, "User Agents file path not found in configuration.");
        Utils.Assert(config.MostVisitedSitesPath != null, "Most Visited Sites' file path not found in configuration.");

        ChromiumDirPath = config.ChromiumDirPath;

        switch (OperatingSystemUtils.GetOperatingSystem())
        {
            case OperatingSystemType.Windows:
                {
                    ChromiumFilePath = new(PathUtils.GetFullPath(PathUtils.Combine(ChromiumDirPath, "chrome.exe")));
                    break;
                }
            case OperatingSystemType.Linux:
            default:
                {
                    ChromiumFilePath = new(PathUtils.GetFullPath(PathUtils.Combine(config.ChromiumDirPath, "chrome")));
                    break;
                }
        }

        Utils.Assert(ChromiumFilePath != null, "Chromium file could not be set properly.");

        OrchestrationUrl = config.OrchestrationUrl;
        RetryDelaySeconds = config.RetryDelaySeconds;
        Headless = config.Headless;
        ProxiesPath = config.ProxiesPath;
        DownloadFullResolutionImage = config.DownloadFullResolutionImage;
        UserProfilesDirPath = config.UserProfilesDirPath;
        GeoLocationDataPath = config.GeoLocationDataPath;
        UserAgentsPath = config.UserAgentsPath;
        MostVisitedSitesPath = config.MostVisitedSitesPath;
        StoreCookies = config.StoreCookies;
        UseProxy = config.UseProxy;
    }
}