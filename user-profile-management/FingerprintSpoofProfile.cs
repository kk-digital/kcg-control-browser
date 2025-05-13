namespace user_profile_management;

// Represents a profile used to spoof browser fingerprint properties to avoid bot detection
public class FingerprintSpoofProfile
{
    // Screen properties (reported device screen resolution)
    public int ScreenWidth { get; set; }
    public int ScreenHeight { get; set; }
    public int ColorDepth { get; set; }  // e.g., 24 or 32 bits per pixel

    // Window properties (browser window outer and inner dimensions)
    public int WindowOuterWidth { get; set; }
    public int WindowOuterHeight { get; set; }
    public int WindowInnerWidth { get; set; }
    public int WindowInnerHeight { get; set; }

    // Viewport size (content area size inside the browser window)
    public int ViewportWidth { get; set; }
    public int ViewportHeight { get; set; }

    // Device pixel ratio (screen DPI scaling factor)
    public double DevicePixelRatio { get; set; }

    // Navigator properties (browser and platform information)
    public string UserAgent { get; set; }
    public string Platform { get; set; }
    public string Language { get; set; }

    // WebGL spoofing properties (GPU vendor and renderer strings)
    public string WebGLVendor { get; set; }
    public string WebGLRenderer { get; set; }

    // Timezone information (to avoid timezone mismatches)
    public string Timezone { get; set; }
    public int TimezoneOffset { get; set; }  // Offset from UTC in minutes

    // Do Not Track privacy setting
    public bool DoNotTrack { get; set; }
}

