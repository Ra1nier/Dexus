// <copyright file="RequestServer.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace RequestManager;

using ClientProfile;
using Newtonsoft.Json.Linq;
using OAuthService;
using System.Diagnostics;

/// <summary>
/// Handles requests to bungie and returning the result to the application.
/// </summary>
public class RequestServer
{
    private HttpClient client;
    private Profile profile;
    private OAuthProfile auth;

    /// <summary>
    /// Initializes a new instance of the <see cref="RequestServer"/> class.
    /// </summary>
    /// <param name="profile">Client Profile.</param>
    /// <param name="auth">OAuth Profile.</param>
    public RequestServer(Profile profile, OAuthProfile auth)
    {
        this.client = new HttpClient();
        this.profile = profile;
        this.auth = auth;
    }

    /// <summary>
    /// Sends a request to Bungie for the users MembershipID.
    /// </summary>
    /// <returns>The membershipID.</returns>
    public async Task<string> PlatformDestiny2_Request_MembershipIDAsync()
    {
        // Construct Http request
        string requestURI = $"https://www.bungie.net/Platform/Destiny2/{this.profile.MembershipType}/Profile/{this.profile.MembershipID}/LinkedProfiles/?getAllMemberships=false";
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, requestURI);

        // Construct Request Headers
        var formData = new Dictionary<string, string>
        {
            { "Authorization", $"Bearer {this.profile.AccessToken}" },
            { "X-API-Key",  this.auth.API_Key },
        };

        var requestContent = new FormUrlEncodedContent(formData);
        request.Content = requestContent;

        // Send Request
        try
        {
            HttpResponseMessage response = await this.client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                string jsonResponse = await response.Content.ReadAsStringAsync();
                return this.ParseResponse(jsonResponse, "membershipId");
            }
            else
            {
                string errorResponse = await response.Content.ReadAsStringAsync();

                return "Error";
            }
        }
        catch (Exception e)
        {
            Debug.WriteLine($"Exception occurred: {e.Message}");
            return "Error";
        }
    }

    /// <summary>
    /// Parses the given JSON for the Key contained in it.
    /// </summary>
    /// <param name="response">JSON String to parse.</param>
    /// <param name="key">Value Key to parse.</param>
    /// <returns>The parsed value.</returns>
    private string ParseResponse(string response, string key)
    {
        return JObject.Parse(response)[key].ToString();
    }
}
