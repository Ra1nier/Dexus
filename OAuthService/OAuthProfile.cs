// <copyright file="OAuthProfile.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace OAuthService;

using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;

/// <summary>
/// Contains the necessary Bungie access points and tokens in a data class.
/// </summary>
public class OAuthProfile
{
    // OAuth Variables
    private readonly string authorizationURL = "https://www.bungie.net/en/OAuth/Authorize";
    private readonly string tokenEndpoint = "https://www.bungie.net/Platform/App/OAuth/Token/";
    private string state;
    private string apiKey = string.Empty;
    private string clientID = string.Empty;
    private string clientSecret = string.Empty;
    private bool usesConfiguredAPIKeys;

    /// <summary>
    /// Initializes a new instance of the <see cref="OAuthProfile"/> class.
    /// </summary>
    public OAuthProfile()
        : this(null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="OAuthProfile"/> class.
    /// </summary>
    /// <param name="configuration">Application configuration used to load hosted Bungie API keys.</param>
    public OAuthProfile(IConfiguration? configuration)
    {
        // Profile Configuration
        this.state = this.GenerateRandomStateValue();

        this.LoadConfiguredAPIKeys(configuration);
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
    /// Gets a value indicating whether API keys were loaded from application configuration.
    /// </summary>
    public bool UsesConfiguredAPIKeys => this.usesConfiguredAPIKeys;

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
    /// Creates a fresh OAuth state value for a new authorization request.
    /// </summary>
    /// <returns>The new state value.</returns>
    public string RefreshState()
    {
        this.state = this.GenerateRandomStateValue();
        return this.state;
    }

    /// <summary>
    /// Validates an OAuth state value using a fixed-time comparison.
    /// </summary>
    /// <param name="candidate">State value returned by the OAuth provider.</param>
    /// <returns>True when the state value matches the current request.</returns>
    public bool ValidateState(string? candidate)
    {
        if (string.IsNullOrEmpty(candidate))
        {
            return false;
        }

        byte[] expectedBytes = Encoding.UTF8.GetBytes(this.state);
        byte[] candidateBytes = Encoding.UTF8.GetBytes(candidate);
        return expectedBytes.Length == candidateBytes.Length
            && CryptographicOperations.FixedTimeEquals(expectedBytes, candidateBytes);
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
    /// Generates a random state value to be used in Authorization URI creation.
    /// </summary>
    /// <returns>The state string.</returns>
    private string GenerateRandomStateValue()
    {
        return Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
    }

    /// <summary>
    /// Loads Bungie API keys from application configuration.
    /// </summary>
    /// <param name="configuration">Application configuration.</param>
    private void LoadConfiguredAPIKeys(IConfiguration? configuration)
    {
        if (configuration is null)
        {
            return;
        }

        IConfigurationSection apiKeys = configuration.GetSection("BungieAPIKeys");

        string configuredApiKey = apiKeys["API_Key"] ?? string.Empty;
        string configuredClientID = apiKeys["Client_ID"] ?? string.Empty;
        string configuredClientSecret = apiKeys["Client_Secret"] ?? string.Empty;

        if (string.IsNullOrWhiteSpace(configuredApiKey)
            || string.IsNullOrWhiteSpace(configuredClientID)
            || string.IsNullOrWhiteSpace(configuredClientSecret))
        {
            return;
        }

        this.apiKey = configuredApiKey;
        this.clientID = configuredClientID;
        this.clientSecret = configuredClientSecret;
        this.usesConfiguredAPIKeys = true;
    }
}
