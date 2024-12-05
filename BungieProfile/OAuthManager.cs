// <copyright file="OAuthManager.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace OAuthService;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

/// <summary>
/// Contains the need functionality to connect to bungie and handle OAuth functionality for the client.
/// </summary>
public class OAuthManager
{
    private HttpRequestMessage request;
    private HttpRequestMessage requestToken;
    private OAuthProfile profile;

    /// <summary>
    /// Initializes a new instance of the <see cref="OAuthManager"/> class.
    /// </summary>
    /// <param name="profile">OAuthProfile used to start the OAuthProcces.</param>
    public OAuthManager(OAuthProfile profile)
    {
        this.profile = profile;

        // Generate OAuth URL
        this.request = new HttpRequestMessage(HttpMethod.Get, this.profile.Authorization_URL);
        this.requestToken = new HttpRequestMessage(HttpMethod.Get, this.profile.Token_Endpoint);
    }

    /// <summary>
    /// Starts the OAuth Procces.
    /// </summary>
    /// <returns>A task representing the asynchronous operation, containing a authorization code or Error if the request failed.</returns>
    public async Task<string> SendOAuthRequestAsync()
    {
        // Add query parameters
        var queryParameters = new Dictionary<string, string>
        {
            { "client_id", this.profile.Client_ID },
            { "response_type", "code" },
            { "state", this.profile.State },
        };

        var uriBuilder = new UriBuilder(this.profile.Authorization_URL)
        {
            Query = string.Join("&", queryParameters.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}")),
        };

        using var client = new HttpClient();
        this.request.RequestUri = uriBuilder.Uri;

        try
        {
            var response = await client.SendAsync(this.request);
            if (response.IsSuccessStatusCode)
            {
                string jsonResponse = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"OAuth request Accepted: {response.StatusCode}");
                Console.WriteLine($"OAuth request: {jsonResponse}");

                return jsonResponse;
            }
            else
            {
                Console.WriteLine($"OAuth request failed: {response.StatusCode}");

                return "Error";
            }
        }
        catch (Exception e)
        {
            // Log or handle the exception
            Console.WriteLine($"Error sending OAuth request: {e.Message}");
            return "Error";
        }
    }

    /// <summary>
    /// Exchanges the provided Authorization code for a token from bungie.
    /// </summary>
    /// <param name="authroizationCode">Code used to send the exchange request.</param>
    /// <returns>A task representing the asynchronous operation, containing a token indicating success.</returns>
    public async Task<string> ExchangeForTokenAsync(string authroizationCode)
    {
        // Add query parameters
        var queryParameters = new Dictionary<string, string>
        {
            { "grant_type", "authorization_code" },
            { "code", authroizationCode },
            { "client_id", this.profile.Client_ID },
            { "client_secret", this.profile.Client_Secret },
            { "client_secret", this.profile.Client_Secret },
        };

        var uriBuilder = new UriBuilder(this.profile.Token_Endpoint)
        {
            Query = string.Join("&", queryParameters.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}")),
        };

        using var client = new HttpClient();
        this.requestToken.RequestUri = uriBuilder.Uri;

        try
        {
            var response = await client.SendAsync(this.request);
            if (response.IsSuccessStatusCode)
            {
                string jsonResponse = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"Token Retreived Successfully: {response.StatusCode}");
                Console.WriteLine($"Token Retreived: {jsonResponse}");

                return jsonResponse;
            }
            else
            {
                Console.WriteLine($"Token Retreive failed: {response.StatusCode}");

                return "Error";
            }
        }
        catch (Exception e)
        {
            // Log or handle the exception
            Console.WriteLine($"Error sending token retreive request: {e.Message}");
            return "Error";
        }
    }
}
