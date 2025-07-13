using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CoziSharp.Models
{
    /// <summary>
    /// Represents a single calendar appointment from Cozi.
    /// </summary>
    public sealed class AppointmentDto
    {
        [JsonPropertyName("itemId")]
        public string ItemId { get; set; } = string.Empty;

        // Cozi returns ISO 8601 date-time strings, so use DateTimeOffset to preserve timezone info
        [JsonPropertyName("start")]
        public DateTimeOffset Start { get; set; }

        [JsonPropertyName("end")]
        public DateTimeOffset End { get; set; }

        [JsonPropertyName("subject")]
        public string Subject { get; set; } = string.Empty;

        [JsonPropertyName("location")]
        public string Location { get; set; } = string.Empty;

        [JsonPropertyName("notes")]
        public string Notes { get; set; } = string.Empty;

        [JsonPropertyName("attendeeSet")]
        public List<string> AttendeeSet { get; set; } = new();

        // Optional / sometimes-present fields

        [JsonPropertyName("whoAdded")]
        public string? WhoAdded { get; set; }

        [JsonPropertyName("createdTime")]
        public long? CreatedUnixMs { get; set; }

        [JsonPropertyName("modifiedTime")]
        public long? ModifiedUnixMs { get; set; }

        /// <summary>
        /// Converts createdTime (Unix ms) to DateTimeOffset if present.
        /// </summary>
        [JsonIgnore]
        public DateTimeOffset? CreatedUtc =>
            CreatedUnixMs.HasValue ? DateTimeOffset.FromUnixTimeMilliseconds(CreatedUnixMs.Value) : null;

        /// <summary>
        /// Converts modifiedTime (Unix ms) to DateTimeOffset if present.
        /// </summary>
        [JsonIgnore]
        public DateTimeOffset? ModifiedUtc =>
            ModifiedUnixMs.HasValue ? DateTimeOffset.FromUnixTimeMilliseconds(ModifiedUnixMs.Value) : null;

        /// <summary>
        /// Stores any extra JSON fields without breaking deserialization.
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? Extra { get; set; }
    }
}
