// Helpers/CalendarHelpers.cs
// Extend flattening helpers so consumers can enrich either a single CalendarEntry
// or a list of CalendarEntry objects with attendee information.
// Existing Month → IEnumerable<CalendarEntryWithAttendees> helper retained.

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
        /*------------------------------------------------------------
         * Basic flatten: MonthDto → IEnumerable<CalendarEntry>
         *-----------------------------------------------------------*/
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

        /*------------------------------------------------------------
         * Enrich MONTH with attendees (unchanged signature)
         *-----------------------------------------------------------*/
        public static async Task<IEnumerable<CalendarEntryWithAttendees>> FlattenWithAttendeesAsync(
            this CalendarMonthDto month,
            CoziClient client,
            CancellationToken ct = default)
        {
            var allPeople = await client.GetPeopleAsync(ct).ConfigureAwait(false);
            var tasks = new List<Task<CalendarEntryWithAttendees>>();

            foreach (var entry in Flatten(month))
                tasks.Add(entry.WithAttendeesAsync(client, allPeople, ct));

            return await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        /*------------------------------------------------------------
         * NEW: Single CalendarEntry → CalendarEntryWithAttendees
         *-----------------------------------------------------------*/
        public static async Task<CalendarEntryWithAttendees> WithAttendeesAsync(
            this CalendarEntry entry,
            CoziClient client,
            IReadOnlyList<PersonDto>? cachedPeople = null,
            CancellationToken ct = default)
        {
            cachedPeople ??= await client.GetPeopleAsync(ct).ConfigureAwait(false);

            // attendee resolution via helper (AttendeeHelpers extension)
            var attendees = entry.Item.GetAttendeesFromExtra(cachedPeople);
            return new CalendarEntryWithAttendees(entry.Date, entry.Item, attendees);
        }

        /*------------------------------------------------------------
         * NEW: IEnumerable<CalendarEntry> → IEnumerable<CalendarEntryWithAttendees>
         *-----------------------------------------------------------*/
        public static async Task<IEnumerable<CalendarEntryWithAttendees>> WithAttendeesAsync(
            this IEnumerable<CalendarEntry> entries,
            CoziClient client,
            CancellationToken ct = default)
        {
            if (entries is null) throw new ArgumentNullException(nameof(entries));
            var list = entries.ToList();
            var allPeople = await client.GetPeopleAsync(ct).ConfigureAwait(false);

            var tasks = list.Select(e => e.WithAttendeesAsync(client, allPeople, ct));
            return await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        /*------------------------------------------------------------
         * Debug helper
         *-----------------------------------------------------------*/
        public static void PrintFull(CalendarEntry entry)
        {
            var opts = new System.Text.Json.JsonSerializerOptions { WriteIndented = true };
            string json = System.Text.Json.JsonSerializer.Serialize(entry.Item, opts);

            Console.WriteLine($"Date: {entry.Date:yyyy-MM-dd}");
            Console.WriteLine(json);
            Console.WriteLine();
        }
    }

    public sealed record CalendarEntry(DateTime Date, CalendarItemDto Item);

    public sealed record CalendarEntryWithAttendees(
        DateTime Date,
        CalendarItemDto Item,
        IReadOnlyList<PersonDto> Attendees);
}
