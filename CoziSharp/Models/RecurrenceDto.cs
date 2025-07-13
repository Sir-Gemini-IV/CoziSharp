using System.Text.Json.Serialization;
using System.Text.Json;

namespace CoziSharp.Models
{
    public sealed class RecurrenceDto
    {
        // Raw fields – Cozi already gives us text + rules arrays; we won’t parse rules yet.
        public List<string>? Text { get; set; }   // e.g. "Every year on July 4th"
        public JsonElement? Rules { get; set; }   // keep as JsonElement for maximum flexibility

        [JsonExtensionData] public Dictionary<string, JsonElement>? Extra { get; set; }
    }
}
