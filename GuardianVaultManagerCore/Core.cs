// <copyright file="Core.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace GuardianVaultManagerCore;

using ClientProfile;
using OAuthService;
using RequestManager;
using System.Diagnostics;

/// <summary>
/// Wraps the model of GuardianVaultManager in a single core application manager.
/// </summary>
public class Core
{
    private OAuthProfile oauthProfile;
    private Profile profile;
    private OAuthServer oauth;
    private RequestServer request;

    /// <summary>
    /// Initializes a new instance of the <see cref="Core"/> class.
    /// </summary>
    public Core()
    {
        this.profile = new Profile();
        this.oauthProfile = new OAuthProfile();
        this.oauth = new OAuthServer();
        this.request = new RequestServer(this.profile, this.oauthProfile);

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
    /// <param name="code">Authorization code.</param>
    /// <exception cref="Exception">If the Exchange could not take place.</exception>
    public async void StartOAuth(string code)
    {
        bool tokensStored = false;
        bool destinyIDStored = false;

        Debug.WriteLine("Starting OAuth");

        // Get Tokens
        string tokens = await this.oauth.StartOAuthProcessAsync(this.oauthProfile, code);
        if (tokens != "Error")
        {
            tokensStored = this.profile.StoreUserTokens(tokens);
        }

        // Get DestinyMembershipID
        string destinyID = await this.request.PlatformDestiny2_Request_MembershipIDAsync();
        if (destinyID != "Error")
        {
            destinyIDStored = this.profile.StoreDestinyMembershipID(destinyID);
        }

        // Create Profile
        if (tokensStored && destinyIDStored)
        {
            if (this.profile.Create())
            {
                this.OAuthStatus = true;
            }
            else
            {
                Debug.WriteLine("Client Profile could not be created.");
            }
        }
    }
}
