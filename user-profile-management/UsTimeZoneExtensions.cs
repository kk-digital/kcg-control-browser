namespace user_profile_management
{
    // Enum representing US and US territories time zones used in the application
    public enum US_TimeZone
    {
        // Mainland U.S. Eastern Time zones (including multiple Indiana and Kentucky zones)
        America_New_York,
        America_Detroit,
        America_Kentucky_Louisville,
        America_Kentucky_Monticello,
        America_Indiana_Indianapolis,
        America_Indiana_Vincennes,
        America_Indiana_Winamac,
        America_Indiana_Marengo,
        America_Indiana_Petersburg,
        America_Indiana_Vevay,

        // Mainland U.S. Central Time zones (including Indiana and North Dakota zones)
        America_Chicago,
        America_Indiana_Knox,
        America_Menominee,
        America_North_Dakota_Center,
        America_North_Dakota_New_Salem,
        America_North_Dakota_Beulah,

        // Mainland U.S. Mountain Time zones
        America_Denver,
        America_Boise,
        America_Phoenix,  // Arizona (no DST)

        // Mainland U.S. Pacific Time zone
        America_Los_Angeles,

        // Alaska Time zones
        America_Anchorage,
        America_Juneau,
        America_Nome,
        America_Sitka,
        America_Yakutat,

        // Hawaii-Aleutian Time zone
        Pacific_Honolulu,

        // U.S. Territories time zones
        America_Puerto_Rico,
        America_St_Thomas,
        Pacific_Samoa,
        Pacific_Guam,
        Pacific_Marquesas,
        Pacific_Midway
    }

    public static class UsTimeZoneExtensions
    {
        // Converts enum value to corresponding IANA time zone string
        public static string GetTimeZoneString(this US_TimeZone timeZone)
        {
            switch (timeZone)
            {
                // Eastern Time zones
                case US_TimeZone.America_New_York:
                    return "America/New_York";
                case US_TimeZone.America_Detroit:
                    return "America/Detroit";
                case US_TimeZone.America_Kentucky_Louisville:
                    return "America/Kentucky/Louisville";
                case US_TimeZone.America_Kentucky_Monticello:
                    return "America/Kentucky/Monticello";
                case US_TimeZone.America_Indiana_Indianapolis:
                    return "America/Indiana/Indianapolis";
                case US_TimeZone.America_Indiana_Vincennes:
                    return "America/Indiana/Vincennes";
                case US_TimeZone.America_Indiana_Winamac:
                    return "America/Indiana/Winamac";
                case US_TimeZone.America_Indiana_Marengo:
                    return "America/Indiana/Marengo";
                case US_TimeZone.America_Indiana_Petersburg:
                    return "America/Indiana/Petersburg";
                case US_TimeZone.America_Indiana_Vevay:
                    return "America/Indiana/Vevay";

                // Central Time zones
                case US_TimeZone.America_Chicago:
                    return "America/Chicago";
                case US_TimeZone.America_Indiana_Knox:
                    return "America/Indiana/Knox";
                case US_TimeZone.America_Menominee:
                    return "America/Menominee";
                case US_TimeZone.America_North_Dakota_Center:
                    return "America/North_Dakota/Center";
                case US_TimeZone.America_North_Dakota_New_Salem:
                    return "America/North_Dakota/New_Salem";
                case US_TimeZone.America_North_Dakota_Beulah:
                    return "America/North_Dakota/Beulah";

                // Mountain Time zones
                case US_TimeZone.America_Denver:
                    return "America/Denver";
                case US_TimeZone.America_Boise:
                    return "America/Boise";
                case US_TimeZone.America_Phoenix:
                    return "America/Phoenix";

                // Pacific Time zone
                case US_TimeZone.America_Los_Angeles:
                    return "America/Los_Angeles";

                // Alaska Time zones
                case US_TimeZone.America_Anchorage:
                    return "America/Anchorage";
                case US_TimeZone.America_Juneau:
                    return "America/Juneau";
                case US_TimeZone.America_Nome:
                    return "America/Nome";
                case US_TimeZone.America_Sitka:
                    return "America/Sitka";
                case US_TimeZone.America_Yakutat:
                    return "America/Yakutat";

                // Hawaii-Aleutian Time zone
                case US_TimeZone.Pacific_Honolulu:
                    return "Pacific/Honolulu";

                // U.S. Territories time zones
                case US_TimeZone.America_Puerto_Rico:
                    return "America/Puerto_Rico";
                case US_TimeZone.America_St_Thomas:
                    return "America/St_Thomas";
                case US_TimeZone.Pacific_Samoa:
                    return "Pacific/Samoa";
                case US_TimeZone.Pacific_Guam:
                    return "Pacific/Guam";
                case US_TimeZone.Pacific_Marquesas:
                    return "Pacific/Marquesas";
                case US_TimeZone.Pacific_Midway:
                    return "Pacific/Midway";

                // Fallback: replace underscores with slashes for any unknown enum values
                default:
                    return timeZone.ToString().Replace('_', '/');
            }
        }

        // Resolves a time zone string to the corresponding enum value
        public static US_TimeZone Resolve(string timeZoneString)
        {
            if (string.IsNullOrWhiteSpace(timeZoneString))
            {
                throw new ArgumentException("Time zone string cannot be null or empty.", nameof(timeZoneString));
            }

            string normalized = timeZoneString.Trim();

            // Match each known time zone string to the enum value
            if (string.Equals(normalized, "America/New_York", StringComparison.OrdinalIgnoreCase))
                return US_TimeZone.America_New_York;
            if (string.Equals(normalized, "America/Detroit", StringComparison.OrdinalIgnoreCase))
                return US_TimeZone.America_Detroit;
            if (string.Equals(normalized, "America/Kentucky/Louisville", StringComparison.OrdinalIgnoreCase))
                return US_TimeZone.America_Kentucky_Louisville;
            if (string.Equals(normalized, "America/Kentucky/Monticello", StringComparison.OrdinalIgnoreCase))
                return US_TimeZone.America_Kentucky_Monticello;
            if (string.Equals(normalized, "America/Indiana/Indianapolis", StringComparison.OrdinalIgnoreCase))
                return US_TimeZone.America_Indiana_Indianapolis;
            if (string.Equals(normalized, "America/Indiana/Vincennes", StringComparison.OrdinalIgnoreCase))
                return US_TimeZone.America_Indiana_Vincennes;
            if (string.Equals(normalized, "America/Indiana/Winamac", StringComparison.OrdinalIgnoreCase))
                return US_TimeZone.America_Indiana_Winamac;
            if (string.Equals(normalized, "America/Indiana/Marengo", StringComparison.OrdinalIgnoreCase))
                return US_TimeZone.America_Indiana_Marengo;
            if (string.Equals(normalized, "America/Indiana/Petersburg", StringComparison.OrdinalIgnoreCase))
                return US_TimeZone.America_Indiana_Petersburg;
            if (string.Equals(normalized, "America/Indiana/Vevay", StringComparison.OrdinalIgnoreCase))
                return US_TimeZone.America_Indiana_Vevay;

            if (string.Equals(normalized, "America/Chicago", StringComparison.OrdinalIgnoreCase))
                return US_TimeZone.America_Chicago;
            if (string.Equals(normalized, "America/Indiana/Knox", StringComparison.OrdinalIgnoreCase))
                return US_TimeZone.America_Indiana_Knox;
            if (string.Equals(normalized, "America/Menominee", StringComparison.OrdinalIgnoreCase))
                return US_TimeZone.America_Menominee;
            if (string.Equals(normalized, "America/North_Dakota/Center", StringComparison.OrdinalIgnoreCase))
                return US_TimeZone.America_North_Dakota_Center;
            if (string.Equals(normalized, "America/North_Dakota/New_Salem", StringComparison.OrdinalIgnoreCase))
                return US_TimeZone.America_North_Dakota_New_Salem;
            if (string.Equals(normalized, "America/North_Dakota/Beulah", StringComparison.OrdinalIgnoreCase))
                return US_TimeZone.America_North_Dakota_Beulah;

            if (string.Equals(normalized, "America/Denver", StringComparison.OrdinalIgnoreCase))
                return US_TimeZone.America_Denver;
            if (string.Equals(normalized, "America/Boise", StringComparison.OrdinalIgnoreCase))
                return US_TimeZone.America_Boise;
            if (string.Equals(normalized, "America/Phoenix", StringComparison.OrdinalIgnoreCase))
                return US_TimeZone.America_Phoenix;

            if (string.Equals(normalized, "America/Los_Angeles", StringComparison.OrdinalIgnoreCase))
                return US_TimeZone.America_Los_Angeles;

            if (string.Equals(normalized, "America/Anchorage", StringComparison.OrdinalIgnoreCase))
                return US_TimeZone.America_Anchorage;
            if (string.Equals(normalized, "America/Juneau", StringComparison.OrdinalIgnoreCase))
                return US_TimeZone.America_Juneau;
            if (string.Equals(normalized, "America/Nome", StringComparison.OrdinalIgnoreCase))
                return US_TimeZone.America_Nome;
            if (string.Equals(normalized, "America/Sitka", StringComparison.OrdinalIgnoreCase))
                return US_TimeZone.America_Sitka;
            if (string.Equals(normalized, "America/Yakutat", StringComparison.OrdinalIgnoreCase))
                return US_TimeZone.America_Yakutat;

            if (string.Equals(normalized, "Pacific/Honolulu", StringComparison.OrdinalIgnoreCase))
                return US_TimeZone.Pacific_Honolulu;

            if (string.Equals(normalized, "America/Puerto_Rico", StringComparison.OrdinalIgnoreCase))
                return US_TimeZone.America_Puerto_Rico;
            if (string.Equals(normalized, "America/St_Thomas", StringComparison.OrdinalIgnoreCase))
                return US_TimeZone.America_St_Thomas;
            if (string.Equals(normalized, "Pacific/Samoa", StringComparison.OrdinalIgnoreCase))
                return US_TimeZone.Pacific_Samoa;
            if (string.Equals(normalized, "Pacific/Guam", StringComparison.OrdinalIgnoreCase))
                return US_TimeZone.Pacific_Guam;
            if (string.Equals(normalized, "Pacific/Marquesas", StringComparison.OrdinalIgnoreCase))
                return US_TimeZone.Pacific_Marquesas;
            if (string.Equals(normalized, "Pacific/Midway", StringComparison.OrdinalIgnoreCase))
                return US_TimeZone.Pacific_Midway;

            // Throw exception if unknown time zone string is provided
            throw new ArgumentException($"Unknown US_TimeZone string: '{timeZoneString}'", nameof(timeZoneString));
        }
    }
}
