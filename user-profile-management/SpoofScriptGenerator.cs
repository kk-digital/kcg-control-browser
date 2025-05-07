
namespace user_profile_management;

public static class SpoofScriptGenerator
{
    private static readonly Random _random = new Random();

    public static string GenerateSpoofScript(
        DisplayResolution displayResolution,
        US_TimeZone timeZone,
        string locale,
        string userAgent,
        BrowserContextViewportSize viewportSize)
    {
        FingerprintSpoofProfile profile = new FingerprintSpoofProfile();

        // Screen info
        profile.ScreenWidth = displayResolution.Width;
        profile.ScreenHeight = displayResolution.Height;
        profile.ColorDepth = 24; // Common color depth
        profile.DevicePixelRatio = 1.0; // Typical for many desktop displays

        // Use viewportSize to set inner window and viewport sizes realistically
        // Clamp viewport size to screen size
        int viewportWidth = Math.Min(viewportSize.Width, displayResolution.Width);
        int viewportHeight = Math.Min(viewportSize.Height, displayResolution.Height);

        // Window inner size is usually close to viewport size (may be equal or slightly bigger)
        profile.WindowInnerWidth = viewportWidth + _random.Next(0, 21);   // +0 to +20 px random padding
        profile.WindowInnerHeight = viewportHeight + _random.Next(0, 31); // +0 to +30 px random padding

        // Window outer size is usually bigger than inner size (browser chrome)
        profile.WindowOuterWidth = profile.WindowInnerWidth + _random.Next(80, 121);  // +80 to +120 px
        profile.WindowOuterHeight = profile.WindowInnerHeight + _random.Next(100, 141); // +100 to +140 px

        // Viewport size usually matches inner window size or slightly smaller
        profile.ViewportWidth = viewportWidth;
        profile.ViewportHeight = viewportHeight;

        // Get fingerprint info based on user agent

        profile.UserAgent = userAgent;
        profile.Platform = FingerprintGenerator.GetNavigatorPlatformByUserAgent(userAgent);
        profile.WebGLVendor = "Google Inc.";
        profile.WebGLRenderer = FingerprintGenerator.GetGPUrendererByNavigatorPlatform(profile.Platform);
        profile.Language = locale;

        // Timezone info
        profile.Timezone = timeZone.GetTimeZoneString();

        // Timezone offset in minutes
        switch (timeZone)
        {
            case US_TimeZone.America_New_York:
                profile.TimezoneOffset = 240;
                break;
            case US_TimeZone.America_Chicago:
                profile.TimezoneOffset = 300;
                break;
            case US_TimeZone.America_Denver:
                profile.TimezoneOffset = 360;
                break;
            case US_TimeZone.America_Los_Angeles:
                profile.TimezoneOffset = 420;
                break;
            case US_TimeZone.America_Anchorage:
                profile.TimezoneOffset = 480;
                break;
            case US_TimeZone.Pacific_Honolulu:
                profile.TimezoneOffset = 600;
                break;
            default:
                profile.TimezoneOffset = 0;
                break;
        }

        profile.DoNotTrack = true;

        // Build JS spoofing script with injected values
        string script = $@"
(() => {{
    // Screen properties
    Object.defineProperty(screen, 'width', {{ get: () => {profile.ScreenWidth} }});
    Object.defineProperty(screen, 'height', {{ get: () => {profile.ScreenHeight} }});
    Object.defineProperty(screen, 'colorDepth', {{ get: () => {profile.ColorDepth} }});

    // Device pixel ratio
    Object.defineProperty(window, 'devicePixelRatio', {{ get: () => {profile.DevicePixelRatio.ToString(System.Globalization.CultureInfo.InvariantCulture)} }});

    // Window and viewport properties
    Object.defineProperty(window, 'outerWidth', {{ get: () => {profile.WindowOuterWidth} }});
    Object.defineProperty(window, 'outerHeight', {{ get: () => {profile.WindowOuterHeight} }});
    Object.defineProperty(window, 'innerWidth', {{ get: () => {profile.WindowInnerWidth} }});
    Object.defineProperty(window, 'innerHeight', {{ get: () => {profile.WindowInnerHeight} }});

    Object.defineProperty(document.documentElement, 'clientWidth', {{ get: () => {profile.ViewportWidth} }});
    Object.defineProperty(document.documentElement, 'clientHeight', {{ get: () => {profile.ViewportHeight} }});

    // Navigator properties
    Object.defineProperty(navigator, 'userAgent', {{ get: () => '{profile.UserAgent}' }});
    Object.defineProperty(navigator, 'platform', {{ get: () => '{profile.Platform}' }});
    Object.defineProperty(navigator, 'language', {{ get: () => '{profile.Language}' }});
    Object.defineProperty(navigator, 'doNotTrack', {{ get: () => '{(profile.DoNotTrack ? "1" : "0")}' }});
    Object.defineProperty(navigator, 'webdriver', {{ get: () => undefined }});

    // WebGL spoofing
    const origGetParameter = WebGLRenderingContext.prototype.getParameter;
    WebGLRenderingContext.prototype.getParameter = function(p) {{
        if (p === 37445) return '{profile.WebGLVendor}';      // UNMASKED_VENDOR_WEBGL
        if (p === 37446) return '{profile.WebGLRenderer}';    // UNMASKED_RENDERER_WEBGL
        return origGetParameter.call(this, p);
    }};

    // Timezone spoofing
    Object.defineProperty(Intl.DateTimeFormat.prototype, 'resolvedOptions', {{
        configurable: true,
        writable: true,
        value: function() {{
            return {{ timeZone: '{profile.Timezone}' }};
        }}
    }});

    // Spoof hardwareConcurrency
    Object.defineProperty(navigator, 'hardwareConcurrency', {{
        get: () => 8
    }});
    Date.prototype.getTimezoneOffset = function() {{ return {profile.TimezoneOffset}; }};
}})();
";

        return script;
    }
}

