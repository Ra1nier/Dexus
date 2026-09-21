using System.Security.Claims;
using ClientProfile;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.WebUtilities;
using OAuthService;
using RequestManager;

namespace Dexus.Authentication;

public static class BungieAuthentication
{
    public const string Scheme = "DexusSession";
    private const string BindingCookie = "__Host-Dexus.OAuth";
    private const string AccountPage = "/DexusAccount?goToTab=2";

    public static IServiceCollection AddBungieSessions(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<BungieSessionStore>();
        services.AddHostedService(sp => sp.GetRequiredService<BungieSessionStore>());
        services.AddAuthentication(Scheme).AddCookie(Scheme, options =>
        {
            options.Cookie.Name = "__Host-Dexus.Session";
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Cookie.Path = "/";
            options.ExpireTimeSpan = BungieSessionStore.SessionLifetime;
            options.SlidingExpiration = false;
            options.LoginPath = "/auth/bungie/start";
            options.Events.OnValidatePrincipal = async context =>
            {
                var store = context.HttpContext.RequestServices.GetRequiredService<BungieSessionStore>();
                if (context.Principal is null || store.Find(context.Principal) is null)
                {
                    context.RejectPrincipal();
                    await context.HttpContext.SignOutAsync(Scheme);
                }
            };
        });
        services.AddAuthorization();
        services.AddCascadingAuthenticationState();
        return services;
    }

    public static IEndpointRouteBuilder MapBungieAuthentication(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/auth/bungie/start", Start);
        // Retain the redirect URL already registered with Bungie.
        endpoints.MapGet("/callback", Callback);
        endpoints.MapPost("/auth/logout", Logout);
        return endpoints;
    }

    private static IResult Start(HttpContext context, OAuthProfile configuration, BungieSessionStore store)
    {
        if (!context.Request.IsHttps) return Results.BadRequest("HTTPS is required for sign-in.");
        if (!configuration.HasValidAPIKeys()) return Results.Redirect(AccountPage + "&authError=configuration");
        string binding = BungieSessionStore.NewSecret();
        string? state = store.BeginAttempt(binding);
        if (state is null) return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
        context.Response.Cookies.Append(BindingCookie, binding, new CookieOptions
        {
            HttpOnly = true, Secure = true, SameSite = SameSiteMode.Lax,
            Path = "/", MaxAge = TimeSpan.FromMinutes(10), IsEssential = true,
        });
        return Results.Redirect(QueryHelpers.AddQueryString(configuration.Authorization_URL,
            new Dictionary<string, string?>
            {
                ["client_id"] = configuration.Client_ID, ["response_type"] = "code", ["state"] = state,
            }));
    }

    private static async Task<IResult> Callback(HttpContext context, OAuthProfile configuration,
        OAuthServer oauth, Profile profile, RequestServer requests, BungieSessionStore store)
    {
        if (!context.Request.IsHttps) return Results.BadRequest("HTTPS is required for sign-in.");
        var query = context.Request.Query;
        string? state = query["state"].Count == 1 ? query["state"][0] : null;
        string? binding = context.Request.Cookies[BindingCookie];
        context.Response.Cookies.Delete(BindingCookie, new CookieOptions { Secure = true, HttpOnly = true, Path = "/", SameSite = SameSiteMode.Lax });
        // Atomic consumption precedes every code exchange, including failed exchanges.
        if (!store.ConsumeAttempt(state, binding)) return Results.Redirect(AccountPage + "&authError=state");
        if (query.ContainsKey("error")) return Results.Redirect(AccountPage + "&authError=denied");
        string? code = query["code"].Count == 1 ? query["code"][0] : null;
        if (!configuration.HasValidAPIKeys() || string.IsNullOrWhiteSpace(code) || code.Length > 4096)
            return Results.Redirect(AccountPage + "&authError=code");

        string tokens = await oauth.StartOAuthProcessAsync(code);
        if (!profile.StoreUserTokens(tokens) || string.IsNullOrWhiteSpace(profile.MembershipID))
            return Results.Redirect(AccountPage + "&authError=exchange");
        string[] membership = await requests.PlatformDestiny2_Request_MembershipIDAsync();
        if (membership[0] == "Error" || !profile.StoreDestinyMembershipID(membership[0], membership[1]))
        {
            profile.ThrowOut();
            return Results.Redirect(AccountPage + "&authError=membership");
        }

        var session = store.Create(profile);
        if (session is null)
        {
            profile.ThrowOut();
            return Results.Redirect(AccountPage + "&authError=capacity");
        }
        var identity = new ClaimsIdentity(new[] { new Claim(BungieSessionStore.SessionClaim, session.Value.Id) }, Scheme);
        var principal = new ClaimsPrincipal(identity);
        try
        {
            // Replacing an existing login invalidates its already-open circuits as well.
            store.Revoke(context.User);
            await context.SignInAsync(Scheme, principal, new AuthenticationProperties
            {
                IsPersistent = false, ExpiresUtc = session.Value.ExpiresUtc, AllowRefresh = false,
            });
        }
        catch
        {
            store.Revoke(principal);
            throw;
        }
        return Results.Redirect(AccountPage);
    }

    private static async Task<IResult> Logout(HttpContext context, IAntiforgery antiforgery, BungieSessionStore store)
    {
        try { await antiforgery.ValidateRequestAsync(context); }
        catch (AntiforgeryValidationException) { return Results.BadRequest(); }
        store.Revoke(context.User);
        await context.SignOutAsync(Scheme);
        return Results.Redirect(AccountPage);
    }
}
