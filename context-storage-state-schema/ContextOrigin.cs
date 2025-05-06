using System.Text.Json.Serialization;

namespace context_storage_state_schema;

public class ContextOrigin
{
    [JsonPropertyName("origin")]
    public string OriginUrl { get; set; }

    [JsonPropertyName("localStorage")]
    public ContextLocalStorageItem[] LocalStorage { get; set; }
}