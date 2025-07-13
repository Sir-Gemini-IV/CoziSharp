using System.Text.Json;
using System.Text.Json.Serialization;

namespace CoziSharp.Models
{
    /// <summary>
    /// A single shopping‑ or to‑do‑list item in Cozi.
    /// </summary>
    public sealed class ItemDto
    {
        [JsonPropertyName("itemId")]
        public string ItemId { get; set; } = string.Empty;

        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;

        [JsonPropertyName("checkedOff")]
        public bool CheckedOff { get; set; }

        // ------------ optional / rarely‑used fields ------------

        [JsonPropertyName("version")]
        public int? Version { get; set; }

        [JsonPropertyName("category")]
        public string? Category { get; set; }

        [JsonPropertyName("quantity")]
        public string? Quantity { get; set; }

        [JsonPropertyName("whoAdded")]
        public string? WhoAdded { get; set; }

        [JsonPropertyName("createdTime")]
        public long? CreatedUnixMs { get; set; }

        [JsonPropertyName("modifiedTime")]
        public long? ModifiedUnixMs { get; set; }

        /// <summary>Capture any future / unknown fields without breaking.</summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? Extra { get; set; }

        /* ---------- helpers ---------- */
        public DateTimeOffset? CreatedUtc => CreatedUnixMs.HasValue ? DateTimeOffset.FromUnixTimeMilliseconds(CreatedUnixMs.Value) : null;
        public DateTimeOffset? ModifiedUtc => ModifiedUnixMs.HasValue ? DateTimeOffset.FromUnixTimeMilliseconds(ModifiedUnixMs.Value) : null;
    }
}
