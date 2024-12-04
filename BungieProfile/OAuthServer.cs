// <copyright file="OAuthServer.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace OAuthManger;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using global::OAuthManager;

/// <summary>
/// Wraps all OAuth functionality into a servlet.
/// </summary>
public class OAuthServer
{
    private OAuthProfile profile;
    private OAuthManager oauthManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="OAuthServer"/> class.
    /// </summary>
    public OAuthServer()
    {
        this.profile = new ();
        this.oauthManager = new OAuthManager(this.profile);
    }

    /// <summary>
    /// Starts the OAuth process.
    /// </summary>
    /// <returns>A task representing the asynchronous operation, returning true if successful.</returns>
    public async Task<bool> StartOAuthProcessAsync()
    {
        string response = await this.oauthManager.SendOAuthRequestAsync();
    }
}
