// <copyright file="Core.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace GuardianVaultManagerCore;

using ClientProfile;
using OAuthService;

/// <summary>
/// Wraps the model of GuardianVaultManager in a single core application manager.
/// </summary>
public class Core
{
    private OAuthProfile oauthProfile;
    private Profile profile;
    private OAuthServer oauth;

    /// <summary>
    /// Initializes a new instance of the <see cref="Core"/> class.
    /// </summary>
    public Core()
    {
        this.profile = new Profile();
        this.oauthProfile = new OAuthProfile();
        this.oauth = new OAuthServer();

        // Create the client profile and assign client OAuth status.
        if (this.profile.Create())
        {
            this.OAuthStatus = true;
        }
        else
        {
            this.OAuthStatus = false;
        }
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
    /// Gets the OAuthProfile used in the server.
    /// </summary>
    public OAuthProfile OAuthProfile => this.oauthProfile;

    /// <summary>
    /// Starts the GuardianVaultManagerCore.
    /// </summary>
    public void StartCoreService()
    {
    }

    /// <summary>
    /// Handles signing in the user to bungie to retrieve necessary data.
    /// </summary>
    public async void StartSignIn()
    {
        string tokens = await this.oauth.StartOAuthProcessAsync(this.oauthProfile);

        if (this.profile.StoreUserTokens(tokens))
        {
            if (this.profile.Create())
            {
                this.OAuthStatus = true;
            }
            else
            {
                throw new Exception("Profile could not be created after tokens were retrieved.");
            }
        }
    }
}
