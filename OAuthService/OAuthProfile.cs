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
    private readonly string state;
    private readonly string authorizationURL = "https://www.bungie.net/en/OAuth/Authorize";
    private readonly string tokenEndpoint = "https://www.bungie.net/Platform/App/OAuth/Token/";
    private string apiKey = string.Empty;
    private string clientID = string.Empty;
    private string clientSecret = string.Empty;

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

        // Profile Configuration
        this.state = this.GenerateRandomStateValue();
    }

    /// <summary>
    /// Gets the API key for GuardianVaultManger given by Bungie.
    /// </summary>
    public string API_Key => this.apiKey;

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
    /// Checks if the user has provided API Keys.
    /// </summary>
    /// <returns>True if all API keys exist, False otherwise.</returns>
    public bool HasValidAPIKeys()
    {
        return !string.IsNullOrEmpty(this.apiKey) && !string.IsNullOrEmpty(this.clientID) && !string.IsNullOrEmpty(this.clientSecret);
    }

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
    /// Constructs the User's API File based on the user provided keys.
    /// </summary>
    /// <param name="apiKey">API to use for Bungie's API.</param>
    /// <param name="clientID">Client ID of the User.</param>
    /// <param name="clientSecret">Client Secret of the user.</param>
    /// <returns>True if completed successfully, False otherwise.</returns>
    public bool ConstructUserAPIFile(string apiKey, string clientID, string clientSecret)
    {
        this.EnsureDataFolderExists();

        try
        {
            var obj = new
            {
                BungieAPIKeys = new
                {
                    API_Key = apiKey,
                    Client_ID = clientID,
                    Client_Secret = clientSecret,
                },
            };

            string json = JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(this.userTokenFileLocation, json);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// Loads the API Keys provided by the user.
    /// </summary>
    /// <returns>True if completed successfully, False otherwise.</returns>
    public bool LoadUserAPIKeys()
    {
        try
        {
            var apiKeys = JsonDocument.Parse(File.ReadAllText(this.userTokenFileLocation)).RootElement.GetProperty("BungieAPIKeys");

            this.apiKey = apiKeys.GetProperty("API_Key").ToString();
            this.clientID = apiKeys.GetProperty("Client_ID").ToString();
            this.clientSecret = apiKeys.GetProperty("Client_Secret").ToString();

            return true;
        }
        catch (Exception)
        {
            return false;
        }
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
    /// Ensures that the data folder exists.
    /// </summary>
    private void EnsureDataFolderExists()
    {
        if (!Directory.Exists(this.dataFolderPath))
        {
            Directory.CreateDirectory(this.dataFolderPath);
        }
    }
}
