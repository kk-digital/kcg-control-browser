namespace user_profile_management;

public class ProxyIpWithProfiles
{
    Random random = new Random();
    public string ProxyIp { get; set; }
    
    // list of associated user profiles
    public List<BrowserProfile> Profiles { get; set; } = new List<BrowserProfile>();
    
    // index of last used user profile
    public int LastProfileUsedIndex { get; set; } = -1;

    public BrowserProfile GetProfileToUse()
    {
        int newProfileIndex;
        
        if (LastProfileUsedIndex == -1 || LastProfileUsedIndex >= Profiles.Count)
        {
            do
            {
                newProfileIndex = random.Next(0, Profiles.Count);
            } while (newProfileIndex == LastProfileUsedIndex);
            
            LastProfileUsedIndex = newProfileIndex;
        }
        
        return Profiles[LastProfileUsedIndex++];
    }
}