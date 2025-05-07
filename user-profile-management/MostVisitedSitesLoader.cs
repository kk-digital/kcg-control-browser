namespace user_profile_management;

public class MostVisitedSitesLoader
{
    private static readonly Random random = new Random();
        
    public static string[] SelectRandomSitesFromFile(string filePath, int selectionCount)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return new string[0];
        }

        if (!File.Exists(filePath))
        {
            return new string[0];
        }

        // Read all lines from the file and trim them
        string[] lines = File.ReadAllLines(filePath);
        List<string> siteList = new List<string>();

        // Replace foreach with for-loop
        for (int i = 0; i < lines.Length; i++)
        {
            string trimmed = lines[i].Trim();
            if (!string.IsNullOrEmpty(trimmed))
            {
                siteList.Add(trimmed);
            }
        }

        // Fisher-Yates shuffle
        Random random = new Random();
        int n = siteList.Count;
        for (int i = n - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            string temp = siteList[i];
            siteList[i] = siteList[j];
            siteList[j] = temp;
        }
    
        // Return the first selectionCount URLs or all if selectionCount exceeds available sites
        if (selectionCount > siteList.Count)
        {
            selectionCount = siteList.Count;
        }

        string[] result = new string[selectionCount];
        for (int i = 0; i < selectionCount; i++)
        {
            result[i] = siteList[i];
        }

        return result;
    }
}
