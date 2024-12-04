// <copyright file="OAuthManager.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace OAuthManger;

using global::OAuthManager;
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
    }

    /// <summary>
    /// Starts the OAuth Procces.
    /// </summary>
    /// <returns>A task representing the asynchronous operation, containing a boolean indicating success.</returns>
    public async Task<string> SendOAuthRequestAsync()
    {
        // Add query parameters
        var queryParameters = new Dictionary<string, string>
        {
            { "client_id", this.profile.Client_ID },
            { "response_type", "code" },
            { "redirect_uri", this.profile.Redirect_URL },
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
        catch (Exception ex)
        {
            // Log or handle the exception
            Console.WriteLine($"Error sending OAuth request: {ex.Message}");
            return "Error";
        }
    }
}
