using System.Text.Json.Serialization;
using System.Text.Json;

namespace CoziSharp.Models
{
    public sealed class CalendarMonthDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        // "2025-07-14" → [ { id = "abc‑123" }, { id = "def‑456" } ]
        public Dictionary<string, List<DayRef>> Days { get; set; } = new();

        // "abc‑123" → full item data
        public Dictionary<string, CalendarItemDto> Items { get; set; } = new();
    }
}

