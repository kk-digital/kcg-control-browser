using Titanium.Web.Proxy.EventArguments;

namespace Utility;

public class LogFileWriter
{
    // fields
    private string _LogfilePath;
    private string _LogOperationStatus;

    // constructor
    public LogFileWriter(string logFilePath)
    {
        _LogfilePath = logFilePath;
    }
    public string GetLogOperationStatus()
    {
        return _LogOperationStatus;
    }
    public async Task LogRequest(SessionEventArgs e, Guid id)
    {
        try
        {
            _LogOperationStatus = "ok";
            string logEntry = $"[{DateTime.Now}]  <urn:Uid:{id}> Request: {e.HttpClient.Request.Method} {e.HttpClient.Request.Url}";
            FileLib.FileUtils.AppendText(_LogfilePath, logEntry + Environment.NewLine);
        }
        catch (Exception ex)
        {
            _LogOperationStatus = ex.Message.ToString();
        }
    }
    public async Task LogResponse(SessionEventArgs e, Guid id)
    {
        try
        {
            _LogOperationStatus = "ok";
            string logEntry = $"[{DateTime.Now}] <urn:Uid:{id}> Response: {e.HttpClient.Response.StatusCode} {e.HttpClient.Request.Url} <Hash: {e.HttpClient.Response.GetHashCode().ToString()}>";
            FileLib.FileUtils.AppendText(_LogfilePath, logEntry + Environment.NewLine);
        }
        catch (Exception ex)
        {
            _LogOperationStatus = ex.Message.ToString();
        }
    }
}
