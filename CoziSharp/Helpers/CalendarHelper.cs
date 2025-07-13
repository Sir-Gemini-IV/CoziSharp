using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CoziSharp.Models;

namespace CoziSharp.Helpers
{
    public static class CalendarHelpers
    {
        /// <summary>
        /// Flatten Cozi’s days/items dictionaries into a simple sequence of CalendarEntry.
        /// </summary>
        public static IEnumerable<CalendarEntry> Flatten(CalendarMonthDto month)
        {
            foreach (var (dateStr, refs) in month.Days)
            {
                if (!DateTime.TryParse(dateStr, out var date)) continue;

                foreach (var r in refs)
                {
                    if (month.Items.TryGetValue(r.Id, out var item))
                        yield return new CalendarEntry(date, item);
                }
            }
        }

        public static async Task<IEnumerable<CalendarEntryWithAttendees>> FlattenWithAttendeesAsync(this CalendarMonthDto month, CoziClient client, CancellationToken ct = default)
        {
            var entries = new List<CalendarEntryWithAttendees>();

            // Fetch all people once
            var allPeople = await client.GetPeopleAsync(ct).ConfigureAwait(false);

            foreach (var (dateStr, refs) in month.Days)
            {
                if (!DateTime.TryParse(dateStr, out var date)) continue;

                foreach (var r in refs)
                {
                    if (month.Items.TryGetValue(r.Id, out var item))
                    {
                        // Use your helper to get attendees from either AttendeeSet or Extra
                        var attendees = item.GetAttendeesFromExtra(allPeople);

                        entries.Add(new CalendarEntryWithAttendees(date, item, attendees));
                    }
                }
            }

            return entries;
        }

        public static async Task<CalendarItemDto?> TryGetCalendarItemAsync(
        this CoziClient client, string itemId, CancellationToken ct = default)
        {
            try
            {
                return await client.GetCalendarItemAsync(itemId, ct).ConfigureAwait(false);
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // Cozi returned 404 – item has no detail record
                return null;
            }
        }
    }

    public sealed record CalendarEntry(DateTime Date, CalendarItemDto Item);

    public sealed record CalendarEntryWithAttendees(DateTime Date, CalendarItemDto Item, List<PersonDto> Attendees);
}
