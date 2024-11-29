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
    private bool authorizationComplete = false;
    private Profile profile;

    /// <summary>
    /// Initializes a new instance of the <see cref="Core"/> class.
    /// </summary>
    public Core()
    {
        this.profile = new Profile();
    }

    /// <summary>
    /// Starts the GuardianVaultManagerCore.
    /// </summary>
    public void StartCoreService()
    {
        if (this.authorizationComplete)
        {
        }
    }
}
