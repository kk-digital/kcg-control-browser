using System.Text.Json.Serialization;

namespace context_storage_state_schema;

// Used to store data for browser context storage state, including cookies and local storage origins
public class ContextStorageState
{
    // Array of cookies stored in the browser context
    [JsonPropertyName("cookies")]
    public Cookie[] Cookies { get; set; }

    // Array of origin contexts, each containing local storage data
    [JsonPropertyName("origins")]
    public ContextOrigin[] Origins { get; set; }
}
