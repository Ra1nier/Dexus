using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using ClientProfile;
using Dexus;
using Dexus.Authentication;
using Dexus.Components;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OAuthService;
using RequestManager;

internal static class OAuthFlowChecks
{
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    public static async Task RunAsync()
    {
        var clock = new TestClock();
        var provider = new FakeBungie();
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { ApplicationName = typeof(App).Assembly.FullName });
        builder.Logging.ClearProviders();
        builder.WebHost.ConfigureKestrel(options => options.Listen(IPAddress.Loopback, 0));
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["BungieAPIKeys:API_Key"] = "test-api-key",
            ["BungieAPIKeys:Client_ID"] = "test-client",
            ["BungieAPIKeys:Client_Secret"] = "test-client-secret",
        });
        builder.Services.AddDataProtection();
        builder.Services.AddAntiforgery(options =>
        {
            options.Cookie.Name = "__Host-Dexus.Antiforgery";
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        });
        builder.Services.AddBungieSessions();
        builder.Services.AddDexusServices();
        builder.Services.AddSingleton<TimeProvider>(clock);
        builder.Services.AddScoped<Profile>();
        builder.Services.AddScoped<OAuthProfile>();
        builder.Services.AddScoped(sp => new OAuthServer(sp.GetRequiredService<ILogger<OAuthServer>>(),
            sp.GetRequiredService<OAuthProfile>(), new HttpClient(provider, disposeHandler: false)));
        builder.Services.AddScoped(sp => new RequestServer(sp.GetRequiredService<ILogger<RequestServer>>(),
            sp.GetRequiredService<Profile>(), sp.GetRequiredService<OAuthProfile>(), new HttpClient(provider, disposeHandler: false)));
        await using var app = builder.Build();
        app.UseForwardedHeaders(new ForwardedHeadersOptions { ForwardedHeaders = ForwardedHeaders.XForwardedProto });
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseAntiforgery();
        app.MapBungieAuthentication();
        app.MapRazorComponents<App>().AddInteractiveServerRenderMode().AddInteractiveWebAssemblyRenderMode()
            .AddAdditionalAssemblies(
                typeof(Dexus.Client._Imports).Assembly,
                typeof(Dexus.Demo.DemoAssemblyMarker).Assembly);
        app.MapGet("/test/session", (HttpContext context, BungieSessionStore sessions) =>
        {
            var session = sessions.Find(context.User);
            if (session is null) return Results.Unauthorized();
            // A fresh circuit binds only to the profile resolved from its authenticated principal.
            var circuit = new Profile();
            circuit.BindTo(session);
            return Results.Text(circuit.DestinyMembershipID!);
        });
        app.MapGet("/test/antiforgery", (HttpContext context, IAntiforgery antiforgery) =>
            Results.Text(antiforgery.GetAndStoreTokens(context).RequestToken!));
        await app.StartAsync();
        try
        {
            using var client = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false, UseCookies = false })
            {
                BaseAddress = new Uri(app.Urls.Single()),
            };
            var alice = new Browser(client);
            var bob = new Browser(client);
            Check((await alice.Send("/test/session")).StatusCode == HttpStatusCode.Unauthorized, "Anonymous session accepted.");
            string demoPage = await (await alice.Send("/demo")).Content.ReadAsStringAsync();
            Check(demoPage.Contains("Demo mode:")
                && demoPage.Contains("Northstar Pulse")
                && demoPage.Contains("does not connect to Bungie"),
                "Anonymous demo did not render its notice and synthetic inventory.");
            string anonymousPage = await (await alice.Send("/DexusAccount?goToTab=2")).Content.ReadAsStringAsync();
            Check(anonymousPage.Contains("Sign In with Bungie"), "Account page did not render the Bungie sign-in button.");
            string aliceState = await alice.Start();
            string aliceBinding = alice.Cookies["__Host-Dexus.OAuth"];
            string bobState = await bob.Start();
            var wrongBrowser = await bob.Send("/callback?state=" + aliceState + "&code=alice");
            Check(wrongBrowser.Headers.Location!.ToString().Contains("authError=state"), "Cross-browser state accepted.");
            Check(provider.Exchanges == 0, "Invalid state exchanged a code.");
            // Restart Bob's attempt after the rejected callback cleared his binding cookie.
            bobState = await bob.Start();
            var aliceCallback = await alice.Send("/callback?state=" + aliceState + "&code=alice");
            Check(aliceCallback.Headers.Location!.ToString() == "/DexusAccount?goToTab=2", "Alice callback failed.");
            string signedInPage = await (await alice.Send("/DexusAccount?goToTab=2")).Content.ReadAsStringAsync();
            Check(signedInPage.Contains("You are signed in with Bungie") && signedInPage.Contains("/auth/logout"),
                "Actual account page did not restore authenticated session after callback.");
            string sessionHeader = aliceCallback.Headers.GetValues("Set-Cookie").Single(v => v.StartsWith("__Host-Dexus.Session="));
            Check(sessionHeader.Contains("secure", StringComparison.OrdinalIgnoreCase)
                && sessionHeader.Contains("httponly", StringComparison.OrdinalIgnoreCase)
                && sessionHeader.Contains("samesite=lax", StringComparison.OrdinalIgnoreCase), "Session cookie flags missing.");
            Check(!sessionHeader.Contains("access-alice") && !sessionHeader.Contains("refresh-alice"), "Tokens exposed in a cookie.");
            Check((await bob.Send("/callback?state=" + bobState + "&code=bob")).Headers.Location!.ToString() == "/DexusAccount?goToTab=2", "Bob callback failed.");
            Check(await (await alice.Send("/test/session")).Content.ReadAsStringAsync() == "111", "Alice session not restored after redirect.");
            Check(await (await bob.Send("/test/session")).Content.ReadAsStringAsync() == "222", "Bob received another visitor's account.");
            Check(await (await alice.Send("/test/session")).Content.ReadAsStringAsync() == "111", "Reload lost the session.");
            alice.Cookies["__Host-Dexus.OAuth"] = aliceBinding;
            Check((await alice.Send("/callback?state=" + aliceState + "&code=alice")).Headers.Location!.ToString().Contains("authError=state"), "Replayed callback accepted.");
            Check(provider.Exchanges == 2, "A code was exchanged more than once.");

            var sessions = app.Services.GetRequiredService<BungieSessionStore>();
            var isolated = new Profile();
            isolated.StoreUserTokens("""{"access_token":"a","refresh_token":"r","expires_in":3600,"refresh_expires_in":7200}""");
            isolated.StoreDestinyMembershipID("333", "3");
            var entry = sessions.Create(isolated)!.Value;
            var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(BungieSessionStore.SessionClaim, entry.Id) }, BungieAuthentication.Scheme));
            var openCircuit = new Profile();
            openCircuit.BindTo(sessions.Find(principal)!);
            Check(sessions.Find(new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(BungieSessionStore.SessionClaim, entry.Id) }))) is null, "Unsigned identity resolved a session.");
            sessions.Revoke(principal);
            Check(openCircuit.AccessToken == "" && !openCircuit.StoreUserTokens("""{"access_token":"late","refresh_token":"late","expires_in":3600,"refresh_expires_in":7200}"""), "Logout did not revoke an existing circuit or allowed a late refresh to resurrect it.");

            Check((await alice.Send("/auth/logout", new Dictionary<string, string>())).StatusCode == HttpStatusCode.BadRequest, "Logout accepted a missing antiforgery token.");
            string csrf = await (await alice.Send("/test/antiforgery")).Content.ReadAsStringAsync();
            Check((await alice.Send("/auth/logout", new Dictionary<string, string> { ["__RequestVerificationToken"] = csrf })).StatusCode == HttpStatusCode.Redirect, "Valid logout failed.");
            Check((await alice.Send("/test/session")).StatusCode == HttpStatusCode.Unauthorized, "Logout retained account access.");
            Check(await (await bob.Send("/test/session")).Content.ReadAsStringAsync() == "222", "Alice logout affected Bob.");

            string expired = await alice.Start();
            clock.Advance(TimeSpan.FromMinutes(11));
            Check((await alice.Send("/callback?state=" + expired + "&code=expired")).Headers.Location!.ToString().Contains("authError=state"), "Expired OAuth state accepted.");
            string denied = await alice.Start();
            Check((await alice.Send("/callback?state=" + denied + "&error=access_denied")).Headers.Location!.ToString().Contains("authError=denied"), "Provider denial mishandled.");
            Check(provider.Exchanges == 2, "Expired/denied attempts exchanged a code.");
            clock.Advance(TimeSpan.FromHours(8));
            Check((await bob.Send("/test/session")).StatusCode == HttpStatusCode.Unauthorized, "Expired server session accepted.");
            Console.WriteLine("OAuth HTTP checks passed: anonymous demo, two visitors, redirect/reload, cross-browser rejection, replay, expiry, denial, cookie flags, logout CSRF, circuit revocation, non-cross-save membership, actual signed-in Razor account page.");
        }
        finally { await app.StopAsync(); }
    }

    private sealed class TestClock : TimeProvider
    {
        private DateTimeOffset now = DateTimeOffset.UtcNow;
        public override DateTimeOffset GetUtcNow() => now;
        public void Advance(TimeSpan amount) => now += amount;
    }

    private sealed class Browser(HttpClient client)
    {
        public Dictionary<string, string> Cookies { get; } = new();
        public async Task<HttpResponseMessage> Send(string path, Dictionary<string, string>? form = null)
        {
            using var request = new HttpRequestMessage(form is null ? HttpMethod.Get : HttpMethod.Post, path);
            request.Headers.Add("X-Forwarded-Proto", "https");
            if (Cookies.Count > 0) request.Headers.Add("Cookie", string.Join("; ", Cookies.Select(p => p.Key + "=" + p.Value)));
            if (form is not null) request.Content = new FormUrlEncodedContent(form);
            var response = await client.SendAsync(request);
            if (response.Headers.TryGetValues("Set-Cookie", out var headers))
            {
                foreach (string header in headers)
                {
                    string[] pair = header.Split(';')[0].Split('=', 2);
                    if (pair[1] == "") Cookies.Remove(pair[0]);
                    else Cookies[pair[0]] = pair[1];
                }
            }
            return response;
        }
        public async Task<string> Start()
        {
            using var response = await Send("/auth/bungie/start");
            Check(response.StatusCode == HttpStatusCode.Redirect, "Sign-in start did not redirect.");
            Uri url = response.Headers.Location!;
            Check(url.Host == "www.bungie.net", "Sign-in did not redirect to Bungie.");
            return QueryHelpers.ParseQuery(url.Query)["state"].ToString();
        }
    }

    private sealed class FakeBungie : HttpMessageHandler
    {
        public int Exchanges { get; private set; }
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string json;
            if (request.RequestUri!.AbsolutePath.EndsWith("/Token/"))
            {
                var form = QueryHelpers.ParseQuery(await request.Content!.ReadAsStringAsync(cancellationToken));
                string user = form["code"].ToString();
                Exchanges++;
                json = JsonSerializer.Serialize(new { access_token = "access-" + user, refresh_token = "refresh-" + user, expires_in = 3600, refresh_expires_in = 7200, membership_id = "bungie-" + user });
            }
            else
            {
                string id = request.Headers.Authorization!.Parameter == "access-alice" ? "111" : "222";
                json = JsonSerializer.Serialize(new { Response = new { profiles = new[] { new { membershipId = id, membershipType = 3, isCrossSavePrimary = false, crossSaveOverride = 0 } } } });
            }
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json, Encoding.UTF8, "application/json") };
        }
    }
}
