namespace user_profile_management;

// determines navigator platform and GPU renderer
public static class FingerprintGenerator
{
    private static readonly string[] windowsRenderers = new[]
    {
        ("ANGLE (Intel(R) UHD Graphics 620 Direct3D11 vs_5_0 ps_5_0)"),
        ("ANGLE (NVIDIA GeForce GTX 1650 Direct3D11 vs_5_0 ps_5_0)"),
        ("ANGLE (AMD Radeon RX 580 Series Direct3D11 vs_5_0 ps_5_0)")
    };
    
    private static readonly string[] linuxRenderers = new[]
    {
        ("Mesa Intel(R) UHD Graphics 620 (KBL GT2)"),
        ("Mesa NVIDIA GeForce GTX 1650/PCIe/SSE2")
    };
    
    private static readonly string[] chromeOSrenderers = new[]
    {
        ("ANGLE (Intel(R) UHD Graphics 620 OpenGL)"),
        ("ANGLE (AMD Radeon Vega 8 OpenGL)")
    };
    
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
                return "ANGLE (Apple M1 Pro OpenGL)";
                break;
            
            case "Win32":
                return windowsRenderers[random.Next(0, windowsRenderers.Length)];
                break;
            
            case "Linux x86_64":
                return linuxRenderers[random.Next(0, linuxRenderers.Length)];
                break;
            
            case "CrOS x86_64":
                return chromeOSrenderers[random.Next(0, chromeOSrenderers.Length)];
                break;
            
            default:
                return string.Empty;
        }
    }
    
    public static string GetNavigatorPlatformByUserAgent(string userAgent)
    {
        if (string.IsNullOrWhiteSpace(userAgent))
        {
            return string.Empty;
        }
        
        string agent = userAgent.ToLowerInvariant();

        // Windows 10 and Windows 11 user agents (Chromium does NOT distinguish 11 from 10 in UA)
        if (agent.Contains("windows nt 10.0") || agent.Contains("windows nt 11.0"))
        {
            // Even on 64-bit and ARM, Chromium browsers report "Win32" as platform[1][5]
            return "Win32";
        }
        // macOS
        else if (agent.Contains("macintosh") || agent.Contains("mac os x"))
        {
            // Modern macOS devices report "MacIntel"
            return "MacIntel";
        }
        // Chrome OS
        else if (agent.Contains("cros"))
        {
            // Chrome OS reports "Linux x86_64" as platform
            return "Linux x86_64";
        }
        // Linux
        else if (agent.Contains("linux"))
        {
            return "Linux x86_64";
        }
        else
        {
            // Fallback for unknown/other desktop OS
            return "Win32";
        }
    }
}
