using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

namespace CoziSharp.Models
{
    public sealed class ItemDetailsDto
    {
        [JsonPropertyName("location")] public string? Location { get; set; }
        [JsonPropertyName("notes")] public string? Notes { get; set; }
        [JsonPropertyName("readOnly")] public bool? ReadOnly { get; set; }

        // Recurrence information -------------------------------------------
        [JsonPropertyName("recurrenceStartDay")] public string? RecurrenceStartDayRaw { get; set; }
        [JsonPropertyName("recurrence")] public RecurrenceDto? Recurrence { get; set; }

        [JsonExtensionData] public Dictionary<string, JsonElement>? Extra { get; set; }

        [JsonIgnore]
        public DateTime? RecurrenceStartDay =>
            RecurrenceStartDayRaw != null ? DateTime.Parse(RecurrenceStartDayRaw) : null;
    }
}
