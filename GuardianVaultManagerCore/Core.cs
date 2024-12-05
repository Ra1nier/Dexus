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
    private Profile profile;

    /// <summary>
    /// Initializes a new instance of the <see cref="Core"/> class.
    /// </summary>
    public Core()
    {
        this.profile = new Profile();

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
    /// Starts the GuardianVaultManagerCore.
    /// </summary>
    public void StartCoreService()
    {
    }

    /// <summary>
    /// Handles signing in the user to bungie to reterive nessissary data.
    /// </summary>
    public async void StartSignIn()
    {
        OAuthServer oauth = new OAuthServer();

        string tokens = await oauth.StartOAuthProcessAsync();

        if (this.profile.StoreUserTokens(tokens))
        {
            if (this.profile.Create())
            {
                this.OAuthStatus = true;
            }
            else
            {
                throw new Exception("Profile could not be created after tokens were retrived.");
            }
        }
    }
}
