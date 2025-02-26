// <copyright file="OAuthProfile.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace OAuthService;

using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;

/// <summary>
/// Contains the needed Bungie access points and tokens in a data class.
/// </summary>
public class OAuthProfile
{
    // Data paths
    private readonly string dataFolderPath;
    private readonly string userTokenFileLocation;

    // OAuth Variables
    private readonly string apikey;
    private readonly string clientID;
    private readonly string clientSecret;
    private readonly string state;
    private readonly string authorizationURL = "https://www.bungie.net/en/OAuth/Authorize";
    private readonly string tokenEndpoint = "https://www.bungie.net/Platform/App/OAuth/Token/";

    /// <summary>
    /// Initializes a new instance of the <see cref="OAuthProfile"/> class.
    /// </summary>
    public OAuthProfile()
    {
        // Grab information from user secrets file
        // var bungieSecrets = new ConfigurationBuilder().AddUserSecrets<OAuthProfile>();
        // IConfigurationRoot configuration = bungieSecrets.Build();
        // var secrets = configuration.GetSection("BungieAPIKeys");
        this.dataFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "OAuthProfile");
        this.userTokenFileLocation = Path.Combine(this.dataFolderPath, "UserBungieAPITokens.json");

        // Store keys from secrets file
        this.LoadUserAPIKeys(out this.apikey, out this.clientID, out this.clientSecret);
        this.state = this.GenerateRandomStateValue();
    }

    /// <summary>
    /// Gets the API key for GuardianVaultManger given by Bungie.
    /// </summary>
    public string API_Key => this.apikey;

    /// <summary>
    /// Gets the Client ID for GuardianVaultManger given by Bungie.
    /// </summary>
    public string Client_ID => this.clientID;

    /// <summary>
    /// Gets the Client Secret key for GuardianVaultManger given by Bungie.
    /// </summary>
    public string Client_Secret => this.clientSecret;

    /// <summary>
    /// Gets the Application State Code. Used to prevent Cross site forgery.
    /// </summary>
    public string State => this.state;

    /// <summary>
    /// Gets the Authorization URL for Bungie.
    /// </summary>
    public string Authorization_URL => this.authorizationURL;

    /// <summary>
    /// Gets the Token Endpoint URL for Bungie.
    /// </summary>
    public string Token_Endpoint => this.tokenEndpoint;

    /// <summary>
    /// Constructs a Authorization URL from the OAuthProfile.
    /// </summary>
    /// <returns>The Authorization URI.</returns>
    public Uri ConstructAuthorizationURL()
    {
        // Add query parameters
        var queryParameters = new Dictionary<string, string>
        {
            { "client_id", this.clientID },
            { "response_type", "code" },
            { "state", this.state },
        };

        // Build URI
        var uriBuilder = new UriBuilder(this.Authorization_URL)
        {
            Query = string.Join("&", queryParameters.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}")),
        };

        return uriBuilder.Uri;
    }

    /// <summary>
    /// Generates a random state value to be used in Authorization URI creation.
    /// </summary>
    /// <returns>The state string.</returns>
    private string GenerateRandomStateValue()
    {
        const string pool = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        StringBuilder builder = new StringBuilder();

        for (int i = 0; i < 20; i++)
        {
            builder.Append(pool[new Random().Next(0, pool.Length)]);
        }

        return builder.ToString();
    }

    /// <summary>
    /// Loads the API Keys provided by the user.
    /// </summary>
    /// <param name="apiKey">The API key given by the user.</param>
    /// <param name="clientID">The Client ID given by the user.</param>
    /// <param name="clientSecret">The Client Secret given by the user.</param>
    private void LoadUserAPIKeys(out string apiKey, out string clientID, out string clientSecret)
    {
        try
        {
            var apiKeys = JsonDocument.Parse(File.ReadAllText(this.userTokenFileLocation)).RootElement.GetProperty("BungieAPIKeys");

            apiKey = apiKeys.GetProperty("API_Key").ToString();
            clientID = apiKeys.GetProperty("Client_ID").ToString();
            clientSecret = apiKeys.GetProperty("Client_Secret").ToString();
        }
        catch (Exception e)
        {
            throw new Exception($"GVM was unable to read the user provided API keys: {e.Message}");
        }
    }
}
