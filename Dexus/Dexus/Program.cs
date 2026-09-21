using Dexus;
using Dexus.Authentication;
using Dexus.Components;
using MudBlazor.Services;
using DexusCore;
using Dexus.Client.UIServices;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using System.Net;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddKeyPerFile("/run/secrets", optional: true);
bool disableHttpsRedirection = builder.Configuration.GetValue<bool>("Dexus:DisableHttpsRedirection");

builder.WebHost.ConfigureKestrel(options =>
{
    options.AddServerHeader = false;
    options.Limits.MaxRequestBodySize = 64 * 1024;
    options.Limits.MaxRequestBufferSize = 64 * 1024;
    options.Limits.MaxRequestHeaderCount = 64;
    options.Limits.MaxRequestHeadersTotalSize = 16 * 1024;
    options.Limits.MaxRequestLineSize = 8 * 1024;
});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.ForwardLimit = 1;

    foreach (string configuredProxy in builder.Configuration.GetSection("Dexus:KnownProxies").Get<string[]>() ?? [])
    {
        if (!IPAddress.TryParse(configuredProxy, out IPAddress? proxyAddress))
        {
            throw new InvalidOperationException($"Dexus:KnownProxies contains an invalid IP address: '{configuredProxy}'.");
        }

        options.KnownProxies.Add(proxyAddress);
    }
});

builder.Services.AddDataProtection().SetApplicationName("Dexus");
builder.Services.AddBungieSessions();
builder.Services.AddAntiforgery(options =>
{
    options.Cookie.Name = "__Host-Dexus.Antiforgery";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.SuppressXFrameOptionsHeader = true;
});
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 300,
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1),
            }));
});

builder.Services.AddDexusServices();

var app = builder.Build();

app.UseForwardedHeaders();

app.Use(async (context, next) =>
{
    IHeaderDictionary headers = context.Response.Headers;
    headers.XContentTypeOptions = "nosniff";
    headers.XFrameOptions = "DENY";
    headers["Referrer-Policy"] = "no-referrer";
    headers["Permissions-Policy"] = "accelerometer=(), camera=(), geolocation=(), microphone=(), payment=(), usb=()";
    headers.ContentSecurityPolicy = "base-uri 'self'; frame-ancestors 'none'; object-src 'none'";

    if (context.Request.Path.Equals("/callback", StringComparison.OrdinalIgnoreCase)
        || context.Request.Path.StartsWithSegments("/auth")
        || context.Request.Path.StartsWithSegments("/DexusAccount")
        || context.Request.Path.StartsWithSegments("/DexusVaultManager"))
    {
        headers.CacheControl = "no-store";
    }

    await next();
});

app.UseRateLimiter();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

if (!disableHttpsRedirection)
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();
app.MapBungieAuthentication();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(
        typeof(Dexus.Client._Imports).Assembly,
        typeof(Dexus.Demo.DemoAssemblyMarker).Assembly);

app.Run();
