namespace user_profile_management;

// Generates fingerprint data by determining navigator platform and GPU renderer
public static class FingerprintGenerator
{
    // Common GPU renderer strings for Windows platforms
    private static readonly string[] windowsRenderers = new[]
    {
        "ANGLE (Intel(R) UHD Graphics 620 Direct3D11 vs_5_0 ps_5_0)",
        "ANGLE (NVIDIA GeForce GTX 1650 Direct3D11 vs_5_0 ps_5_0)",
        "ANGLE (AMD Radeon RX 580 Series Direct3D11 vs_5_0 ps_5_0)"
    };
    
    // Common GPU renderer strings for Linux platforms
    private static readonly string[] linuxRenderers = new[]
    {
        "Mesa Intel(R) UHD Graphics 620 (KBL GT2)",
        "Mesa NVIDIA GeForce GTX 1650/PCIe/SSE2"
    };
    
    // Common GPU renderer strings for Chrome OS platforms
    private static readonly string[] chromeOSrenderers = new[]
    {
        "ANGLE (Intel(R) UHD Graphics 620 OpenGL)",
        "ANGLE (AMD Radeon Vega 8 OpenGL)"
    };
    
    // Returns a plausible GPU renderer string based on the navigator platform.
    // For some platforms, it randomly selects from a predefined list of renderers.
    public static string GetGPUrendererByNavigatorPlatform(string platform)
    {
        if (string.IsNullOrWhiteSpace(platform))
        {
            return string.Empty;
        }
        
        Random random = new Random();
        
        switch (platform)
        {
            case "MacIntel":
                // Fixed GPU renderer string for Mac M1 Pro
                return "ANGLE (Apple M1 Pro OpenGL)";
            
            case "Win32":
                // Randomly select a Windows GPU renderer
                return windowsRenderers[random.Next(0, windowsRenderers.Length)];
            
            case "Linux x86_64":
                // Randomly select a Linux GPU renderer
                return linuxRenderers[random.Next(0, linuxRenderers.Length)];
            
            case "CrOS x86_64":
                // Randomly select a Chrome OS GPU renderer
                return chromeOSrenderers[random.Next(0, chromeOSrenderers.Length)];
            
            default:
                // Unknown platform returns empty string
                return string.Empty;
        }
    }
    
    // Determines the navigator platform string based on the user agent string.
    // Uses common substrings to identify Windows, macOS, Chrome OS, and Linux.
    public static string GetNavigatorPlatformByUserAgent(string userAgent)
    {
        if (string.IsNullOrWhiteSpace(userAgent))
        {
            return string.Empty;
        }
        
        string agent = userAgent.ToLowerInvariant();

        // Windows 10 and 11 user agents (Chromium reports "Win32" regardless of architecture)
        if (agent.Contains("windows nt 10.0") || agent.Contains("windows nt 11.0"))
        {
            return "Win32";
        }
        // macOS user agents
        else if (agent.Contains("macintosh") || agent.Contains("mac os x"))
        {
            return "MacIntel";
        }
        // Chrome OS user agents
        else if (agent.Contains("cros"))
        {
            // Chrome OS reports platform as Linux x86_64
            return "Linux x86_64";
        }
        // Linux user agents
        else if (agent.Contains("linux"))
        {
            return "Linux x86_64";
        }
        else
        {
            // Default fallback platform
            return "Win32";
        }
    }
}
