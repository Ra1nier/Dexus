// <copyright file="OAuthServer.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace OAuthService;

using System.Threading.Tasks;

/// <summary>
/// Wraps all OAuth functionality into a servlet.
/// </summary>
public class OAuthServer
{
    private OAuthProfile? profile;
    private OAuthManager? oauthManager;

    /// <summary>
    /// Starts the OAuth process.
    /// </summary>
    /// <param name="profileToUse">OAuthProfile to use for initializing the OAuth Process.</param>
    /// <returns>A <see cref="Task{TResult}"/>A task representing the asynchronous operation, returning json containing tokens if successful.</returns>
    public async Task<string> StartOAuthProcessAsync(OAuthProfile profileToUse)
    {
        this.profile = profileToUse;
        this.oauthManager = new OAuthManager(this.profile);

        string authorizationCode;

        string response = await this.oauthManager.SendOAuthRequestAsync();
        if (response.Contains("Error"))
        {
            return "Error";
        }
        else
        {
            authorizationCode = this.ExtractAuthorizationCode(response);
        }

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

    /// <summary>
    /// Extracts the OAuth Code from the given json string.
    /// </summary>
    /// <param name="json">JSON to extract code from.</param>
    /// <returns>The OAuthCode.</returns>
    /// <exception cref="InvalidDataException">If the JSON does not contain a valid OAuthCode.</exception>
    private string ExtractAuthorizationCode(string json)
    {
        if (json.Contains("code="))
        {
            return json.Split("code=")[1].Split("&")[0];
        }
        else
        {
            throw new InvalidDataException("Json does not contain code.");
        }
    }
}
