// <copyright file="OAuthManager.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace OAuthService;

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

    /// <summary>
    /// Initializes a new instance of the <see cref="OAuthManager"/> class.
    /// </summary>
    /// <param name="profile">OAuthProfile used to start the OAuthProcess.</param>
    public OAuthManager(OAuthProfile profile)
    {
        this.profile = profile;

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
        using var client = new HttpClient();
        this.request.RequestUri = this.profile.ConstructAuthorizationURL();

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
    /// <param name="authorizationCode">Code used to send the exchange request.</param>
    /// <returns>A task representing the asynchronous operation, containing a token indicating success.</returns>
    public async Task<string> ExchangeForTokenAsync(string authorizationCode)
    {
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
        this.requestToken.Method = HttpMethod.Post; // Ensure the method is POST
        this.requestToken.Content = content; // Attach the form data as the request body

        try
        {
            // Log the request body for debugging purposes
            var contentString = await content.ReadAsStringAsync();
            Debug.WriteLine($"Request Body: {contentString}");

            var response = await client.SendAsync(this.requestToken);
            if (response.IsSuccessStatusCode)
            {
                string jsonResponse = await response.Content.ReadAsStringAsync();

                Debug.WriteLine($"Token Retrieved Successfully: {response.StatusCode}");
                Debug.WriteLine($"Token Retrieved: {jsonResponse}");

                return jsonResponse;
            }
            else
            {
                // Log the response for debugging purposes
                string errorResponse = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"Token Retrieve failed: {response.StatusCode}");
                Debug.WriteLine($"Error Response: {errorResponse}");

                return "Error";
            }
        }
        catch (Exception e)
        {
            // Log or handle the exception
            throw new HttpRequestException($"Unable to exchange code for token: {e.Message}");
        }
    }
}