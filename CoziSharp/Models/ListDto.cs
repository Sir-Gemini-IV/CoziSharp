// ListDto.cs
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CoziSharp.Models
{
    /// <summary>
    /// Represents a Cozi shopping or to-do list.
    /// </summary>
    public sealed class ListDto
    {
        [JsonPropertyName("listId")]
        public string ListId { get; set; } = string.Empty;

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("listType")]
        public string ListType { get; set; } = string.Empty;

        [JsonPropertyName("version")]
        public int Version { get; set; }

        [JsonPropertyName("items")]
        public List<ListItemDto> Items { get; set; } = new();
    }

    /// <summary>
    /// Represents a single item in a Cozi list.
    /// </summary>
    public sealed class ListItemDto
    {
        [JsonPropertyName("itemId")]
        public string ItemId { get; set; } = string.Empty;

        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;

        [JsonPropertyName("checkedOff")]
        public bool CheckedOff { get; set; }
    }
}
