// <copyright file="Core.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace GuardianVaultManagerCore;

using ClientProfile;
using Microsoft.Extensions.Logging;
using OAuthService;
using RequestManager;

/// <summary>
/// Wraps the model of GuardianVaultManager in a single core application manager.
/// </summary>
public class Core
{
    private OAuthProfile oauthProfile;
    private Profile profile;
    private OAuthServer oauth;
    private RequestServer request;
    private ManifestServer manifest;

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
        this.oauth = new OAuthServer(this.logger, this.oauthProfile);
        this.request = new RequestServer(this.profile, this.oauthProfile, this.logger);
        this.manifest = new ManifestServer(this.logger);

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

        this.GetDestiny2Manifest();
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

            // Check if the refresh token is expired and if so start the OAuth process
            if (this.CheckExpiry(this.profile.RefreshTokenExpireDate))
            {
                this.OAuthStatus = false;
                return false;
            }

            // Check if access token is expired and if so refresh the user tokens
            if (this.CheckExpiry(this.profile.AccessTokenExpireDate))
            {
                this.OAuthStatus = false;

                string tokens = await this.oauth.RefreshAccessToken(this.profile.RefreshToken);
                if (tokens != "Error" && this.profile.StoreUserTokens(tokens))
                {
                    this.logger.LogInformation("Refreshed User Tokens.");

                    if (this.profile.Create() && this.profile.LoadDestinyID())
                    {
                        this.OAuthStatus = true;
                    }
                }
                else
                {
                    this.logger.LogCritical("Unable to Refresh User Tokens.");
                }
            }

            return true;
        }

        return false;
    }

    /// <summary>
    /// Gets the Destiny 2 manifest from bungie.
    /// </summary>
    public async void GetDestiny2Manifest()
    {
        this.manifest.ManifestPath = await this.request.Manifest_Request_GetCurrentManifest();
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
        string tokens = await this.oauth.StartOAuthProcessAsync(code);
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
        return await this.request.PlatformDestiny2_Request_Contents("ProfileInventories", true);
    }

    /// <summary>
    /// Retrieves the clients vault contents from Bungie.
    /// </summary>
    /// <returns>A string containing the vault JSON.</returns>
    public async Task<string> GetCharacters()
    {
        return await this.request.PlatformDestiny2_Request_Contents("Characters", true);
    }

    /// <summary>
    /// Retrieves the clients vault contents from Bungie.
    /// </summary>
    /// <returns>A string containing the vault JSON.</returns>
    public async Task<string> GetCharactersInventories()
    {
        return await this.request.PlatformDestiny2_Request_Contents("CharacterInventories", true);
    }

    /// <summary>
    /// Gets the photo of the itemHash provided.
    /// </summary>
    /// <param name="itemHash">ItemHash to use in the search.</param>
    /// <returns>The URL for the photo.</returns>
    public async Task<WeaponDetails> GetWeaponDetails(long itemHash)
    {
         return await this.manifest.Manifest_Query_WeaponDetails(itemHash);
    }

    /// <summary>
    /// Transfers the requested item to either the characterID given or the vault if indicated.
    /// </summary>
    /// <param name="itemHash">The item's Hash.</param>
    /// <param name="itemID">The item's ID.</param>
    /// <param name="stackSize">The amount to transfer.</param>
    /// <param name="characterID">The character ID of where either the item resides or is to be transferred.</param>
    /// <param name="isVaultTransfer">Indicates whether this is a transfer between a character to the vault.</param>
    /// <returns>True if the operation was completed successfully, false otherwise.</returns>
    public async Task<bool> TransferItem(long itemHash, string itemID, int stackSize, string characterID, bool isVaultTransfer)
    {
        if (this.profile.MembershipType != null)
        {
            return await this.request.ActionItem_Request_TransferItem(this.profile.MembershipType.ToString(), itemHash, itemID, stackSize, characterID, isVaultTransfer);
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
        int expiredCondition = DateTime.Compare(DateTime.Now, expiry);

        if (expiredCondition > 0 || expiredCondition == 0)
        {
            return true;
        }

        return false;
    }
}
