namespace user_profile_management;

public class ProxyIpWithProfiles
{
    // Random instance used to select profiles randomly
    Random random = new Random();

    // The proxy IP address represented by this instance
    public string ProxyIp { get; set; }
    
    // List of associated browser user profiles linked to this proxy IP
    public List<BrowserProfile> Profiles { get; set; } = new List<BrowserProfile>();
    
    // Index of the last used profile in the Profiles list; initialized to -1 (none used yet)
    public int LastProfileUsedIndex { get; set; } = -1;

    // Returns a BrowserProfile to use, selecting randomly if no previous profile or index out of range
    public BrowserProfile GetProfileToUse()
    {
        int newProfileIndex;
        
        // If no profile used yet or last used index is invalid
        if (LastProfileUsedIndex == -1 || LastProfileUsedIndex >= Profiles.Count)
        {
            // Pick a new random profile index different from the last used index
            do
            {
                newProfileIndex = random.Next(0, Profiles.Count);
            } while (newProfileIndex == LastProfileUsedIndex);
            
            // Update last used index to the new random index
            LastProfileUsedIndex = newProfileIndex;
        }

        // Return the profile at the current index, then increment the index for next call
        return Profiles[LastProfileUsedIndex++];
    }
}