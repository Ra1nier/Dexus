// <copyright file="Core.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace DexusCore;

using Microsoft.Extensions.Logging;
using ClientProfile;
using OAuthService;
using RequestManager;
using ContentManager;
using GuardianServices;

/// <summary>
/// Wraps the model of Dexus in a single core application manager.
/// </summary>
public class Core
{
    private static readonly SemaphoreSlim ManifestUpdateLock = new SemaphoreSlim(1, 1);

    // Core libraries
    private OAuthProfile oauthProfile;
    private Profile profile;
    private OAuthServer oauth;
    private RequestServer request;
    private ManifestServer manifest;
    private ContentManager content;

    // Core services
    private SnackbarService snackbar;
    private LoadingbarService loadingbars;
    private TimelineService timeline;

    // Main logger
    private ILogger logger;

    // Local Values
    private int oAuthStatus = 3;
    private Task? manifestLoadTask;

    /// <summary>
    /// Initializes a new instance of the <see cref="Core"/> class.
    /// </summary>
    /// <param name="oauthProfile">Injected oauthProfile service.</param>
    /// <param name="profile">Injected profile service.</param>
    /// <param name="oauth">Injected oauth service.</param>
    /// <param name="request">Injected request service.</param>
    /// <param name="manifest">Injected manifest service.</param>
    /// <param name="content">Injected content service.</param>
    /// <param name="snackbar">Injected snackbar service.</param>
    /// <param name="loadingbars">Injected loadingbars service.</param>
    /// <param name="timeline">Injected timeline service.</param>
    /// <param name="logger">Injected logger service.</param>
    public Core(
        OAuthProfile oauthProfile,
        Profile profile,
        OAuthServer oauth,
        RequestServer request,
        ManifestServer manifest,
        ContentManager content,
        SnackbarService snackbar,
        LoadingbarService loadingbars,
        TimelineService timeline,
        ILogger<Core> logger)
    {
        this.oauthProfile = oauthProfile;
        this.profile = profile;
        this.oauth = oauth;
        this.request = request;
        this.manifest = manifest;
        this.content = content;
        this.snackbar = snackbar;
        this.loadingbars = loadingbars;
        this.timeline = timeline;
        this.logger = logger;
        this.loadingbars.GenerateLoadingbar("Main");
        this.OAuthStatus = oauthProfile.HasValidAPIKeys() ? 2 : 3;
    }

    /// <summary>
    /// Gets the ContentManager used in the core.
    /// </summary>
    public ContentManager Content => this.content;

    /// <summary>
    /// Gets the OAuthProfile used in the core.
    /// </summary>
    public OAuthProfile OAuthProfile => this.oauthProfile;

    /// <summary>
    /// Gets a value indicating whether OAuth has been completed. 0 = Fully Completed, 1 = Refresh Needed, 2 = OAuth Not Completed, 3 = No OAuth Keys Provided.
    /// </summary>
    public int OAuthStatus
    {
        get
        {
            return this.oAuthStatus;
        }

        private set
        {
            if (value < 0 || value > 3)
            {
                this.oAuthStatus = 3;
            }
            else
            {
                this.oAuthStatus = value;
            }
        }
    }

    // Services

    /// <summary>
    /// Gets the snackbar service.
    /// </summary>
    public SnackbarService Snackbar => this.snackbar;

    /// <summary>
    /// Gets the loadingbars service.
    /// </summary>
    public LoadingbarService Loadingbars => this.loadingbars;

    /// <summary>
    /// Gets the timeline service.
    /// </summary>
    public TimelineService Timeline => this.timeline;

    // Main Functionality

    /// <summary>
    /// Attempts to sign in the client and create the user profile.
    /// </summary>
    /// <returns>A bool value indicating whether Sign-In was successful.</returns>
    public async Task<bool> SignIn()
    {
        // Create the client profile and assign client OAuth status.
        if (this.profile.Create() && this.profile.LoadDestinyID())
        {
            this.OAuthStatus = 0;

            return await this.ConfirmOAuthStatus();
        }

        return false;
    }

    /// <summary>
    /// Attemps to Logout the current user.
    /// </summary>
    /// <returns>A bool value indicating whether Logout was successful.</returns>
    public bool Logout()
    {
        if (this.profile.ThrowOut())
        {
            this.OAuthStatus = 2;
            this.Snackbar.AddMessage("You have been successfully logged out!");
            return true;
        }

        this.Snackbar.AddMessage("Dexus could not log you out. Try restarting.");
        return false;
    }

    /// <summary>
    /// Ensures the Destiny 2 manifest is loaded before manifest-dependent content is queried.
    /// </summary>
    /// <returns>A task representing the manifest load operation.</returns>
    public async Task EnsureManifestLoaded()
    {
        if (!string.IsNullOrWhiteSpace(this.manifest.ManifestPath))
        {
            return;
        }

        this.manifestLoadTask ??= this.GetDestiny2Manifest();
        await this.manifestLoadTask;

        if (string.IsNullOrWhiteSpace(this.manifest.ManifestPath))
        {
            this.manifestLoadTask = null;
        }
    }

    /// <summary>
    /// Checks and confirms whether the user's access and refresh token are still valid. If not, it will refresh the access token.
    /// </summary>
    /// <returns>True if the access token can be used. False otherwise.</returns>
    public async Task<bool> ConfirmOAuthStatus()
    {
        await this.profile.RefreshLock.WaitAsync();
        try
        {
            // Check if the API keys are valid. If not, set the OAuth status to 3 and return false.
            if (!this.OAuthProfile.HasValidAPIKeys())
            {
                this.OAuthStatus = 3;
                return false;
            }

            // Check if the refresh token is expired, and if so, start the OAuth process
            if (!this.profile.Create() || this.CheckExpiry(this.profile.RefreshTokenExpireDate))
            {
                this.OAuthStatus = 2;
                return false;
            }

            // Check if the access token is expired and if so, refresh the user tokens
            if (this.CheckExpiry(this.profile.AccessTokenExpireDate))
            {
                this.OAuthStatus = 1;
                return await this.RefreshAccessToken();
            }

            this.OAuthStatus = 0;
            return true;
        }
        finally
        {
            this.profile.RefreshLock.Release();
        }
    }

    /// <summary>
    /// Gets the Destiny 2 manifest from bungie and saves the path to it in the manifest manager.
    /// </summary>
    private async Task GetDestiny2Manifest()
    {
        await ManifestUpdateLock.WaitAsync();
        try
        {
            string englishManifestPath = await this.request.Destiny2Manifest_Request_GetManifestPath("en");
            if (!string.IsNullOrWhiteSpace(englishManifestPath))
            {
                this.manifest.ManifestPath = await this.request.Destiny2Manifest_Request_GetCurrentManifest(englishManifestPath);
            }
        }
        finally
        {
            ManifestUpdateLock.Release();
        }
    }

    /// <summary>
    /// Refreshes the Users Access token.
    /// </summary>
    /// <returns>True if the Refresh was successful. False otherwise.</returns>
    private async Task<bool> RefreshAccessToken()
    {
        string tokens = await this.oauth.RefreshAccessToken(this.profile.RefreshToken);
        if (tokens != "Error" && this.profile.StoreUserTokens(tokens))
        {
            this.logger.LogInformation("Refreshed User Tokens.");

            if (this.profile.Create() && this.profile.LoadDestinyID())
            {
                this.OAuthStatus = 0;
                return true;
            }
        }
        else
        {
            this.logger.LogCritical("Unable to Refresh User Tokens.");
        }

        return false;
    }

    /// <summary>
    /// Compares a DateTime value to the current time to see if the token has expired.
    /// </summary>
    /// <param name="expiry">DateTime value to compare.</param>
    /// <returns>True if it has expired, False if not.</returns>
    private bool CheckExpiry(DateTime expiry)
    {
        int expiredCondition = DateTime.Compare(DateTime.UtcNow, expiry);

        if (expiredCondition > 0 || expiredCondition == 0)
        {
            return true;
        }

        return false;
    }
}
