using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Polly;
using Polly.Retry;
using CoziSharp.Auth;
using CoziSharp.Api;
using CoziSharp.Models;

namespace CoziSharp
{
    public interface IReadOnlyCoziClient : IAsyncDisposable
    {
        Task LoginAsync(string username, string password, CancellationToken ct = default);

        Task<IReadOnlyList<ListDto>> GetListsAsync(CancellationToken ct = default);
        Task<ListDto> GetListAsync(string listId, CancellationToken ct = default);

        Task<CalendarMonth> GetCalendarMonthAsync(int year, int month, CancellationToken ct = default);
        Task<IReadOnlyList<PersonDto>> GetPeopleAsync(CancellationToken ct = default);
    }

    public sealed class CoziClient : IReadOnlyCoziClient
    {
        private readonly HttpClient _http;
        private readonly AsyncRetryPolicy<HttpResponseMessage> _retry;

        private string? _username;
        private string? _password;

        private AuthToken? _token;
        private string? _accountId;

        public CoziClient(HttpClient http)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
            _http.BaseAddress = new Uri(Endpoints.BaseUrl);
            _http.DefaultRequestHeaders.UserAgent.ParseAdd("CoziSharp/0.2 (read-only)");

            _retry = Policy<HttpResponseMessage>
                .Handle<HttpRequestException>()
                .OrResult(r => (int)r.StatusCode >= 500)
                .WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)));
        }

        #region IReadOnlyCoziClient

        public async Task LoginAsync(string username, string password, CancellationToken ct = default)
        {
            _username = username ?? throw new ArgumentNullException(nameof(username));
            _password = password ?? throw new ArgumentNullException(nameof(password));

            await AuthenticateAsync(ct).ConfigureAwait(false);
        }

        public async Task<IReadOnlyList<ListDto>> GetListsAsync(CancellationToken ct = default)
        {
            EnsureLoggedIn();
            var url = Endpoints.Lists(_accountId!);
            return await SendAsync<List<ListDto>>(() => _http.GetAsync(url, ct), ct).ConfigureAwait(false);
        }

        public async Task<ListDto> GetListAsync(string listId, CancellationToken ct = default)
        {
            EnsureLoggedIn();
            var url = Endpoints.ListById(_accountId!, listId);
            return await SendAsync<ListDto>(() => _http.GetAsync(url, ct), ct).ConfigureAwait(false);
        }

        public async Task<CalendarMonth> GetCalendarMonthAsync(int year, int month, CancellationToken ct = default)
        {
            EnsureLoggedIn();
            if (month < 1 || month > 12) throw new ArgumentOutOfRangeException(nameof(month));

            var url = Endpoints.CalendarMonth(_accountId!, year, month);
            return await SendAsync<CalendarMonth>(() => _http.GetAsync(url, ct), ct).ConfigureAwait(false);
        }

        public async Task<IReadOnlyList<PersonDto>> GetPeopleAsync(CancellationToken ct = default)
        {
            EnsureLoggedIn();
            var url = Endpoints.People(_accountId!);
            return await SendAsync<List<PersonDto>>(() => _http.GetAsync(url, ct), ct).ConfigureAwait(false);
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
            var resp = await _http.PostAsJsonAsync(Endpoints.Login, payload, cancellationToken: ct).ConfigureAwait(false);
            resp.EnsureSuccessStatusCode();

            var json = await resp.Content.ReadFromJsonAsync<AuthResponse>(ct)
                       ?? throw new InvalidOperationException("Empty auth response");

            _token = new AuthToken(json.accessToken, DateTimeOffset.UtcNow.AddSeconds(json.expiresIn));
            _accountId = json.accountId;

            _http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token.Value);
        }

        private void EnsureLoggedIn()
        {
            if (_token == null || _token.IsExpired)
                throw new InvalidOperationException("Client is not authenticated – call LoginAsync first.");
        }

        private async Task<T> SendAsync<T>(Func<Task<HttpResponseMessage>> sender, CancellationToken ct)
        {
            if (_token is { IsExpired: true })
                await AuthenticateAsync(ct).ConfigureAwait(false);

            var response = await _retry.ExecuteAsync(sender).ConfigureAwait(false);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                await AuthenticateAsync(ct).ConfigureAwait(false);
                response = await sender();
            }

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<T>(ct)
                   ?? throw new InvalidOperationException("Unexpected null content");
        }

        #endregion

        #region Private DTOs

        private sealed class AuthResponse
        {
            public string accessToken { get; set; } = string.Empty;
            public int expiresIn { get; set; }
            public string accountId { get; set; } = string.Empty;
        }

        #endregion
    }
}
