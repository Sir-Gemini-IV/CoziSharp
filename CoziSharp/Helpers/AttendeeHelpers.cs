using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CoziSharp.Models;

namespace CoziSharp.Helpers
{
    public static class AttendeeHelpers
    {
        public static async Task<List<PersonDto>> GetAttendeesAsync(
                this CoziClient client, CalendarItemDto stubItem, CancellationToken ct = default)
        {
            // Skip holidays / Cozi items
            if (stubItem.ItemSource?.Contains("Cozi", StringComparison.OrdinalIgnoreCase) == true)
                return new List<PersonDto>();

            var full = await client.TryGetCalendarItemAsync(stubItem.Id, ct).ConfigureAwait(false);
            if (full?.AttendeeSet == null || full.AttendeeSet.Count == 0)
                return new List<PersonDto>();

            var people = await client.GetPeopleAsync(ct).ConfigureAwait(false);

            return people
                .Where(p => full.AttendeeSet.Contains(p.AccountPersonId))
                .ToList();
        }

        public static List<PersonDto> GetAttendeesFromExtra(this CalendarItemDto item, IReadOnlyList<PersonDto> allPeople)
        {
            if (item.Extra == null || !item.Extra.TryGetValue("householdMembers", out var householdMembersEl))
                return new List<PersonDto>();

            if (householdMembersEl.ValueKind != JsonValueKind.Array)
                return new List<PersonDto>();

            var memberIds = new List<string>();
            foreach (var el in householdMembersEl.EnumerateArray())
            {
                // assuming the array contains strings of person IDs
                if (el.ValueKind == JsonValueKind.String)
                    memberIds.Add(el.GetString()!);
                else if (el.ValueKind == JsonValueKind.Object && el.TryGetProperty("id", out var idEl))
                    memberIds.Add(idEl.GetString()!);
            }

            return allPeople.Where(p => memberIds.Contains(p.AccountPersonId)).ToList();
        }
    }
}
