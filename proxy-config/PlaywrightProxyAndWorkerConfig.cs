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
    [JsonConverter(typeof(FilePathJsonConverter))]
    public FilePath ProxiesPath;
    
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
        Utils.Assert(config.DownloadFullResolutionImage != null, "DownloadFullResolutionImage must be set to true/false.");

        config.ChromiumDirPath = PathUtils.GetFullPath(config.ChromiumDirPath);
        switch (OperatingSystemUtils.GetOperatingSystem())
        {
            case OperatingSystemType.Windows:
            {
                ChromiumFilePath = new(PathUtils.Combine(config.ChromiumDirPath, "chrome.exe"));
                break;
            }
            case OperatingSystemType.Linux:
            default:
            {
                ChromiumFilePath = new(PathUtils.Combine(config.ChromiumDirPath, "chrome"));
                break;
            }
        }

        Utils.Assert(ChromiumFilePath != null, "Chromium file could not be set properly.");
        
        OrchestrationUrl = config.OrchestrationUrl;
        RetryDelaySeconds = config.RetryDelaySeconds;
        Headless = config.Headless;
        ProxiesPath = config.ProxiesPath;
        DownloadFullResolutionImage = config.DownloadFullResolutionImage;
    }
}