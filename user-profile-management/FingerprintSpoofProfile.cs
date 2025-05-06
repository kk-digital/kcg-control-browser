namespace user_profile_management;

public class FingerprintSpoofProfile
{
    // Screen properties (reported device screen resolution)
    public int ScreenWidth { get; set; }
    public int ScreenHeight { get; set; }
    public int ColorDepth { get; set; }  // e.g., 24 or 32

    // Window properties (browser window dimensions)
    public int WindowOuterWidth { get; set; }
    public int WindowOuterHeight { get; set; }
    public int WindowInnerWidth { get; set; }
    public int WindowInnerHeight { get; set; }

    // Viewport size (content area size)
    public int ViewportWidth { get; set; }
    public int ViewportHeight { get; set; }

    // Device pixel ratio (screen DPI scaling)
    public double DevicePixelRatio { get; set; }

    // Navigator properties (browser & platform info)
    public string UserAgent { get; set; }
    public string Platform { get; set; }
    public string Language { get; set; }

    // WebGL spoofing (GPU vendor and renderer)
    public string WebGLVendor { get; set; }
    public string WebGLRenderer { get; set; }

    // Timezone info (helps avoid timezone mismatches)
    public string Timezone { get; set; }
    public int TimezoneOffset { get; set; }  // in minutes

    // Do Not Track (privacy setting)
    public bool DoNotTrack { get; set; }
}

// Spoof all key fingerprint properties to avoid bot detection
// (() => {
//     // Screen properties
//     Object.defineProperty(screen, 'width', { get: () => SPOOFED_SCREEN_WIDTH });
//     Object.defineProperty(screen, 'height', { get: () => SPOOFED_SCREEN_HEIGHT });
//     Object.defineProperty(screen, 'colorDepth', { get: () => SPOOFED_COLOR_DEPTH });
//
//     // Device pixel ratio
//     Object.defineProperty(window, 'devicePixelRatio', { get: () => SPOOFED_DEVICE_PIXEL_RATIO });
//
//     // Window and viewport properties
//     Object.defineProperty(window, 'outerWidth', { get: () => SPOOFED_WINDOW_OUTER_WIDTH });
//     Object.defineProperty(window, 'outerHeight', { get: () => SPOOFED_WINDOW_OUTER_HEIGHT });
//     Object.defineProperty(window, 'innerWidth', { get: () => SPOOFED_WINDOW_INNER_WIDTH });
//     Object.defineProperty(window, 'innerHeight', { get: () => SPOOFED_WINDOW_INNER_HEIGHT });
//
//     // Viewport size (for document.documentElement)
//     Object.defineProperty(document.documentElement, 'clientWidth', { get: () => SPOOFED_VIEWPORT_WIDTH });
//     Object.defineProperty(document.documentElement, 'clientHeight', { get: () => SPOOFED_VIEWPORT_HEIGHT });
//
//     // Navigator properties
//     Object.defineProperty(navigator, 'userAgent', { get: () => 'SPOOFED_USER_AGENT' });
//     Object.defineProperty(navigator, 'platform', { get: () => 'SPOOFED_PLATFORM' });
//     Object.defineProperty(navigator, 'language', { get: () => 'SPOOFED_LANGUAGE' });
//     Object.defineProperty(navigator, 'doNotTrack', { get: () => SPOOFED_DO_NOT_TRACK ? "1" : "0" });
//
//     // WebGL spoofing
//     const origGetParameter = WebGLRenderingContext.prototype.getParameter;
//     WebGLRenderingContext.prototype.getParameter = function(p) {
//         if (p === 37445) return 'SPOOFED_WEBGL_VENDOR';      // UNMASKED_VENDOR_WEBGL
//         if (p === 37446) return 'SPOOFED_WEBGL_RENDERER';    // UNMASKED_RENDERER_WEBGL
//         return origGetParameter.call(this, p);
//     };
//
//     // Timezone spoofing
//     Object.defineProperty(Intl.DateTimeFormat.prototype, 'resolvedOptions', {
//         value: function() {
//             return {
//                 timeZone: 'SPOOFED_TIMEZONE'
//             };
//         }
//     });
//     const spoofedOffset = SPOOFED_TIMEZONE_OFFSET;
//     Date.prototype.getTimezoneOffset = function() { return spoofedOffset; };
// })();
