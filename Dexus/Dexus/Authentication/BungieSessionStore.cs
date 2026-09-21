using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ClientProfile;

namespace Dexus.Authentication;

// One process only: credentials never enter the browser or a shared disk file.
public sealed class BungieSessionStore(TimeProvider clock) : BackgroundService
{
    public const string SessionClaim = "dexus:session";
    public static readonly TimeSpan SessionLifetime = TimeSpan.FromHours(8);
    private readonly object sync = new();
    private readonly Dictionary<string, Attempt> attempts = new();
    private readonly Dictionary<string, Session> sessions = new();
    private const int Capacity = 2048;

    public string? BeginAttempt(string browserBinding)
    {
        lock (sync)
        {
            Prune();
            if (attempts.Count >= Capacity) return null;
            string state = NewSecret();
            attempts.Add(Hash(state), new(Hash(browserBinding), clock.GetUtcNow().AddMinutes(10)));
            return state;
        }
    }

    public bool ConsumeAttempt(string? state, string? browserBinding)
    {
        if (state?.Length != 64 || browserBinding?.Length != 64) return false;
        lock (sync)
        {
            Prune();
            string key = Hash(state);
            if (!attempts.TryGetValue(key, out var attempt)
                || !CryptographicOperations.FixedTimeEquals(
                    Encoding.ASCII.GetBytes(attempt.BrowserHash), Encoding.ASCII.GetBytes(Hash(browserBinding))))
                return false;
            return attempts.Remove(key);
        }
    }

    public (string Id, DateTimeOffset ExpiresUtc)? Create(Profile profile)
    {
        if (!profile.Create() || !profile.LoadDestinyID()) return null;
        lock (sync)
        {
            Prune();
            if (sessions.Count >= Capacity) return null;
            string id = NewSecret();
            DateTimeOffset expires = clock.GetUtcNow() + SessionLifetime;
            profile.SetSessionExpiry(expires);
            sessions.Add(id, new(profile, expires));
            return (id, expires);
        }
    }

    public Profile? Find(ClaimsPrincipal principal)
    {
        if (principal.Identity?.IsAuthenticated != true) return null;
        string? id = principal.FindFirstValue(SessionClaim);
        lock (sync)
        {
            Prune();
            return id is not null && sessions.TryGetValue(id, out var session) ? session.Profile : null;
        }
    }

    public void Revoke(ClaimsPrincipal principal)
    {
        if (principal.Identity?.IsAuthenticated != true) return;
        string? id = principal.FindFirstValue(SessionClaim);
        lock (sync)
        {
            if (id is not null && sessions.Remove(id, out var session)) session.Profile.ThrowOut();
        }
    }

    public static string NewSecret() => Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
    private static string Hash(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));

    private void Prune()
    {
        DateTimeOffset now = clock.GetUtcNow();
        foreach (var pair in attempts.Where(p => p.Value.ExpiresUtc <= now).ToArray()) attempts.Remove(pair.Key);
        foreach (var pair in sessions.Where(p => p.Value.ExpiresUtc <= now || !p.Value.Profile.IsActive).ToArray())
        {
            pair.Value.Profile.ThrowOut();
            sessions.Remove(pair.Key);
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));
        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                lock (sync) Prune();
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { }
    }

    private sealed record Attempt(string BrowserHash, DateTimeOffset ExpiresUtc);
    private sealed record Session(Profile Profile, DateTimeOffset ExpiresUtc);
}
