using System;
using System.IO;
using System.Threading.Tasks;

namespace Utility
{
    // Use for writing partial JSON data to file to avoid overloading memory when processing 
    // large list of Pinterest Boards or YouTube Channels
    public class JsonBatchWriter
    {
        // =================================================================================
        // Add extra indentation to a JSON string
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
        public static async Task AppendToJsonFileAsync(string filePath, string jsonData)
        {
            // Use FileStream with async enabled for better performance on some platforms
            using (FileStream fs = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.None, 4096, useAsync: true))
            using (StreamWriter writer = new StreamWriter(fs))
            {
                await writer.WriteAsync(jsonData);
            }
        }

        //=================================================================================================
        public static async Task AppendOpeningAsync(string filePath)
        {
            await AppendToJsonFileAsync(filePath, "[\n");
        }

        //=================================================================================================
        public static async Task AppendClosingAsync(string filePath)
        {
            await AppendToJsonFileAsync(filePath, "\n]\n");
        }
    }
}

// sample usage:

// static async Task Main()
// {
//     string filePath = "output.json";
//
//     // Step 1: Write the opening bracket for the JSON array asynchronously
//     await JsonBatchWriter.AppendOpeningAsync(filePath);
//
//     // Example list of items to append (can be large)
//     var items = new[]
//     {
//         new { Id = 1, Name = "Board A", Pins = 100 },
//         new { Id = 2, Name = "Board B", Pins = 250 },
//         new { Id = 3, Name = "Board C", Pins = 75 }
//     };
//
//     // Step 2: Append each item as JSON asynchronously, adding commas between items
//     for (int i = 0; i < items.Length; i++)
//     {
//         string jsonItem = JsonSerializer.Serialize(items[i], new JsonSerializerOptions { WriteIndented = true });
//
//         // Optionally add extra indentation if desired (e.g., 2 spaces)
//         jsonItem = JsonBatchWriter.AddExtraIndentation(jsonItem, 2);
//
//         // Add a comma before all items except the first
//         if (i > 0)
//         {
//             jsonItem = "," + Environment.NewLine + jsonItem;
//         }
//
//         await JsonBatchWriter.AppendToJsonFileAsync(filePath, jsonItem);
//     }
//
//     // Step 3: Write the closing bracket for the JSON array asynchronously
//     await JsonBatchWriter.AppendClosingAsync(filePath);
//
//     Console.WriteLine($"JSON file '{filePath}' has been written asynchronously with {items.Length} items.");
// }





