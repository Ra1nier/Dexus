// <copyright file="Core.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace GuardianVaultManagerCore;

using ClientProfile;
using Microsoft.Extensions.Logging;
using OAuthService;
using RequestManager;
using System.ComponentModel;

/// <summary>
/// Wraps the model of GuardianVaultManager in a single core application manager.
/// </summary>
public class Core
{
    private OAuthProfile oauthProfile;
    private Profile profile;
    private OAuthServer oauth;
    private RequestServer request;

    private ILogger logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="Core"/> class.
    /// </summary>
    public Core()
    {
        // Create logger
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder
                .SetMinimumLevel(LogLevel.Trace)
                .AddConsole()
                .AddDebug();
        });
        this.logger = loggerFactory.CreateLogger<Core>();

        // Create local instances of libraries
        this.profile = new Profile();
        this.oauthProfile = new OAuthProfile();
        this.oauth = new OAuthServer(this.logger);
        this.request = new RequestServer(this.profile, this.oauthProfile, this.logger);

        this.OAuthStatus = false;
    }

    /// <summary>
    /// Gets a value indicating whether OAuth has been completed.
    /// </summary>
    public bool OAuthStatus
    {
        get;
        private set;
    }

    /// <summary>
    /// Gets the OAuthProfile used in the core.
    /// </summary>
    public OAuthProfile OAuthProfile => this.oauthProfile;

    /// <summary>
    /// Starts the GuardianVaultManagerCore.
    /// </summary>
    public async void StartCoreService()
    {
        if (!await this.SignIn())
        {
            this.logger.LogError("Could not sign the client in. Sign In Failed");
        }
    }

    /// <summary>
    /// Attempts to sign in the client and create the user profile.
    /// </summary>
    /// <returns>A bool value indicating whether or not Sign In was successful.</returns>
    public async Task<bool> SignIn()
    {
        // Create the client profile and assign client OAuth status.
        if (this.profile.Create() && this.profile.LoadDestinyID())
        {
            this.OAuthStatus = true;
            this.logger.LogCritical(await this.request.PlatformDestiny2_Request_Contents("CharacterInventories", true));

            return true;
        }

        return false;
    }

    /// <summary>
    /// Handles signing in the user to bungie to retrieve necessary data.
    /// </summary>
    /// <param name="code">Authorization code.</param>
    /// <exception cref="Exception">If the Exchange could not take place.</exception>
    public async void StartOAuth(string code)
    {
        bool tokensStored = false;
        bool destinyIDStored = false;

        this.logger.LogDebug("Starting OAuth");

        // Get Tokens from Bungie
        string tokens = await this.oauth.StartOAuthProcessAsync(this.oauthProfile, code);
        if (tokens != "Error")
        {
            tokensStored = this.profile.StoreUserTokens(tokens);
        }

        // Create Profile
        if (tokensStored)
        {
            if (!this.profile.Create())
            {
                tokensStored = false;
                this.logger.LogWarning("Client Profile could not be created. Functionality will be extremely limited.");
            }
        }

        // Get DestinyMembershipID
        string[] destinyID = await this.request.PlatformDestiny2_Request_MembershipIDAsync();
        if (destinyID[0] != "Error")
        {
            destinyIDStored = this.profile.StoreDestinyMembershipID(destinyID[0], destinyID[1]);
        }

        // Load the Destiny ID into the profile
        if (destinyIDStored)
        {
            if (!this.profile.LoadDestinyID())
            {
                destinyIDStored = false;
                this.logger.LogWarning("Destiny ID could not be loaded into the profile. DestinyMembershipID failed to initialize");
            }
        }

        if (tokensStored && destinyIDStored)
        {
            this.logger.LogDebug("Profile has been successfully created.");
            this.OAuthStatus = true;
        }
        else
        {
            string reason = tokensStored ? "Destiny ID could not be loaded" : "Tokens could not be loaded.";

            this.logger.LogError($"Profile failed to initialize. {reason}");
        }
    }

    /// <summary>
    /// Retrieves the clients vault contents from Bungie.
    /// </summary>
    /// <returns>A string containing the vault JSON.</returns>
    public async Task<string> GetVaultContents()
    {
        return await this.request.PlatformDestiny2_Request_Contents("CharacterInventories", true);
    }
}
