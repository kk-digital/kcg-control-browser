namespace user_profile_management;

// stores simplified digital fingerprint of a browser
public class BrowserFingerprint
{
    public string Platform { get; set; }        // The browser's reported platform or operating system (e.g., "Win32", "Linux x86_64", "MacIntel").
    public string WebGLVendor { get; set; }     // The graphics card vendor reported by WebGL, typically accessed via JavaScript
    public string WebGLRenderer { get; set; }   // The GPU renderer string reported by WebGL
}