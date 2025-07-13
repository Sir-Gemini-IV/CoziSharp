using System.Text.Json.Serialization;
using System.Text.Json;

namespace CoziSharp.Models
{
    public sealed class CalendarItemDto
    {
        // Core identifiers ---------------------------------------------------
        [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
        [JsonPropertyName("itemType")] public string ItemType { get; set; } = string.Empty;   // usually "appointment"
        [JsonPropertyName("itemVersion")] public int ItemVersion { get; set; }

        [JsonPropertyName("attendeeSet")]
        public List<string>? AttendeeSet { get; set; }

        // Descriptive fields -------------------------------------------------
        [JsonPropertyName("description")] public string Description { get; set; } = string.Empty;
        [JsonPropertyName("descriptionShort")] public string? DescriptionShort { get; set; }
        [JsonPropertyName("itemSource")] public string? ItemSource { get; set; }  // "Holiday US - Cozi", etc.

        // Date & time --------------------------------------------------------
        [JsonPropertyName("day")] public string DayRaw { get; set; } = string.Empty;          // "yyyy-MM-dd" (local)
        [JsonPropertyName("startTime")] public string StartTime { get; set; } = "00:00:00";  // HH:mm:ss
        [JsonPropertyName("endTime")] public string EndTime { get; set; } = "00:00:00";
        [JsonPropertyName("dateSpan")] public int DateSpan { get; set; } = 1;               // multi‑day events

        // Details object -----------------------------------------------------
        [JsonPropertyName("itemDetails")] public ItemDetailsDto? Details { get; set; }

        // Catch‑all for forward‑compatibility --------------------------------
        [JsonExtensionData] public Dictionary<string, JsonElement>? Extra { get; set; }

        /* ----- convenience properties (not serialized) ------------------- */
        [JsonIgnore] public DateTime Date => DateTime.Parse(DayRaw);
        [JsonIgnore] public TimeSpan Start => TimeSpan.Parse(StartTime);
        [JsonIgnore] public TimeSpan End => TimeSpan.Parse(EndTime);
        [JsonIgnore] public DateTime StartDateTime => Date + Start;
        [JsonIgnore] public DateTime EndDateTime => Date + End;

        /// <summary>True when ItemSource contains "Holiday" (simple heuristic).</summary>
        [JsonIgnore] public bool IsHoliday => ItemSource?.Contains("Holiday", StringComparison.OrdinalIgnoreCase) == true;
    }
}