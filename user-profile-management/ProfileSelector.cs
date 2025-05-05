using System.Collections;

namespace user_profile_management;

public class ProfileSelector
{
    private static readonly Random random = new Random();
    private readonly List<BrowserProfile> _profiles;
    
    // Population weights per timezone
    private static readonly string[] Timezones = new string[]
    {
        "America/New_York",      // Eastern
        "America/Chicago",       // Central
        "America/Denver",        // Mountain
        "America/Los_Angeles",   // Pacific
        "America/Anchorage",     // Alaska
        "Pacific/Honolulu"       // Hawaii-Aleutian
    };
    
    private static readonly double[] TimezoneWeights = new double[]
    {
        0.476, // Eastern (~47.6%)
        0.291, // Central (~29.1%)
        0.067, // Mountain (~6.7%)
        0.166, // Pacific (~16.6%)
        0.012, // Alaska (~1.2%)
        0.008  // Hawaii-Aleutian (~0.8%)
    };

    // Language weights
    private static readonly string[] LanguageKeys = new string[]
    {
        "en-US,en;q=0.9",
        "en-US,en;q=0.8",
        "es-US,es;q=0.9,en;q=0.7"
    };

    private static readonly double[] LanguageWeights = new double[]
    {
        0.6,
        0.3,
        0.1
    };

    public ProfileSelector(List<BrowserProfile> profiles)
    {
        _profiles = profiles;
    }
    
        public static BrowserProfile GetWeightedRandomProfile(string timeZone, BrowserProfile[] profiles)
    {
        // Filter profiles by timezone into a non-generic ArrayList
        ArrayList filteredProfiles = new ArrayList();
        for (int i = 0; i < profiles.Length; i++)
        {
            if (profiles[i].Timezone == timeZone)
            {
                filteredProfiles.Add(profiles[i]);
            }
        }

        if (filteredProfiles.Count == 0)
        {
            Console.WriteLine("No profiles found for timezone: " + timeZone);
            return null;
        }

        // Find timezone weight
        double timezoneWeight = 1.0; // default weight if not found
        for (int i = 0; i < Timezones.Length; i++)
        {
            if (Timezones[i] == timeZone)
            {
                timezoneWeight = TimezoneWeights[i];
                break;
            }
        }

        // Build weighted list: store profiles and weights in parallel arrays
        BrowserProfile[] filteredArray = new BrowserProfile[filteredProfiles.Count];
        double[] weights = new double[filteredProfiles.Count];

        for (int i = 0; i < filteredProfiles.Count; i++)
        {
            filteredArray[i] = (BrowserProfile)filteredProfiles[i];

            // Find language weight
            double langWeight = 0.1; // default low weight
            for (int j = 0; j < LanguageKeys.Length; j++)
            {
                if (filteredArray[i].AcceptLanguage == LanguageKeys[j])
                {
                    langWeight = LanguageWeights[j];
                    break;
                }
            }

            weights[i] = timezoneWeight * langWeight;
        }

        // Sum weights
        double totalWeight = 0.0;
        for (int i = 0; i < weights.Length; i++)
        {
            totalWeight += weights[i];
        }

        // If totalWeight is zero, assign equal weights
        if (totalWeight == 0.0)
        {
            double equalWeight = 1.0 / weights.Length;
            for (int i = 0; i < weights.Length; i++)
            {
                weights[i] = equalWeight;
            }
            totalWeight = 1.0;
        }

        // Weighted random selection
        double r = random.NextDouble() * totalWeight;
        double cumulative = 0.0;

        for (int i = 0; i < weights.Length; i++)
        {
            cumulative += weights[i];
            if (r <= cumulative)
            {
                return filteredArray[i];
            }
        }

        // Fallback (should not happen)
        return filteredArray[filteredArray.Length - 1];
    }

    public BrowserProfile GetProfileByTimezone(string timezone)
    {
        List<BrowserProfile> matchingProfiles = new List<BrowserProfile>();

        for (int i = 0; i < _profiles.Count; i++)
        {
            BrowserProfile profile = _profiles[i];
            if (string.Equals(profile.Timezone, timezone, StringComparison.OrdinalIgnoreCase))
            {
                matchingProfiles.Add(profile);
            }
        }

        if (matchingProfiles.Count == 0)
        {
            throw new Exception($"No profile found for timezone: {timezone}");
        }

        Random random = new Random();
        int index = random.Next(matchingProfiles.Count);
        return matchingProfiles[index];
    }
}