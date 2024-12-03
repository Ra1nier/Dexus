// <copyright file="Core.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace GuardianVaultManagerCore;

using ClientProfile;

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
        if (this.profile.Create()) // FIXME: This might not be the best way to check OAuth completion. Additional checks may be need.
        {
            this.OAuthStatus = true;
        }
        else
        {
            this.OAuthStatus = false;
        }
    }

    /// <summary>
    /// Gets a value indicating whether oAuth has been completed.
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
        if (!this.OAuthStatus)
        {
        }
    }
}
