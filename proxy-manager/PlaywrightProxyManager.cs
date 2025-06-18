using FileLib;
using LogUtility;
using PlaywrightProxyConfig;

namespace PlaywrightProxyManager;

/// <summary>
/// Manages proxies.
/// </summary>
public class PlaywrightProxyManager
{
    // fields
    private List<PlaywrightProxyInstance> _proxies;
    private int _activeProxyIndex = 0;
    private FilePath _proxiesPath;
    public bool UseLocalHost = false;
    //=====================================================================================================
    // constructor
    public PlaywrightProxyManager(PlaywrightProxyAndWorkerConfig config)
    {
        _proxiesPath = config.ProxiesPath;
        _proxies = new List<PlaywrightProxyInstance>();
    }
    // methods
    //=====================================================================================================
    // store proxy instance only if it doesn't exist yet
    private void StoreProxy(PlaywrightProxyInstance proxy)
    {
        bool exists = _proxies.Contains(proxy);
        if (!exists) _proxies.Add(proxy);
    }
    //=====================================================================================================
    // randomly select proxy from list
    public PlaywrightProxyInstance NextProxy()
    {
        if (_proxies.Count == 0)
        {
            LibLog.LogWarning(
                $@"Proxy list is empty. Need to insert a list of proxies into 'kcg-proxies.txt' with the below format:
                    {Environment.NewLine}ip address:port:username:password
                    {Environment.NewLine}eg.
                    {Environment.NewLine}178.101.46.13:5000:user1:lkjerpofgejs");

            return null;
        }
        else if (_proxies.Count == 1)
        {
            PlaywrightProxyInstance proxyInstance = _proxies[0];

            if (!PlaywrightProxyChecker.IsProxyWorking(proxyInstance.IP, proxyInstance.Port, proxyInstance.Username,
                    proxyInstance.Password))
            {
                LibLog.LogWarning($@"Proxy with IP {proxyInstance.IP} doesn't seem to be working");
                return null;
            }

            _activeProxyIndex = 0;
            _proxies[_activeProxyIndex].Status = PlaywrightProxyInstance.ProxyStatus.NOT_AVAILABLE;
            return _proxies[_activeProxyIndex];
        }

        for (int n = 0; n < _proxies.Count; n++)
        {
            if (_proxies[n].Status == PlaywrightProxyInstance.ProxyStatus.AVAILABLE)
            {

                if (!PlaywrightProxyChecker.IsProxyWorking(_proxies[n].IP, _proxies[n].Port, _proxies[n].Username,
                        _proxies[n].Password))
                {
                    LibLog.LogWarning($@"Proxy with IP {_proxies[n].IP} doesn't seem to be working");
                    continue;
                }

                _proxies[n].Status = PlaywrightProxyInstance.ProxyStatus.NOT_AVAILABLE;
                _activeProxyIndex = n;
                return _proxies[n];
            }
        }

        Init();
        _activeProxyIndex = 0;
        _proxies[_activeProxyIndex].Status = PlaywrightProxyInstance.ProxyStatus.NOT_AVAILABLE;
        LibLog.LogInfo($"Using proxy {_proxies[_activeProxyIndex].IP}.");
        return _proxies[_activeProxyIndex];
    }
    //=====================================================================================================
    // initialize proxy repo with proxies loaded from text file
    public bool Init()
    {
        try
        {
            PlaywrightProxyInstance proxyInstance;
            string[] tokens;
            string username;
            string password;
            string ip;
            int port;
            string fileContent = _proxiesPath.ReadAllText();

            if (fileContent == null)
            {
                LibLog.LogError($"Could not read file from {_proxiesPath}");
                return false;
            }

            string[] lines = fileContent.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

            _proxies.Clear();

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];

                if (string.IsNullOrEmpty(line))
                {
                    break;
                }

                tokens = line.Split(":");

                ip = tokens[0];
                port = Convert.ToInt32(tokens[1]);
                username = tokens[2];
                password = tokens[3];

                proxyInstance = new PlaywrightProxyInstance(ip, port, username, password);
                proxyInstance.Status = PlaywrightProxyInstance.ProxyStatus.AVAILABLE;
                StoreProxy(proxyInstance);
            }

            if (_proxies.Count > 0)
            {
                Shuffle(_proxies);
                return true;
            }
            else
            {
                return false;
            }
        }
        catch (Exception ex)
        {
            LibLog.LogError(ex.Message);
            return false;
        }
    }

    void Shuffle(List<PlaywrightProxyInstance> list)
    {
        Random random = new();
        int listIndex = list.Count;
        PlaywrightProxyInstance proxyInstance;

        while (listIndex > 1)
        {
            listIndex--;
            int randomIndex = random.Next(listIndex + 1);

            // Swap the elements
            proxyInstance = list[randomIndex];
            list[randomIndex] = list[listIndex];
            list[listIndex] = proxyInstance;
        }
    }
}
