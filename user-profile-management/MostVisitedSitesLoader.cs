namespace user_profile_management;

public class MostVisitedSitesLoader
{
    private static readonly Random random = new Random();
        
    public static string[] SelectRandomSitesFromFile(string filePath, int selectionCount)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentNullException(nameof(filePath));
        }

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("Most visited sites' file not found.", filePath);
        }

        // Read all lines from the file and trim them
        string[] lines = File.ReadAllLines(filePath);
        List<string> siteList = new List<string>();

        foreach (string line in lines)
        {
            string trimmed = line.Trim();
            if (!string.IsNullOrEmpty(trimmed))
            {
                siteList.Add(trimmed);
            }
        }

        // Fisher-Yates shuffle
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

        return siteList.Take(selectionCount).ToArray();
    }
}
