using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using ImageMagick;
using UtilityHttpServer;

namespace Utility;

public class ExtractorHelper
{
    // Static MD5 instance for reusability
    private static readonly MD5 _md5 = MD5.Create();
    private static uint _downloadedImageCount;

    public static uint DownloadedImageCount
    {
        get => Interlocked.CompareExchange(ref _downloadedImageCount, 0, 0); // Safely reads the value
        set => Interlocked.Exchange(ref _downloadedImageCount, value); // Safely sets the value
    }

    // For incrementing:
    public static void IncrementDownloadedImageCount()
    {
        Interlocked.Increment(ref _downloadedImageCount);
    }

    public static string CleanText(string input)
    {
        string output = input.Trim();

        if (string.IsNullOrWhiteSpace(output)) return output;

        output = Regex.Unescape(output);

        output = Regex.Replace(output, @"\\u0026amp;|&amp;", "&");
        output = Regex.Replace(output, @"\u0026quot;", @"""");
        output = Regex.Replace(output, @"\u0026gt;", ">");
        output = Regex.Replace(output, @"\u0026amp;", @"&");
        output = Regex.Replace(output, @"\u2B50", "*");
        output = Regex.Replace(output, @"[^\u0020-\u007E]", "");
        output = Regex.Replace(output, @"\uD83D\uDD34", ":)");
        output = Regex.Replace(output, @"\s+", " ");
        output = Regex.Replace(output, @",\s*,", ",");
        output = Regex.Replace(output, @"\| \|", "");
        output = Regex.Replace(output, "\\u0022", "");
        output = Regex.Replace(output, "\\u0026", "");
        output = Regex.Replace(output, "\\u0027", "");

        output = NetworkClient.HtmlDecode(output);

        return output;
    }

    // use for pinterest thumbnail url
    public static string ExtractFilenameFromUrl(string url)
    {
        try
        {
            // Create a Uri object from the URL
            Uri uri = new(url);

            // Get the last part of the URL (the filename)
            string filename = Path.GetFileName(uri.LocalPath);

            filename = MakeFileNameValid(filename);
            return filename;
        }
        catch (Exception)
        {
            return "";
        }
    }

    public static string MakeFileNameValid(string fileName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return fileName;
            }

            string ret = fileName;

            // Ensure it's a valid filename for the file system
            char[] invalidChars = Path.GetInvalidFileNameChars();
            for (int i = 0; i < invalidChars.Length; i++)
            {
                ret = ret.Replace(invalidChars[i], '_'); // Replace invalid characters with '_'
            }


            return ret;
        }
        catch (Exception)
        {
            return fileName;
        }
    }

    public static string NormalizeHtml(ref HtmlDocument doc)
    {
        // Iterate over a copy of the node collection to avoid modifying it during traversal
        List<HtmlNode> nodes = doc.DocumentNode.Descendants().ToList(); // Materialize the list once for better control

        for (int nodeIndex = nodes.Count - 1; nodeIndex >= 0; nodeIndex--) // Reverse iteration for safe removal
        {
            HtmlNode node = nodes[nodeIndex];

            if (node.NodeType == HtmlNodeType.Text && string.IsNullOrWhiteSpace(node.InnerHtml))
            {
                node.Remove();
            }
        }

        // Return the normalized HTML
        return doc.DocumentNode.OuterHtml.Trim();
    }

    public static string ConvertToHex(byte[] hashBytes)
    {
        // Optimize hex conversion by avoiding StringBuilder
        char[] result = new char[hashBytes.Length * 2];

        for (int hashBytesIndex = 0; hashBytesIndex < hashBytes.Length; hashBytesIndex++)
        {
            byte b = hashBytes[hashBytesIndex];
            result[hashBytesIndex * 2] = GetHexChar(b >> 4);
            result[hashBytesIndex * 2 + 1] = GetHexChar(b & 0xF);
        }

        return new string(result);
    }

    private static char GetHexChar(int value)
    {
        return (char)(value < 10 ? '0' + value : 'a' + (value - 10));
    }

    public static string ComputeMD5(string input)
    {
        // Use UTF-8 encoder for consistent encoding
        byte[] inputBytes = Encoding.UTF8.GetBytes(input);

        // Compute the hash
        byte[] hashBytes;

        lock (_md5) // MD5 is not thread-safe, so we synchronize it
        {
            hashBytes = _md5.ComputeHash(inputBytes);
        }

        // Convert to a hexadecimal string
        return ConvertToHex(hashBytes);
    }

    public static bool IsValidUrl(string url)
    {
        // Check if the string is null or empty
        if (string.IsNullOrWhiteSpace(url)) return false;

        // Try to create a Uri instance
        return Uri.TryCreate(url, UriKind.Absolute, out Uri uriResult)
               && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }

    public static string PrettyPrintHtml(string html)
    {
        HtmlDocument document = new();
        document.LoadHtml(html);

        // Configure the output to indent the HTML
        document.OptionOutputAsXml = false;
        document.OptionWriteEmptyNodes = true;

        // Save the formatted HTML to a string
        using (StringWriter writer = new())
        {
            document.Save(writer);
            return writer.ToString();
        }
    }

    public static string GetMimeType(byte[] imageBytes)
    {
        using (MemoryStream ms = new(imageBytes))
        {
            // Create a MagickImage from the byte array
            using (MagickImage image = new(ms))
            {
                // Get the image format
                MagickFormat format = image.Format;

                // Map the format to MIME type
                switch (format)
                {
                    case MagickFormat.Jpeg:
                        return "image/jpeg";
                    case MagickFormat.Avif:
                        return "image/avif";
                    case MagickFormat.Png:
                        return "image/png";
                    case MagickFormat.Gif:
                        return "image/gif";
                    case MagickFormat.Bmp:
                        return "image/bmp";
                    case MagickFormat.Tiff:
                        return "image/tiff";
                    case MagickFormat.WebP:
                        return "image/webp";
                    case MagickFormat.Heic:
                        return "image/heic";
                    default:
                        return "application/octet-stream"; // Fallback for unknown types
                }
            }
        }
    }

    public static string GetFileExtension(byte[] imageBytes)
    {
        using (MemoryStream ms = new(imageBytes))
        {
            // Create a MagickImage from the byte array
            using (MagickImage image = new(ms))
            {
                // Get the image format
                MagickFormat format = image.Format;

                // Map the format to MIME type
                switch (format)
                {
                    case MagickFormat.Jpeg:
                        return ".jpg";
                    case MagickFormat.Avif:
                        return ".avif";
                    case MagickFormat.Png:
                        return ".png";
                    case MagickFormat.Gif:
                        return ".gif";
                    case MagickFormat.Bmp:
                        return ".bmp";
                    case MagickFormat.Tiff:
                        return ".tiff";
                    case MagickFormat.WebP:
                        return ".webp";
                    case MagickFormat.Heic:
                        return ".heic";
                    default:
                        return ".dat"; // Fallback for unknown types
                }
            }
        }
    }

    public static bool Exiting()
    {
        if (Console.KeyAvailable)
        {
            // Read the key without displaying it in the console
            ConsoleKeyInfo key = Console.ReadKey(intercept: true);

            // Check if the ESC key is pressed
            if (key.Key == ConsoleKey.Escape)
            {
                // Drain the key buffer by reading all available keys
                while (Console.KeyAvailable)
                    Console.ReadKey(intercept: true);
                return true;
            }
        }

        return false;
    }

    public static string GetRandomUserDataFolder()
    {
        string randomFileName = Path.GetRandomFileName();
        string folderName = Path.GetFileNameWithoutExtension(randomFileName);
        string userDataFolder = "data/chromium-user-data";

        return Path.Combine(userDataFolder, folderName);
    }

    public static bool UrlHasValidUserId(string userUrl)
    {
        // eg. https://www.pinterest.com/BasketBCapsHat/
        // eg. https://www.pinterest.com/nba/_created/
        string domainName = "pinterest.com";

        if (string.IsNullOrEmpty(userUrl))
        {
            return false;
        }

        if (!userUrl.Contains(domainName, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        userUrl = userUrl.Substring(userUrl.IndexOf(domainName, StringComparison.OrdinalIgnoreCase) +
                                    domainName.Length);
        // eg. extract this portion /BasketBCapsHat/

        // Ensure the string starts and ends with '/'
        if (userUrl[0] != '/' || userUrl[userUrl.Length - 1] != '/')
        {
            return false;
        }

        // Remove the leading and trailing '/'
        string trimmed = userUrl.Substring(1, userUrl.Length - 2);

        string[] parts = trimmed.Split('/');

        if (parts.Length >= 2 && parts[0].Length > 0 && parts[parts.Length - 1].Equals("_created", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return parts.Length >= 2;
    }

    public static bool UrlHasValidBoardId(string boardUrl)
    {
        // eg. https://www.pinterest.com/NBA_FAN10/the-philadelphia-76ers/
        const string baseUrl = "https://www.pinterest.com/";

        // it must not be null or white space or empty
        if (string.IsNullOrWhiteSpace(boardUrl))
        {
            return false;
        }

        // it must start with "https://www.pinterest.com/"
        if (!boardUrl.StartsWith(baseUrl, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        string path = boardUrl.Substring(baseUrl.Length);

        // Remove trailing slash if it exists
        if (path.EndsWith("/"))
            path = path.Substring(0, path.Length - 1);

        // Split the remaining part by '/'
        string[] parts = path.Split('/');

        // We expect exactly two parts: [username, boardname]
        if (parts.Length < 2)
        {
            return false;
        }

        string username = parts[0];
        string boardname = parts[1];

        // Validate that both username and boardname contain only allowed characters
        if (!IsAlphanumericOrDashUnderscore(username) || !IsAlphanumericOrDashUnderscore(boardname))
        {
            return false;
        }

        return true;
    }

    public static bool UrlHasValidPinId(string pinUrl)
    {
        // eg. https://www.pinterest.com/pin/358388082863085116/
        string domainName = "pinterest.com";

        if (string.IsNullOrEmpty(pinUrl))
        {
            return false;
        }

        if (!pinUrl.Contains(domainName, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        pinUrl = pinUrl.Substring(pinUrl.IndexOf(domainName, StringComparison.OrdinalIgnoreCase) + domainName.Length);
        // eg. extract this portion /pin/358388082863085116/

        if (!pinUrl.StartsWith("/pin/", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        // Ensure the string starts and ends with '/'
        if (pinUrl[0] != '/' || pinUrl[pinUrl.Length - 1] != '/')
        {
            return false;
        }

        // Extract the number part between "/pin/" and the last "/"
        string numberPart = pinUrl.Substring(5, pinUrl.Length - 6); // Remove "/pin/" (5 chars) and last "/"
        // eg. extract this portion 358388082863085116

        return numberPart.All(char.IsDigit);
    }

    public static bool UrlHasValidChannelId(string channelUrl)
    {
        // https://www.youtube.com/@talkSPORTBoxing
        // https://www.youtube.com/BTSportBoxing
        // https://www.youtube.com/channel/UCkDYIeGnavKtlGDuC0v4iyw
        // https://www.youtube.com/user/PewDiePie

        const string baseUrl = "https://www.youtube.com/";

        // it must not be null or white space or empty
        if (string.IsNullOrWhiteSpace(channelUrl))
        {
            return false;
        }

        // it must start with "https://www.youtube.com/"
        if (!channelUrl.StartsWith(baseUrl, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        // Remove the base URL to analyze the path
        string path = channelUrl.Substring(baseUrl.Length);

        // Valid formats:
        // 1. @Username
        // 2. Username
        // 3. channel/ChannelID
        // 4. user/Username

        if (path.StartsWith("@"))
        {
            return path.Length > 1 && IsValidUsername(path.Substring(1));
        }
        else if (path.StartsWith("channel/"))
        {
            string id = path.Substring("channel/".Length);
            return id.Length > 0 && IsAlphanumericOrDashUnderscore(id);
        }
        else if (path.StartsWith("user/"))
        {
            string username = path.Substring("user/".Length);
            return username.Length > 0 && IsValidUsername(username);
        }
        else
        {
            // Just a plain username like BTSportBoxing
            return path.Length > 0 && IsValidUsername(path);
        }
    }

    public static bool UrlHasValidPlaylistId(string playlistUrl)
    {
        // Valid format:
        // https://www.youtube.com/playlist?list=playlistId

        const string baseUrl = "https://www.youtube.com/playlist?list=";

        // It must not be null, white space, or empty
        if (string.IsNullOrWhiteSpace(playlistUrl))
        {
            return false;
        }

        // It must start with "https://www.youtube.com/playlist?list="
        if (!playlistUrl.StartsWith(baseUrl, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        // Remove the base URL part to extract the playlist ID
        string playlistId = playlistUrl.Substring(baseUrl.Length);

        // Playlist ID must be non-empty and match the pattern of alphanumeric characters and dashes
        return IsValidPlaylistId(playlistId);
    }

    // Helper method to validate the playlist ID using a for-loop
    private static bool IsValidPlaylistId(string playlistId)
    {
        // The playlist ID should be alphanumeric with dashes (common format)
        for (int i = 0; i < playlistId.Length; i++)
        {
            char c = playlistId[i];
            if (!char.IsLetterOrDigit(c) && c != '-')
            {
                return false;
            }
        }

        return playlistId.Length > 0;
    }

    private static bool IsValidUsername(string input)
    {
        char c;

        for (int i = 0; i < input.Length; i++)
        {
            c = input[i];
            if (!char.IsLetterOrDigit(c) && c != '-' && c != '_')
            {
                return false;
            }
        }
        return true;
    }

    private static bool IsAlphanumericOrDashUnderscore(string input)
    {
        char c;

        for (int i = 0; i < input.Length; i++)
        {
            c = input[i];
            if (!char.IsLetterOrDigit(c) && c != '-' && c != '_')
            {
                return false;
            }
        }
        return true;
    }

    public static bool IsValidWordOrTopic(string input)
    {
        // eg. "cars", "home decor"
        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        return input.All(c => char.IsLetter(c) || char.IsWhiteSpace(c) || char.IsDigit(c));
    }

    public static string TransformToStandardPinUrl(string pinUrl)
    {
        // eg. "https://www.pinterest.com/pin/wanna-eat-em-2023--15973773672126172/" to "https://www.pinterest.com/pin/15973773672126172/"
        // eg. "https://www.pinterest.com/pin/products--137289488887745186/" to "https://www.pinterest.com/pin/137289488887745186/"

        if (string.IsNullOrEmpty(pinUrl))
        {
            return string.Empty;
        }

        // Find the last occurrence of "--"
        int index = pinUrl.LastIndexOf("--");

        if (index < 0)
        {
            return pinUrl;
        }

        // Extract everything after "--" and remove the trailing "/"
        string extracted = pinUrl.Substring(index + 2).TrimEnd('/');

        // Construct the transformed URL
        string transformed = "/pin/" + extracted + "/";

        return transformed;
    }

    public static string TransformThumbnailToFullImageUrl(string thumbnailUrl)
    {
        //eg. https://i.pinimg.com/236x/b7/0a/8b/b70a8b73d4c254ddcf63591fb34f5360.jpg
        return thumbnailUrl.Replace("236x", "736x");
    }
}