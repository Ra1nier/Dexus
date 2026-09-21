using Dexus.Client.UIServices;
using DexusCore;
using MudBlazor.Services;

namespace Dexus;

public static class DexusServices
{
    public static IServiceCollection AddDexusServices(this IServiceCollection services)
    {
        // Add MudBlazor services
        services.AddMudServices();

        // Add services to the container.
        var razorComponents = services.AddRazorComponents();
        razorComponents.AddInteractiveServerComponents(options =>
            {
                options.DetailedErrors = false;
                options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(1);
                options.DisconnectedCircuitMaxRetained = 20;
                options.JSInteropDefaultCallTimeout = TimeSpan.FromSeconds(30);
            })
            .AddHubOptions(options =>
            {
                options.MaximumReceiveMessageSize = 64 * 1024;
            });
        razorComponents.AddInteractiveWebAssemblyComponents();

        // Add dependencies in the server side container to allow prerendering of components. TODO : Might need to fix when implementing electron.
        services.AddHttpClient("Bungie", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
            client.MaxResponseContentBufferSize = 16 * 1024 * 1024;
        });
        services.AddScoped<ClientProfile.Profile>();
        services.AddScoped<OAuthService.OAuthProfile>();
        services.AddScoped<GuardianServices.SnackbarService>();
        services.AddScoped<GuardianServices.LoadingbarService>();
        services.AddScoped<GuardianServices.TimelineService>();
        services.AddScoped(sp => new OAuthService.OAuthServer(
            sp.GetRequiredService<ILogger<OAuthService.OAuthServer>>(),
            sp.GetRequiredService<OAuthService.OAuthProfile>(),
            sp.GetRequiredService<IHttpClientFactory>().CreateClient("Bungie")));
        services.AddScoped(sp => new RequestManager.RequestServer(
            sp.GetRequiredService<ILogger<RequestManager.RequestServer>>(),
            sp.GetRequiredService<ClientProfile.Profile>(),
            sp.GetRequiredService<OAuthService.OAuthProfile>(),
            sp.GetRequiredService<IHttpClientFactory>().CreateClient("Bungie")));
        services.AddScoped(sp => new RequestManager.ManifestServer(
            sp.GetRequiredService<ILogger<RequestManager.ManifestServer>>()));
        services.AddScoped(sp => new ContentManager.ContentManager(
            sp.GetRequiredService<ILogger<ContentManager.ContentManager>>(),
            sp.GetRequiredService<ClientProfile.Profile>(),
            sp.GetRequiredService<RequestManager.RequestServer>(),
            sp.GetRequiredService<RequestManager.ManifestServer>(),
            sp.GetRequiredService<GuardianServices.SnackbarService>(),
            sp.GetRequiredService<GuardianServices.LoadingbarService>()));
        services.AddScoped<Core>();
        services.AddScoped<DialogUIService>();
        services.AddScoped<UICore>();

        return services;
    }
}
