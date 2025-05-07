using System.Text.Json.Serialization;

namespace context_storage_state_schema;

public class ContextLocalStorageItem
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("value")]
    public string Value { get; set; }
}