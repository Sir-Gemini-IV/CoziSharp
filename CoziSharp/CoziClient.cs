using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Polly;
using Polly.Retry;
using CoziSharp.Models;
using CoziSharp.Helpers;

namespace CoziSharp
{
    public interface IReadOnlyCoziClient : IAsyncDisposable
    {
        Task LoginAsync(string username, string password, CancellationToken ct = default);

        Task<IReadOnlyList<ListDto>> GetListsAsync(CancellationToken ct = default);
        Task<ListDto> GetListAsync(string listId, CancellationToken ct = default);
        Task<CalendarMonthDto> GetCalendarMonthAsync(int year, int month, CancellationToken ct = default);
        Task<string> GetCalendarMonthRawAsync(int year, int month, CancellationToken ct = default);
        Task<IReadOnlyList<PersonDto>> GetPeopleAsync(CancellationToken ct = default);

        // new aggregations
        Task<List<CalendarEntry>> GetCalendarYearAsync(int year, CancellationToken ct = default);
        Task<List<CalendarEntry>> GetCalendarWeekAsync(DateTime dateInWeek, CancellationToken ct = default);
        Task<List<CalendarEntry>> GetCalendarDayAsync(DateTime date, CancellationToken ct = default);

        Task<CalendarItemDto> GetCalendarItemAsync(string itemId, CancellationToken ct = default);
        Task<string> GetCalendarItemRawAsync(string itemId, CancellationToken ct = default);
    }

    public sealed class CoziClient : IReadOnlyCoziClient
    {
        private readonly HttpClient _http;
        private readonly AsyncRetryPolicy<HttpResponseMessage> _retry;

        private string? _username;
        private string? _password;

        private AuthToken? _token;
        private string? _accountId;

        private static readonly Uri _baseAddress = new("https://rest.cozi.com/");

        public CoziClient(HttpClient http)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
            _http.BaseAddress = _baseAddress;
            _http.DefaultRequestHeaders.UserAgent.ParseAdd("CoziSharp/0.6 (read-only)");

            _retry = Policy<HttpResponseMessage>
                .Handle<HttpRequestException>()
                .OrResult(r => (int)r.StatusCode >= 500)
                .WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)));
        }

        #region Public API

        public async Task LoginAsync(string username, string password, CancellationToken ct = default)
        {
            _username = username ?? throw new ArgumentNullException(nameof(username));
            _password = password ?? throw new ArgumentNullException(nameof(password));
            await AuthenticateAsync(ct).ConfigureAwait(false);
        }

        public async Task<IReadOnlyList<ListDto>> GetListsAsync(CancellationToken ct = default)
        {
            EnsureLoggedIn();
            var url = $"api/ext/2004/{_accountId}/list/";
            return await SendAsync<List<ListDto>>(() => _http.GetAsync(url, ct), ct).ConfigureAwait(false);
        }

        public async Task<ListDto> GetListAsync(string listId, CancellationToken ct = default)
        {
            EnsureLoggedIn();
            var url = $"api/ext/2004/{_accountId}/list/{listId}";
            return await SendAsync<ListDto>(() => _http.GetAsync(url, ct), ct).ConfigureAwait(false);
        }

        public async Task<CalendarMonthDto> GetCalendarMonthAsync(int year, int month, CancellationToken ct = default)
        {
            EnsureLoggedIn();
            ValidateMonth(month);
            var url = $"api/ext/2004/{_accountId}/calendar/{year:D4}/{month:D2}";
            return await SendAsync<CalendarMonthDto>(() => _http.GetAsync(url, ct), ct).ConfigureAwait(false);
        }

        public async Task<string> GetCalendarMonthRawAsync(int year, int month, CancellationToken ct = default)
        {
            EnsureLoggedIn();
            ValidateMonth(month);
            var url = $"api/ext/2004/{_accountId}/calendar/{year:D4}/{month:D2}";

            if (_token is { IsExpired: true })
                await AuthenticateAsync(ct).ConfigureAwait(false);

            var resp = await _retry.ExecuteAsync(() => _http.GetAsync(url, ct)).ConfigureAwait(false);

            if (resp.StatusCode == HttpStatusCode.Unauthorized)
            {
                await AuthenticateAsync(ct).ConfigureAwait(false);
                resp = await _http.GetAsync(url, ct);
            }

            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadAsStringAsync(ct);
        }

        public async Task<IReadOnlyList<PersonDto>> GetPeopleAsync(CancellationToken ct = default)
        {
            EnsureLoggedIn();
            var url = $"api/ext/2004/{_accountId}/account/person/";
            return await SendAsync<List<PersonDto>>(() => _http.GetAsync(url, ct), ct).ConfigureAwait(false);
        }

        /// <summary>
        /// Get a single calendar item with automatic fallback between API versions 2004 and 2207.
        /// </summary>
        public async Task<CalendarItemDto> GetCalendarItemAsync(string itemId, CancellationToken ct = default)
        {
            EnsureLoggedIn();

            var url2004 = $"api/ext/2004/{_accountId}/calendar/item/{itemId}";
            var url2207 = $"api/ext/2207/{_accountId}/calendar/item/{itemId}";

            if (_token is { IsExpired: true })
                await AuthenticateAsync(ct).ConfigureAwait(false);

            HttpResponseMessage resp = await _retry.ExecuteAsync(() => _http.GetAsync(url2004, ct)).ConfigureAwait(false);

            if (resp.StatusCode == HttpStatusCode.NotFound)
            {
                // fallback to 2207
                resp = await _retry.ExecuteAsync(() => _http.GetAsync(url2207, ct)).ConfigureAwait(false);
            }
            else if (resp.StatusCode == HttpStatusCode.Unauthorized)
            {
                await AuthenticateAsync(ct).ConfigureAwait(false);
                resp = await _http.GetAsync(url2004, ct);
            }

            if (resp.StatusCode == HttpStatusCode.NotFound)
                throw new InvalidOperationException($"Calendar item '{itemId}' not found in either 2004 or 2207 API.");

            resp.EnsureSuccessStatusCode();

            var item = await resp.Content.ReadFromJsonAsync<CalendarItemDto>(ct).ConfigureAwait(false);
            if (item == null) throw new InvalidOperationException("Unexpected null calendar item content.");

            return item;
        }

        /// <summary>
        /// Get raw JSON for a calendar item with automatic fallback between API versions 2004 and 2207.
        /// </summary>
        public async Task<string> GetCalendarItemRawAsync(string itemId, CancellationToken ct = default)
        {
            EnsureLoggedIn();

            var url2004 = $"api/ext/2004/{_accountId}/calendar/item/{itemId}";
            var url2207 = $"api/ext/2207/{_accountId}/calendar/item/{itemId}";

            if (_token is { IsExpired: true })
                await AuthenticateAsync(ct).ConfigureAwait(false);

            var resp = await _retry.ExecuteAsync(() => _http.GetAsync(url2004, ct)).ConfigureAwait(false);

            if (resp.StatusCode == HttpStatusCode.NotFound)
            {
                resp = await _retry.ExecuteAsync(() => _http.GetAsync(url2207, ct)).ConfigureAwait(false);
            }
            else if (resp.StatusCode == HttpStatusCode.Unauthorized)
            {
                await AuthenticateAsync(ct).ConfigureAwait(false);
                resp = await _http.GetAsync(url2004, ct);
            }

            if (resp.StatusCode == HttpStatusCode.NotFound)
                throw new InvalidOperationException($"Calendar item '{itemId}' not found in either 2004 or 2207 API.");

            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
        }

        // ------------------- Aggregation helpers ----------------------------

        public async Task<List<CalendarEntry>> GetCalendarYearAsync(int year, CancellationToken ct = default)
        {
            var list = new List<CalendarEntry>();
            for (int m = 1; m <= 12; m++)
            {
                var dto = await GetCalendarMonthAsync(year, m, ct).ConfigureAwait(false);
                list.AddRange(CalendarHelpers.Flatten(dto));
            }
            return list;
        }

        public async Task<List<CalendarEntry>> GetCalendarWeekAsync(DateTime dateInWeek, CancellationToken ct = default)
        {
            DateTime monday = dateInWeek.Date.AddDays(-(int)dateInWeek.DayOfWeek + (int)DayOfWeek.Monday);
            DateTime sundayExclusive = monday.AddDays(7);

            var months = new HashSet<int> { monday.Month, sundayExclusive.Month };
            var entries = new List<CalendarEntry>();
            foreach (int month in months)
            {
                var dto = await GetCalendarMonthAsync(monday.Year, month, ct).ConfigureAwait(false);
                entries.AddRange(CalendarHelpers.Flatten(dto));
            }
            return entries.Where((CalendarEntry e) => e.Date >= monday && e.Date < sundayExclusive).ToList();
        }

        public async Task<List<CalendarEntry>> GetCalendarDayAsync(DateTime date, CancellationToken ct = default)
        {
            var dto = await GetCalendarMonthAsync(date.Year, date.Month, ct).ConfigureAwait(false);
            return CalendarHelpers.Flatten(dto).Where((CalendarEntry e) => e.Date.Date == date.Date).ToList();
        }

        public ValueTask DisposeAsync()
        {
            _http.Dispose();
            return ValueTask.CompletedTask;
        }

        #endregion

        #region Internals

        private async Task AuthenticateAsync(CancellationToken ct)
        {
            var payload = new { username = _username!, password = _password!, issueRefresh = true };
            var resp = await _http.PostAsJsonAsync("api/ext/2207/auth/login", payload, cancellationToken: ct).ConfigureAwait(false);
            resp.EnsureSuccessStatusCode();

            var json = await resp.Content.ReadFromJsonAsync<AuthResponse>(ct) ?? throw new InvalidOperationException("Empty auth response");
            _token = new AuthToken(json.accessToken, DateTimeOffset.UtcNow.AddSeconds(json.expiresIn));
            _accountId = json.accountId;

            _http.DefaultRequestHeaders.Authorization = new("Bearer", _token.Value);
        }

        private void EnsureLoggedIn()
        {
            if (_token == null || _token.IsExpired)
                throw new InvalidOperationException("Client is not authenticated – call LoginAsync first.");
        }

        private static void ValidateMonth(int month)
        {
            if (month < 1 || month > 12) throw new ArgumentOutOfRangeException(nameof(month));
        }

        private async Task<T> SendAsync<T>(Func<Task<HttpResponseMessage>> sender, CancellationToken ct)
        {
            if (_token is { IsExpired: true })
                await AuthenticateAsync(ct).ConfigureAwait(false);

            var resp = await _retry.ExecuteAsync(sender).ConfigureAwait(false);

            if (resp.StatusCode == HttpStatusCode.Unauthorized)
            {
                await AuthenticateAsync(ct).ConfigureAwait(false);
                resp = await sender();
            }

            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<T>(ct) ?? throw new InvalidOperationException("Unexpected null content");
        }

        #endregion

        #region Private DTOs / Records

        private sealed class AuthResponse
        {
            public string accessToken { get; set; } = string.Empty;
            public int expiresIn { get; set; }
            public string accountId { get; set; } = string.Empty;
        }

        private sealed record AuthToken(string Value, DateTimeOffset Expiry)
        {
            public bool IsExpired => DateTimeOffset.UtcNow >= Expiry - TimeSpan.FromMinutes(1);
        }

        #endregion
    }
}
