namespace PlaywrightProxyManager;

// stores proxy data

public class PlaywrightProxyInstance
{
    //========================================================================================
    public string IP { get; set; }
    public int Port { get; set; }
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public ProxyStatus Status { get; set; }
    //========================================================================================
    public enum ProxyStatus
    {
        AVAILABLE,
        NOT_AVAILABLE
    }
    //========================================================================================
    public PlaywrightProxyInstance(string ip, int port, string username, string password = "")
    {
        IP = ip;
        Port = port;
        Username = username;
        Password = password;
    }
    //========================================================================================
}
