using System.Text;
using Titanium.Web.Proxy.EventArguments;
using Titanium.Web.Proxy.Http;
using Titanium.Web.Proxy.Models;

namespace UtilityWarcTools;

public class WARCfileWriter
{
    // fields
    private string _WARCfilePath;
    private string _WARCoperationStatus;

    // constructor
    public WARCfileWriter(string WARCfilePath)
    {
        _WARCfilePath = WARCfilePath;
        InitializeWarcFile();
    }

    // methods
    public string GetWARCoperationStatus()
    {
        return _WARCoperationStatus;
    }

    private void InitializeWarcFile()
    {
        try
        {
            _WARCoperationStatus = "ok";

            StringBuilder warcInfoRecord = new ();
            string now = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
            string warcRecordId = $"<urn:Uid:{Guid.NewGuid()}>";

            warcInfoRecord.AppendLine("WARC/1.0");
            warcInfoRecord.AppendLine($"WARC-Type: warcinfo");
            warcInfoRecord.AppendLine($"WARC-Date: {now}");
            warcInfoRecord.AppendLine($"WARC-Record-ID: {warcRecordId}");
            warcInfoRecord.AppendLine("Content-Type: application/warc-fields");
            warcInfoRecord.AppendLine("Content-Length: 0");
            warcInfoRecord.AppendLine();
            warcInfoRecord.AppendLine();

            FileLib.FileUtils.WriteAllText(_WARCfilePath, warcInfoRecord.ToString());
        }
        catch (Exception ex)
        {
            _WARCoperationStatus = ex.Message.ToString();
        }
    }
    // Method to combine two byte arrays
    private static byte[] Combine(byte[] first, byte[] second)
    {
        if (first == null) first = new byte[0];
        if (second == null) second = new byte[0];

        byte[] result = new byte[first.Length + second.Length];
        Buffer.BlockCopy(first, 0, result, 0, first.Length);
        Buffer.BlockCopy(second, 0, result, first.Length, second.Length);
        return result;
    }
    // Write a WARC request record
    public async Task WriteWARCrequestRecord(SessionEventArgs e, Guid id)
    {
        try
        {
            _WARCoperationStatus = "ok";

            StringBuilder warcRecord = new ();
            string now = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
            string warcRecordId = $"<urn:Uid:{id}>";

            // Prepare the request headers
            string requestHeaders = $"{e.HttpClient.Request.Method} {e.HttpClient.Request.RequestUri.PathAndQuery} {e.HttpClient.Request.HttpVersion}{Environment.NewLine}";
            
            List<HttpHeader> headersList = e.HttpClient.Request.Headers.ToList(); // Convert to List if it's not already a List.
            HttpHeader header;
            
            for (int i = 0; i < headersList.Count; i++)
            {
                header = headersList[i];
                requestHeaders += $"{header.Name}: {header.Value}{Environment.NewLine}";
            }

            requestHeaders += Environment.NewLine;

            // Read the request body
            string body = await e.GetRequestBodyAsString();

            // Combine headers and body
            byte[] payload = Combine(Encoding.UTF8.GetBytes(requestHeaders), Encoding.UTF8.GetBytes(body));

            // Build the WARC record
            warcRecord.AppendLine("WARC/1.0");
            warcRecord.AppendLine("WARC-Type: request");
            warcRecord.AppendLine($"WARC-Date: {now}");
            warcRecord.AppendLine($"WARC-Record-ID: {warcRecordId}");
            warcRecord.AppendLine($"WARC-Target-URI: {e.HttpClient.Request.Url}");
            warcRecord.AppendLine("Content-Type: application/http; msgtype=request");
            warcRecord.AppendLine($"Content-Length: {payload.Length}");
            warcRecord.AppendLine();

            // Append the request payload (headers + body)
            byte[] headerBytes = Encoding.UTF8.GetBytes(warcRecord.ToString());
            byte[] newline = Encoding.UTF8.GetBytes(Environment.NewLine + Environment.NewLine);
            payload = Combine(headerBytes, Combine(payload, newline));
            FileLib.FileUtils.AppendText(_WARCfilePath, Encoding.UTF8.GetString(payload));
        }
        catch (Exception ex)
        {
            _WARCoperationStatus = ex.Message.ToString();
        }
    }
    // Write a WARC response record
    public async Task WriteWARCresponseRecord(Request request, SessionEventArgs e, Guid id)
    {
        try
        {
            _WARCoperationStatus = "ok";

            StringBuilder warcRecord = new ();
            string now = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
            string warcRecordId = $"<urn:Uid:{id}>";

            string statusLine = $"{e.HttpClient.Response.HttpVersion} {e.HttpClient.Response.StatusCode} {e.HttpClient.Response.StatusDescription}{Environment.NewLine}";
            string responseHeaders = statusLine;
            
            List<HttpHeader> responseHeadersList = e.HttpClient.Response.Headers.ToList(); // Convert to List if it's not already a List.
            HttpHeader header;
            
            for (int i = 0; i < responseHeadersList.Count; i++)
            {
                header = responseHeadersList[i];
                responseHeaders += $"{header.Name}: {header.Value}{Environment.NewLine}";
            }

            responseHeaders += Environment.NewLine;

            string body = "";

            try
            {
                if (e.HttpClient.Response.HasBody)
                    body = await e.GetResponseBodyAsString();
                else
                    return;
            }
            catch (Exception ex)
            {
                _WARCoperationStatus = ex.Message.ToString();
                return;
            }

            // Combine headers and body
            byte[] payload = Combine(Encoding.UTF8.GetBytes(responseHeaders), Encoding.UTF8.GetBytes(body));

            // Build the WARC record
            warcRecord.AppendLine("WARC/1.0");
            warcRecord.AppendLine("WARC-Type: response");
            warcRecord.AppendLine($"WARC-Date: {now}");
            warcRecord.AppendLine($"WARC-Record-ID: {warcRecordId}");
            warcRecord.AppendLine($"WARC-Target-URI: {request.Url}");
            warcRecord.AppendLine("Content-Type: application/http; msgtype=response");
            warcRecord.AppendLine($"Content-Length: {payload.Length}");
            warcRecord.AppendLine();

            // Append the response payload (headers + body)
            byte[] headerBytes = Encoding.UTF8.GetBytes(warcRecord.ToString());
            byte[] newline = Encoding.UTF8.GetBytes(Environment.NewLine + Environment.NewLine);
            payload = Combine(headerBytes, Combine(payload, newline));
            FileLib.FileUtils.AppendText(_WARCfilePath, Encoding.UTF8.GetString(payload));
        }
        catch (Exception ex)
        {
            _WARCoperationStatus = ex.Message.ToString();
        }
    }
}

