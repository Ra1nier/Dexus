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
    private readonly HttpClient client;
    private OAuthProfile profile;

    private ILogger logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="OAuthServer"/> class.
    /// </summary>
    /// <param name="logger">Logger service.</param>
    /// <param name="profile">OAuth profile to use during any OAuth requests.</param>
    /// <param name="client">Managed HTTP client for Bungie requests.</param>
    public OAuthServer(ILogger logger, OAuthProfile profile, HttpClient client)
    {
        this.logger = logger;
        this.profile = profile;
        this.client = client;
    }

    /// <summary>
    /// Starts the OAuth process.
    /// </summary>
    /// <param name="authorizationCode">Authorization Code received from bungie.</param>
    /// <returns>A <see cref="Task{TResult}"/>A task representing the asynchronous operation, returning json containing tokens if successful.</returns>
    public async Task<string> StartOAuthProcessAsync(string authorizationCode)
    {
        this.logger.LogInformation("Bungie OAuth server started.");

        using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, this.profile.Token_Endpoint);
        request.Headers.UserAgent.ParseAdd($"Dexus/1.0 AppId/{this.profile.Client_ID}");

        string tokens = await this.ExchangeForTokenAsync(request, authorizationCode);

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
    /// Refreshes the access token from Bungie.
    /// </summary>
    /// <param name="refreshToken">Token/Code to use in order to refresh the access token.</param>
    /// <returns>A string containing the response from Bungie.</returns>
    public async Task<string> RefreshAccessToken(string refreshToken)
    {
        this.logger.LogInformation("Sending Access Token Refresh Request.");

        using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, this.profile.Token_Endpoint);
        request.Headers.UserAgent.ParseAdd($"Dexus/1.0 AppId/{this.profile.Client_ID}");

        // Add form parameters for the POST body
        var formData = new Dictionary<string, string>
            {
                { "grant_type", "refresh_token" },
                { "refresh_token", refreshToken },
                { "client_id", this.profile.Client_ID },
                { "client_secret", this.profile.Client_Secret },
            };

        // Create a FormUrlEncodedContent with the parameters
        var content = new FormUrlEncodedContent(formData);

        request.Method = HttpMethod.Post;
        request.Content = content;

        try
        {
            using var response = await this.client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                string jsonResponse = await response.Content.ReadAsStringAsync();

                this.logger.LogInformation($"Refresh Successful: {response.StatusCode}");

                return jsonResponse;
            }
            else
            {
                this.logger.LogError("Token refresh failed with status code {StatusCode}.", response.StatusCode);

                return "Error";
            }
        }
        catch (Exception)
        {
            this.logger.LogError("Bungie token refresh request failed.");
            return "Error";
        }
    }

    /// <summary>
    /// Exchanges the provided Authorization code for a token from bungie.
    /// </summary>
    /// <param name="request">HTTPRequest to use for the exchange.</param>
    /// <param name="authorizationCode">Code used to send the exchange request.</param>
    /// <returns>A task representing the asynchronous operation, containing a token indicating success.</returns>
    private async Task<string> ExchangeForTokenAsync(HttpRequestMessage request, string authorizationCode)
    {
        this.logger.LogDebug("Exchanging Authorization code for tokens.");

        // Add form parameters for the POST body
        var formData = new Dictionary<string, string>
            {
                { "grant_type", "authorization_code" },
                { "code", authorizationCode },
                { "client_id", this.profile.Client_ID },
                { "client_secret", this.profile.Client_Secret },
            };

        // Create a FormUrlEncodedContent with the parameters
        var content = new FormUrlEncodedContent(formData);

        request.Method = HttpMethod.Post;
        request.Content = content;
        request.Headers.UserAgent.ParseAdd($"Dexus/1.0 AppId/{this.profile.Client_ID}");

        try
        {
            using var response = await this.client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                string jsonResponse = await response.Content.ReadAsStringAsync();

                this.logger.LogTrace($"Token Retrieved Successfully: {response.StatusCode}");

                return jsonResponse;
            }
            else
            {
                this.logger.LogError("Token retrieval failed with status code {StatusCode}.", response.StatusCode);

                return "Error";
            }
        }
        catch (Exception)
        {
            this.logger.LogError("Bungie token exchange request failed.");
            return "Error";
        }
    }
}
