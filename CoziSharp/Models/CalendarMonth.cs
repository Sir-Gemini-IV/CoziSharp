using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CoziSharp.Models
{
    public sealed class CalendarMonth
    {
        [JsonPropertyName("appointments")]
        public List<AppointmentDto> Appointments { get; set; } = new();
    }
}
