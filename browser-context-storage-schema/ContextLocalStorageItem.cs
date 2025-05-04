using System.Text.Json.Serialization;

namespace browser_context_storage_schema;

public class ContextLocalStorageItem
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("value")]
    public string Value { get; set; }
}