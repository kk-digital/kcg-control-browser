namespace Utility;

/// <summary>
/// use for writing partial json data to file to avoid overloading memory when processing 
/// large list of Pinterest Boards or YouTube Channels
/// </summary>
public class JsonBatchWriter
{
    // =================================================================================
    // add extra indentation to a json string
    public static string AddExtraIndentation(string json, int extraSpaces)
    {
        string extraSpacesString = new string(' ', extraSpaces);
        string[] lines = json.Split(Environment.NewLine);
        for (int i = 0; i < lines.Length; i++)
        {
            lines[i] = extraSpacesString + lines[i];
        }
        return string.Join(Environment.NewLine, lines);
    }
    //=================================================================================================
    public static void AppendToJsonFile(string filePath, string jsonData)
    {
        using (StreamWriter writer = new StreamWriter(filePath, append: true))
        {
            writer.Write(jsonData);
        }
    }
    //=================================================================================================
    public static void AppendOpening(string filePath)
    {
        AppendToJsonFile(filePath, "[\n");
    }
    //=================================================================================================
    public static void AppendClosing(string filePath)
    {
        AppendToJsonFile(filePath, "\n]\n");
    }
    //=================================================================================================
}

