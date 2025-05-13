namespace user_profile_management;

public class MostVisitedSitesLoader
{
    // Static Random instance for random operations
    private static readonly Random random = new Random();
        
    // Reads URLs from a file and returns a random selection of them
    public static string[] SelectRandomSitesFromFile(string filePath, int selectionCount)
    {
        // Return empty array if file path is null, empty, or whitespace
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return Array.Empty<string>();
        }

        // Return empty array if file does not exist
        if (!File.Exists(filePath))
        {
            return Array.Empty<string>();
        }

        // Read all lines from the file
        string[] lines = File.ReadAllLines(filePath);
        List<string> siteList = new List<string>();

        // Trim each line and add non-empty lines to the site list
        for (int i = 0; i < lines.Length; i++)
        {
            string trimmed = lines[i].Trim();
            if (!string.IsNullOrEmpty(trimmed))
            {
                siteList.Add(trimmed);
            }
        }

        // Shuffle the list of sites using Fisher-Yates algorithm to randomize order
        Random random = new Random();
        int n = siteList.Count;
        
        for (int i = n - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            string temp = siteList[i];
            siteList[i] = siteList[j];
            siteList[j] = temp;
        }
    
        // Adjust selection count if it exceeds the number of available sites
        if (selectionCount > siteList.Count)
        {
            selectionCount = siteList.Count;
        }

        // Create an array to hold the selected sites
        string[] result = new string[selectionCount];

        // Copy the first selectionCount sites from the shuffled list to the result array
        for (int i = 0; i < selectionCount; i++)
        {
            result[i] = siteList[i];
        }

        // Return the randomly selected sites
        return result;
    }
}
