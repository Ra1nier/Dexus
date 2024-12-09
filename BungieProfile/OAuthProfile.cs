// <copyright file="OAuthProfile.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace OAuthService;

using Microsoft.Extensions.Configuration;

/// <summary>
/// Contains the needed Bungie access points and tokens in a data class.
/// </summary>
public class OAuthProfile
{
    private readonly string apikey;
    private readonly string clientID;
    private readonly string clientSecret;
    private readonly string state;
    private readonly string authorizationURL = "https://www.bungie.net/en/OAuth/Authorize";
    private readonly string tokenEndpoint = "https://www.bungie.net/Platform/App/OAuth/Token/";
    private readonly string redirectURL = "https://partly-notable-buzzard.ngrok-free.app/callback"; // FIXME: No longer correct.

    /// <summary>
    /// Initializes a new instance of the <see cref="OAuthProfile"/> class.
    /// </summary>
    public OAuthProfile()
    {
        // Grab information from user secrets file
        var bungieSecrets = new ConfigurationBuilder().AddUserSecrets<OAuthProfile>();
        IConfigurationRoot configuration = bungieSecrets.Build();
        var secrets = configuration.GetSection("BungieAPIKeys");

        // Store keys from secrets file
        this.apikey = secrets["API_Key"] ?? string.Empty;
        this.clientID = secrets["Client_ID"] ?? string.Empty;
        this.clientSecret = secrets["Client_Secret"] ?? string.Empty;
        this.state = secrets["State"] ?? string.Empty;
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
    public string State => this.clientSecret;

    /// <summary>
    /// Gets the Authorization URL for Bungie.
    /// </summary>
    public string Authorization_URL => this.authorizationURL;

    /// <summary>
    /// Gets the Token Endpoint URL for Bungie.
    /// </summary>
    public string Token_Endpoint => this.tokenEndpoint;

    /// <summary>
    /// Gets the Redirect URL for Bungie.
    /// </summary>
    public string Redirect_URL => this.redirectURL;

    /// <summary>
    /// Constructs a Authorization URL from the OAuthProfile.
    /// </summary>
    /// <returns>The Authorization URI.</returns>
    public Uri ConstructAuthorizationURL()
    {
        // Add query parameters
        var queryParameters = new Dictionary<string, string>
        {
            { "client_id", this.Client_ID },
            { "response_type", "code" },
            { "state", this.State },
        };

        // Build URI
        var uriBuilder = new UriBuilder(this.Authorization_URL)
        {
            Query = string.Join("&", queryParameters.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}")),
        };

        return uriBuilder.Uri;
    }
}
