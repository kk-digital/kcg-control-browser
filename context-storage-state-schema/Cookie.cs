using System.Text.Json.Serialization;

namespace context_storage_state_schema;

// Represents a web browser cookie with common attributes used for serialization/deserialization
public class Cookie
{
    // The name of the cookie
    [JsonPropertyName("name")]
    public string Name { get; set; }

    // The value stored in the cookie
    [JsonPropertyName("value")]
    public string Value { get; set; }

    // The domain for which the cookie is valid (cookie sent to this domain and subdomains)
    [JsonPropertyName("domain")]
    public string Domain { get; set; }

    // The URL path that must exist in the requested URL for the cookie to be sent
    [JsonPropertyName("path")]
    public string Path { get; set; }

    // Expiration time of the cookie, typically a Unix timestamp (seconds since epoch)
    [JsonPropertyName("expires")]
    public double Expires { get; set; }

    // Indicates if the cookie is HTTP-only (not accessible via JavaScript)
    [JsonPropertyName("httpOnly")]
    public bool HttpOnly { get; set; }

    // Indicates if the cookie is secure (only sent over HTTPS)
    [JsonPropertyName("secure")]
    public bool Secure { get; set; }

    // SameSite attribute controls cross-site request behavior ("Strict", "Lax", "None")
    [JsonPropertyName("sameSite")]
    public string SameSite { get; set; }
}
