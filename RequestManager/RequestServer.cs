// <copyright file="RequestServer.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace RequestManager;

using ClientProfile;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using OAuthService;
using System.Text.Json;

/// <summary>
/// Handles requests to bungie and returning the result to the application.
/// </summary>
public class RequestServer
{
    private HttpClient client;
    private Profile profile;
    private OAuthProfile auth;

    // Logger
    private ILogger logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RequestServer"/> class.
    /// </summary>
    /// <param name="profile">Client Profile.</param>
    /// <param name="auth">OAuth Profile.</param>
    /// <param name="logger">logging service.</param>
    public RequestServer(Profile profile, OAuthProfile auth, ILogger logger)
    {
        this.client = new HttpClient();
        this.profile = profile;
        this.auth = auth;
        this.logger = logger;
    }

    /// <summary>
    /// Sends a request to Bungie for the user's MembershipID.
    /// </summary>
    /// <returns>The membershipID or an error message.</returns>
    public async Task<string> PlatformDestiny2_Request_MembershipIDAsync()
    {
        this.logger.LogInformation("REQUEST: Destiny Membership ID");

        // Construct HTTP request
        string requestURI = $"https://www.bungie.net/Platform/Destiny2/{this.profile.MembershipType}/Profile/{this.profile.MembershipID}/LinkedProfiles/?getAllMemberships=false";
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestURI);

        // Add headers
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", this.profile.AccessToken);
        request.Headers.Add("X-API-Key", this.auth.API_Key);

        // Send request
        try
        {
            HttpResponseMessage response = await this.client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                string jsonResponse = await response.Content.ReadAsStringAsync();
                this.logger.LogDebug($"Successful JSON Response: {jsonResponse}");

                // Parse the JSON response
                using JsonDocument document = JsonDocument.Parse(jsonResponse);

                // Navigate to the "profilesWithErrors" array
                var profilesWithErrors = document.RootElement
                    .GetProperty("Response")
                    .GetProperty("profilesWithErrors");

                string id = "Error";

                // Loop through the array to find the target membershipId
                foreach (var profile in profilesWithErrors.EnumerateArray())
                {
                    if (profile.TryGetProperty("infoCard", out JsonElement infoCard))
                    {
                        if (infoCard.TryGetProperty("membershipId", out JsonElement value))
                        {
                            id = value.GetString() ?? "Error"; // Return the matching membershipId
                        }
                    }
                }

                return id;
            }
            else
            {
                string errorResponse = await response.Content.ReadAsStringAsync();
                this.logger.LogError($"Error Response: {errorResponse}");
                return "Error";
            }
        }
        catch (Exception e)
        {
            this.logger.LogError($"REQUEST FAILED: Destiny Membership ID, {e.Message}");
            return "Error";
        }
    }

    /// <summary>
    /// Parses the given JSON for the key contained in it.
    /// </summary>
    /// <param name="response">JSON string to parse.</param>
    /// <param name="key">Value key to parse.</param>
    /// <returns>The parsed value.</returns>
    private string ParseResponse(string response, string key)
    {
        try
        {
            using JsonDocument document = JsonDocument.Parse(response);
            if (document.RootElement.TryGetProperty(key, out JsonElement value))
            {
                return value.GetString() ?? string.Empty;
            }
            else
            {
                throw new KeyNotFoundException($"Key '{key}' not found in the JSON response.");
            }
        }
        catch (JsonException e)
        {
            this.logger.LogError($"JSON Parsing Error: {e.Message}");
            throw;
        }
    }
}
