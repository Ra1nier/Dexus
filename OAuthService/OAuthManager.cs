// <copyright file="OAuthManager.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace OAuthService;

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

/// <summary>
/// Contains the need functionality to connect to bungie and handle OAuth functionality for the client.
/// </summary>
public class OAuthManager
{
    private HttpRequestMessage request;
    private HttpRequestMessage requestToken;
    private OAuthProfile profile;

    private ILogger logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="OAuthManager"/> class.
    /// </summary>
    /// <param name="profile">OAuthProfile used to start the OAuthProcess.</param>
    /// <param name="logger">Logging service..</param>
    public OAuthManager(OAuthProfile profile, ILogger logger)
    {
        this.profile = profile;
        this.logger = logger;

        // Generate OAuth URL
        this.request = new HttpRequestMessage(HttpMethod.Get, this.profile.Authorization_URL);
        this.requestToken = new HttpRequestMessage(HttpMethod.Post, this.profile.Token_Endpoint);
    }

    /// <summary>
    /// Starts the OAuth Process. DEPRECIATED -- LIKELY TO BE REMOVED.
    /// </summary>
    /// <returns>A task representing the asynchronous operation, containing a authorization code or Error if the request failed.</returns>
    public async Task<string> SendOAuthRequestAsync()
    {
        this.logger.LogDebug("Retrieving Authorization code from Bungie.");

        using var client = new HttpClient();
        this.request.RequestUri = this.profile.ConstructAuthorizationURL();

        try
        {
            var response = await client.SendAsync(this.request);
            if (response.IsSuccessStatusCode)
            {
                string jsonResponse = await response.Content.ReadAsStringAsync();

                this.logger.LogTrace($"OAuth request Accepted: {response.StatusCode}");
                this.logger.LogTrace($"OAuth request: {jsonResponse}");

                return jsonResponse;
            }
            else
            {
                this.logger.LogError($"Authorization code was not retrieved: {response.StatusCode}");

                return "Error";
            }
        }
        catch (Exception e)
        {
            // Log or handle the exception
            this.logger.LogError($"Error sending OAuth request for Authorization code: {e.Message}");
            return "Error";
        }
    }

    /// <summary>
    /// Exchanges the provided Authorization code for a token from bungie.
    /// </summary>
    /// <param name="authorizationCode">Code used to send the exchange request.</param>
    /// <returns>A task representing the asynchronous operation, containing a token indicating success.</returns>
    public async Task<string> ExchangeForTokenAsync(string authorizationCode)
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

        using var client = new HttpClient();
        this.requestToken.Method = HttpMethod.Post;
        this.requestToken.Content = content;

        try
        {
            // Log the request body for debugging purposes
            var contentString = await content.ReadAsStringAsync();

            var response = await client.SendAsync(this.requestToken);
            if (response.IsSuccessStatusCode)
            {
                string jsonResponse = await response.Content.ReadAsStringAsync();

                this.logger.LogTrace($"Token Retrieved Successfully: {response.StatusCode}");
                this.logger.LogTrace($"Token Retrieved: {jsonResponse}");

                return jsonResponse;
            }
            else
            {
                // Log the response for debugging purposes
                string errorResponse = await response.Content.ReadAsStringAsync();
                this.logger.LogError($"Token Retrieval failed: {response.StatusCode}, {errorResponse}");

                return "Error";
            }
        }
        catch (Exception e)
        {
            this.logger.LogError($"Error sending OAuth request for Authorization code: {e.Message}");
            return "Error";
        }
    }
}