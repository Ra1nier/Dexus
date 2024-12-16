// <copyright file="OAuthServer.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace OAuthService;

using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

/// <summary>
/// Wraps all OAuth functionality into a servlet.
/// </summary>
public class OAuthServer
{
    private OAuthProfile? profile;
    private OAuthManager? oauthManager;

    private ILogger logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="OAuthServer"/> class.
    /// </summary>
    /// <param name="logger">Logger service.</param>
    public OAuthServer(ILogger logger)
    {
        this.logger = logger;
    }

    /// <summary>
    /// Starts the OAuth process.
    /// </summary>
    /// <param name="profileToUse">OAuthProfile to use for initializing the OAuth Process.</param>
    /// <param name="authorizationCode">Authorization Code received from bungie.</param>
    /// <returns>A <see cref="Task{TResult}"/>A task representing the asynchronous operation, returning json containing tokens if successful.</returns>
    public async Task<string> StartOAuthProcessAsync(OAuthProfile profileToUse, string authorizationCode)
    {
        this.logger.LogInformation("Bungie OAuth server started.");
        this.profile = profileToUse;
        this.oauthManager = new OAuthManager(this.profile, this.logger);

        string tokens = await this.oauthManager.ExchangeForTokenAsync(authorizationCode);

        if (tokens.Contains("access_token"))
        {
            return tokens;
        }
        else
        {
            return "Error";
        }
    }
}
