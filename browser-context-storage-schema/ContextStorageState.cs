using System.Text.Json.Serialization;

namespace browser_context_storage_schema;

// use to store data for browser context storage state
public class ContextStorageState
{
    [JsonPropertyName("cookies")]
    public Cookie[] Cookies { get; set; }

    [JsonPropertyName("origins")]
    public ContextOrigin[] Origins { get; set; }
}