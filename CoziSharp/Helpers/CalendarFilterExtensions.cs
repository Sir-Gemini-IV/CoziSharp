using CoziSharp.Models;

namespace CoziSharp.Helpers
{
    public static class CalendarFilterExtensions
    {
        /* Generic predicate overload --------------------------------------- */
        public static IEnumerable<CalendarEntry> Where(this IEnumerable<CalendarEntry> src,
                                                       Func<CalendarItemDto, bool> predicate)
            => src.Where(e => predicate(e.Item));

        /* Id ---------------------------------------------------------------- */
        public static IEnumerable<CalendarEntry> WhereIdEquals(this IEnumerable<CalendarEntry> src, string id)
            => src.Where(e => string.Equals(e.Item.Id, id, StringComparison.Ordinal));

        /* ItemType ---------------------------------------------------------- */
        public static IEnumerable<CalendarEntry> WhereItemTypeEquals(this IEnumerable<CalendarEntry> src, string itemType, StringComparison cmp = StringComparison.OrdinalIgnoreCase)
            => src.Where(e => string.Equals(e.Item.ItemType, itemType, cmp));

        public static IEnumerable<CalendarEntry> WhereItemTypeContains(this IEnumerable<CalendarEntry> src, string substring, StringComparison cmp = StringComparison.OrdinalIgnoreCase)
            => src.Where(e => e.Item.ItemType?.IndexOf(substring, cmp) >= 0);

        /* Description ------------------------------------------------------- */
        public static IEnumerable<CalendarEntry> WhereDescriptionContains(this IEnumerable<CalendarEntry> src, string substring, StringComparison cmp = StringComparison.OrdinalIgnoreCase)
            => src.Where(e => e.Item.Description?.IndexOf(substring, cmp) >= 0 ||
                               e.Item.DescriptionShort?.IndexOf(substring, cmp) >= 0);

        /* ItemSource -------------------------------------------------------- */
        public static IEnumerable<CalendarEntry> WhereItemSourceContains(this IEnumerable<CalendarEntry> src, string substring, StringComparison cmp = StringComparison.OrdinalIgnoreCase)
            => src.Where(e => e.Item.ItemSource?.IndexOf(substring, cmp) >= 0);

        public static IEnumerable<CalendarEntry> WhereItemSourceDoesNotContain(this IEnumerable<CalendarEntry> src, string substring, StringComparison cmp = StringComparison.OrdinalIgnoreCase)
            => src.Where(e => e.Item.ItemSource == null || e.Item.ItemSource.IndexOf(substring, cmp) < 0);

        /* Dates ------------------------------------------------------------- */
        public static IEnumerable<CalendarEntry> WhereDateEquals(this IEnumerable<CalendarEntry> src, DateTime date)
            => src.Where(e => e.Date.Date == date.Date);

        public static IEnumerable<CalendarEntry> WhereDateOnOrAfter(this IEnumerable<CalendarEntry> src, DateTime from)
            => src.Where(e => e.Date.Date >= from.Date);

        public static IEnumerable<CalendarEntry> WhereDateBefore(this IEnumerable<CalendarEntry> src, DateTime toExclusive)
            => src.Where(e => e.Date.Date < toExclusive.Date);

        public static IEnumerable<CalendarEntry> WhereDateBetween(this IEnumerable<CalendarEntry> src, DateTime fromInclusive, DateTime toExclusive)
            => src.Where(e => e.Date.Date >= fromInclusive.Date && e.Date.Date < toExclusive.Date);

        /* Location + Notes -------------------------------------------------- */
        public static IEnumerable<CalendarEntry> WhereLocationContains(this IEnumerable<CalendarEntry> src, string substring, StringComparison cmp = StringComparison.OrdinalIgnoreCase)
            => src.Where(e => e.Item.Details?.Location?.IndexOf(substring, cmp) >= 0);

        public static IEnumerable<CalendarEntry> WhereNotesContains(this IEnumerable<CalendarEntry> src, string substring, StringComparison cmp = StringComparison.OrdinalIgnoreCase)
            => src.Where(e => e.Item.Details?.Notes?.IndexOf(substring, cmp) >= 0);

        /* Holiday / ReadOnly / DateSpan etc. -------------------------------- */
        public static IEnumerable<CalendarEntry> WhereIsHoliday(this IEnumerable<CalendarEntry> src)
            => src.Where(e => e.Item.IsHoliday);

        public static IEnumerable<CalendarEntry> WhereIsNotHoliday(this IEnumerable<CalendarEntry> src)
            => src.Where(e => !e.Item.IsHoliday);

        public static IEnumerable<CalendarEntry> WhereReadOnly(this IEnumerable<CalendarEntry> src, bool readOnly = true)
            => src.Where(e => (e.Item.Details?.ReadOnly ?? false) == readOnly);

        public static IEnumerable<CalendarEntry> WhereDateSpanEquals(this IEnumerable<CalendarEntry> src, int span)
            => src.Where(e => e.Item.DateSpan == span);

        /* Composite helper -------------------------------------------------- */
        public static IEnumerable<CalendarEntry> WhereItemSourceNotContains(this IEnumerable<CalendarEntry> src, string substring, StringComparison cmp = StringComparison.OrdinalIgnoreCase)
            => src.WhereItemSourceDoesNotContain(substring, cmp);
    }
}
