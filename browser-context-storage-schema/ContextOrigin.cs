using System.Text.Json.Serialization;

namespace browser_context_storage_schema;

public class ContextOrigin
{
    [JsonPropertyName("origin")]
    public string OriginUrl { get; set; }

    [JsonPropertyName("localStorage")]
    public ContextLocalStorageItem[] LocalStorage { get; set; }
}