// <copyright file="OAuthProfile.cs" company="Max Broome">
// Copyright (c) Max Broome. All rights reserved.
// </copyright>

namespace OAuthManager;

using Microsoft.Extensions.Configuration;

/// <summary>
/// Contains the needed Bungie access points and tokens in a data class.
/// </summary>
public class OAuthProfile
{
    private readonly string apikey;
    private readonly string clientID;
    private readonly string clientSecret;
    private readonly string authorizationURL = "https://www.bungie.net/en/OAuth/Authorize";
    private readonly string tokenEndpoint = "https://www.bungie.net/Platform/App/OAuth/Token/";
    private readonly string redirectURL = "https://partly-notable-buzzard.ngrok-free.app/callback";

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
        this.apikey = secrets["API_Key"];
        this.clientID = secrets["Client_ID"];
        this.clientSecret = secrets["Client_Secret"];
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
}
