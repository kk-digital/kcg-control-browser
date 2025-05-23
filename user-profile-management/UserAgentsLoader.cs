﻿namespace user_profile_management;

public class UserAgentsLoader
{
    private static readonly Random random = new Random();
        
    public static string[] LoadUserAgentsFromFile(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentNullException(nameof(filePath));
        }

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("User agents file not found.", filePath);
        }

        // Read all lines from the file
        string[] lines = File.ReadAllLines(filePath);

        // Deduplicate using HashSet
        HashSet<string> uniqueUserAgents = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < lines.Length; i++)
        {
            string trimmed = lines[i].Trim();
            if (!string.IsNullOrEmpty(trimmed))
            {
                uniqueUserAgents.Add(trimmed);
            }
        }

        // Convert to list for shuffling
        List<string> userAgentList = new List<string>(uniqueUserAgents);

        // Fisher-Yates shuffle
        int n = userAgentList.Count;
        for (int i = n - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            string temp = userAgentList[i];
            userAgentList[i] = userAgentList[j];
            userAgentList[j] = temp;
        }

        return userAgentList.ToArray();
    }
}