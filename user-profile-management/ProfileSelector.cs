using System.Collections;
using LogUtility;

namespace user_profile_management;

public class ProfileSelector
{
    private static readonly Random random = new Random();
    // private readonly List<BrowserProfile> _profiles;
    private readonly BrowserProfile[] _profiles;
    
    // timezone including U.S. territories and detailed zones
    private static readonly string[] Timezones = new string[]
    {
        // Mainland U.S. (including multiple Eastern and Central zones)
        "America/New_York",               // Eastern Time (most populous)
        "America/Detroit",                // Eastern Time (Michigan)
        "America/Kentucky/Louisville",   // Eastern Time (Kentucky)
        "America/Kentucky/Monticello",   // Eastern Time (Kentucky)
        "America/Indiana/Indianapolis",  // Eastern Time (Indiana)
        "America/Indiana/Vincennes",     // Eastern Time (Indiana)
        "America/Indiana/Winamac",       // Eastern Time (Indiana)
        "America/Indiana/Marengo",       // Eastern Time (Indiana)
        "America/Indiana/Petersburg",    // Eastern Time (Indiana)
        "America/Indiana/Vevay",         // Eastern Time (Indiana)

        "America/Chicago",               // Central Time (most populous)
        "America/Indiana/Knox",          // Central Time (Indiana)
        "America/Menominee",             // Central Time (Michigan)
        "America/North_Dakota/Center",  // Central Time (North Dakota)
        "America/North_Dakota/New_Salem",// Central Time (North Dakota)
        "America/North_Dakota/Beulah",  // Central Time (North Dakota)

        "America/Denver",                // Mountain Time
        "America/Boise",                 // Mountain Time (Idaho)
        "America/Phoenix",               // Mountain Standard Time (Arizona - no DST)

        "America/Los_Angeles",           // Pacific Time

        "America/Anchorage",             // Alaska Time
        "America/Juneau",                // Alaska Time
        "America/Nome",                  // Alaska Time
        "America/Sitka",                 // Alaska Time
        "America/Yakutat",               // Alaska Time

        "Pacific/Honolulu",              // Hawaii-Aleutian Time

        // U.S. Territories
        "America/Puerto_Rico",           // Atlantic Time (Puerto Rico)
        "America/St_Thomas",             // Atlantic Time (U.S. Virgin Islands)
        "Pacific/Samoa",                 // Samoa Time (American Samoa)
        "Pacific/Guam",                  // Chamorro Time (Guam)
        "Pacific/Marquesas",             // Marquesas Time (U.S. Minor Outlying Islands)
        "Pacific/Midway"                 // Samoa Time Zone (U.S. Minor Outlying Islands)
    };
    
    // Approximate population weights including territories and detailed zones
    private static readonly double[] TimezoneWeights = new double[]
    {
        // Eastern Time zones combined weight ~47.6%
        0.35,  // America/New_York
        0.03,  // America/Detroit
        0.005, // America/Kentucky/Louisville
        0.005, // America/Kentucky/Monticello
        0.005, // America/Indiana/Indianapolis
        0.002, // America/Indiana/Vincennes
        0.002, // America/Indiana/Winamac
        0.002, // America/Indiana/Marengo
        0.002, // America/Indiana/Petersburg
        0.002, // America/Indiana/Vevay

        // Central Time zones combined weight ~29.1%
        0.25,  // America/Chicago
        0.005, // America/Indiana/Knox
        0.002, // America/Menominee
        0.002, // America/North_Dakota/Center
        0.002, // America/North_Dakota/New_Salem
        0.002, // America/North_Dakota/Beulah

        0.067, // America/Denver (Mountain Time)
        0.005, // America/Boise
        0.01,  // America/Phoenix (no DST)

        0.166, // America/Los_Angeles (Pacific Time)

        0.007, // America/Anchorage (Alaska Time)
        0.002, // America/Juneau
        0.001, // America/Nome
        0.001, // America/Sitka
        0.001, // America/Yakutat

        0.008, // Pacific/Honolulu (Hawaii-Aleutian Time)

        // Territories (small combined population, weights approximate)
        0.003, // America/Puerto_Rico
        0.001, // America/St_Thomas
        0.0005, // Pacific/Samoa
        0.0005, // Pacific/Guam
        0.0003, // Pacific/Marquesas
        0.0003  // Pacific/Midway
    };

    // Language weights
    private static readonly string[] LanguageKeys = new string[]
    {
        "en-US,en;q=0.9",
        "en-US,en;q=0.8"
    };

    private static readonly double[] LanguageWeights = new double[]
    {
        0.6,
        0.3
    };

    public ProfileSelector(BrowserProfile[] profiles)
    {
        _profiles = profiles;
    }
    
    public BrowserProfile GetWeightedRandomProfileByTimeZone(string timeZone)
    {
        // Filter profiles by timezone into a non-generic ArrayList
        ArrayList filteredProfiles = new ArrayList();
        
        for (int i = 0; i < _profiles.Length; i++)
        {
            if (_profiles[i].Timezone == timeZone)
            {
                filteredProfiles.Add(_profiles[i]);
            }
        }

        if (filteredProfiles.Count == 0)
        {
            LibLog.LogError("No profiles found for timezone: " + timeZone);
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

        return null;
    }

    public BrowserProfile GetProfileByTimeZone(string timezone)
    {
        List<BrowserProfile> matchingProfiles = new List<BrowserProfile>();

        for (int i = 0; i < _profiles.Length; i++)
        {
            BrowserProfile profile = _profiles[i];
            if (string.Equals(profile.Timezone, timezone, StringComparison.OrdinalIgnoreCase))
            {
                matchingProfiles.Add(profile);
            }
        }

        if (matchingProfiles.Count == 0)
        {
            LibLog.LogError($"No profile found for timezone: {timezone}");
            return null;
        }

        Random random = new Random();
        int index = random.Next(matchingProfiles.Count);
        return matchingProfiles[index];
    }
}