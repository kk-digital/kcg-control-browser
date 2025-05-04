using System.Reflection;

namespace user_profile_management;

public enum US_TimeZone
{
    America_New_York,
    America_Chicago,
    America_Denver,
    America_Los_Angeles,
    America_Anchorage,
    Pacific_Honolulu
}

public static class UsTimeZoneExtensions
{
    public static string GetTimeZoneString(this US_TimeZone timeZone)
    {
        switch (timeZone)
        {
            case US_TimeZone.America_New_York:
                return "America/New_York";
            case US_TimeZone.America_Chicago:
                return "America/Chicago";
            case US_TimeZone.America_Denver:
                return "America/Denver";
            case US_TimeZone.America_Los_Angeles:
                return "America/Los_Angeles";
            case US_TimeZone.America_Anchorage:
                return "America/Anchorage";
            case US_TimeZone.Pacific_Honolulu:
                return "Pacific/Honolulu";
            default:
                // Fallback: replace underscores with slashes
                return timeZone.ToString().Replace('_', '/');
        }
    }
    
    public static US_TimeZone Resolve(string timeZoneString)
    {
        if (string.IsNullOrWhiteSpace(timeZoneString))
        {
            throw new ArgumentException("Time zone string cannot be null or empty.", nameof(timeZoneString));
        }

        string normalized = timeZoneString.Trim();

        if (string.Equals(normalized, "America/New_York", StringComparison.OrdinalIgnoreCase))
        {
            return US_TimeZone.America_New_York;
        }
        if (string.Equals(normalized, "America/Chicago", StringComparison.OrdinalIgnoreCase))
        {
            return US_TimeZone.America_Chicago;
        }
        if (string.Equals(normalized, "America/Denver", StringComparison.OrdinalIgnoreCase))
        {
            return US_TimeZone.America_Denver;
        }
        if (string.Equals(normalized, "America/Los_Angeles", StringComparison.OrdinalIgnoreCase))
        {
            return US_TimeZone.America_Los_Angeles;
        }
        if (string.Equals(normalized, "America/Anchorage", StringComparison.OrdinalIgnoreCase))
        {
            return US_TimeZone.America_Anchorage;
        }
        if (string.Equals(normalized, "Pacific/Honolulu", StringComparison.OrdinalIgnoreCase))
        {
            return US_TimeZone.Pacific_Honolulu;
        }

        throw new ArgumentException($"Unknown US_TimeZone string: '{timeZoneString}'", nameof(timeZoneString));
    }
}

// usage:
// class Program
// {
//     static void Main()
//     {
//         UsTimeZone tz = UsTimeZone.America_Los_Angeles;
//         string ianaString = tz.GetTimeZoneString();
//
//         Console.WriteLine(ianaString);  // Output: America/Los_Angeles
//     }
// }


