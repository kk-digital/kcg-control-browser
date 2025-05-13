using System.Text.Json.Serialization;

namespace context_storage_state_schema;

// Represents a single key-value pair item stored in the browser's local storage
public class ContextLocalStorageItem
{
    // The key/name of the local storage item
    [JsonPropertyName("name")]
    public string Name { get; set; }

    // The value associated with the key in local storage
    [JsonPropertyName("value")]
    public string Value { get; set; }
}
