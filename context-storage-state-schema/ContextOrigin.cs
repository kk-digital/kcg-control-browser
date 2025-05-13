using System.Text.Json.Serialization;

namespace context_storage_state_schema;

// Represents the origin context of a web environment, including its URL and local storage items
public class ContextOrigin
{
    // The origin URL associated with this context
    [JsonPropertyName("origin")]
    public string OriginUrl { get; set; }

    // Array of local storage items associated with this origin
    [JsonPropertyName("localStorage")]
    public ContextLocalStorageItem[] LocalStorage { get; set; }
}
