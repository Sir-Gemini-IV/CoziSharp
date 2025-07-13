// Endpoints.cs
namespace CoziSharp.Api
{
    internal static class Endpoints
    {
        public const string BaseUrl = "https://rest.cozi.com/";

        // Authentication endpoint
        public const string Login = "api/ext/2207/auth/login";

        // Lists
        public static string Lists(string accountId) => $"api/ext/2004/{accountId}/list/";
        public static string ListById(string accountId, string listId) => $"api/ext/2004/{accountId}/list/{listId}";

        // Calendar
        public static string CalendarMonth(string accountId, int year, int month) =>
            $"api/ext/2004/{accountId}/calendar/{year:D4}/{month:D2}";

        // People
        public static string People(string accountId) => $"api/ext/2004/{accountId}/account/person/";

        // Add other endpoints as needed...
    }
}
