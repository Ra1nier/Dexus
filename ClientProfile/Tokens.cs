// <copyright file="Tokens.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace ClientProfile;

using System.Text.Json.Serialization;

/// <summary>
/// Object that stores user tokens.
/// </summary>
internal class Tokens
{
    /// <summary>
    /// Gets or sets the Access Token for the client.
    /// </summary>
    [JsonPropertyName("access_token")]
    public string? AccessToken { get; set; }

    /// <summary>
    /// Gets or sets the Token Type for the client.
    /// </summary>
    [JsonPropertyName("token_type")]
    public string? TokenType { get; set; }

    /// <summary>
    /// Gets or sets the Access Token Expire Date for the client.
    /// </summary>
    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    /// <summary>
    /// Gets or sets the Refresh Token for the client.
    /// </summary>
    [JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; set; }

    /// <summary>
    /// Gets or sets the Refresh Token Expire Date for the client.
    /// </summary>
    [JsonPropertyName("refresh_expires_in")]
    public int RefreshExpiresIn { get; set; }

    /// <summary>
    /// Gets or sets the Membership ID for the client.
    /// </summary>
    [JsonPropertyName("membership_id")]
    public string? MembershipId { get; set; }
}
