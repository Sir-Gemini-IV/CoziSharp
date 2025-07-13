// AuthToken.cs
using System;

namespace CoziSharp.Auth
{
    /// <summary>
    /// Represents a bearer token with expiry info.
    /// </summary>
    public sealed record AuthToken(string Value, DateTimeOffset Expiry)
    {
        /// <summary>
        /// Indicates whether the token is expired or about to expire (within 1 minute).
        /// </summary>
        public bool IsExpired => DateTimeOffset.UtcNow >= Expiry - TimeSpan.FromMinutes(1);
    }
}
