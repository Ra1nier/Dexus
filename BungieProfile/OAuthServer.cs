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
    /// <returns>A task representing the asynchronous operation, returning json conatining tokens if successful.</returns>
    public async Task<string> StartOAuthProcessAsync()
    {
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
