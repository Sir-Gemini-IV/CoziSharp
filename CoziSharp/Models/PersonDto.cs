// PersonDto.cs
// Represents a person (family member) from the Cozi account.

using System.Text.Json.Serialization;

namespace CoziSharp.Models
{
    /// <summary>
    /// A Cozi account person (e.g. family member).
    /// </summary>
    public sealed class PersonDto
    {
        [JsonPropertyName("personId")]
        public string PersonId { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("color")]
        public string? Color { get; set; }  // e.g., "#FF3366"

        [JsonPropertyName("initials")]
        public string? Initials { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }  // e.g., "adult", "child"

        [JsonExtensionData]
        public Dictionary<string, System.Text.Json.JsonElement>? Extra { get; set; }
    }
}
