// <copyright file="Profile.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace ClientProfile;

using System.Text.Json;

/// <summary>
/// Contains the definition of a Profle created from Bungie.
/// </summary>
public class Profile
{
    /// <summary>
    /// Path to store files related to user tokens.
    /// </summary>
    private readonly string userTokenFileLocation = "/data/UserTokens.json";

    /// <summary>
    /// Stores all user tokens.
    /// </summary>
    private Tokens tokens;

    /// <summary>
    /// Initializes a new instance of the <see cref="Profile"/> class.
    /// </summary>
    public Profile()
    {
        this.tokens = new Tokens();
    }

    /// <summary>
    /// Gets the Access Token for the client.
    /// </summary>
    public string AccessToken { get => this.tokens.AccessToken ?? string.Empty; }

    /// <summary>
    /// Gets the Access Token Expire Date for the client.
    /// </summary>
    public int AccessTokenExpireDate { get => this.tokens.ExpiresIn; }

    /// <summary>
    /// Gets the Refresh Token for the client.
    /// </summary>
    public string RefreshToken { get => this.tokens.RefreshToken ?? string.Empty; }

    /// <summary>
    /// Gets the Refresh Token Expire Date for the client.
    /// </summary>
    public int RefreshTokenExpireDate { get => this.tokens.RefreshExpiresIn; }

    /// <summary>
    /// Gets the Membership ID for the client.
    /// </summary>
    public string MembershipID { get => this.tokens.MembershipId ?? string.Empty; }

    ///// <summary>
    ///// Gets the Destiny Membership ID for the client.
    ///// </summary>
    // public string DestinyMembershipID { get => this.tokens.DestinyMembershipID; }

    ///// <summary>
    ///// Gets the Membership Type for the client.
    ///// </summary>
    // public string MembershipType { get => this.tokens.MembershipType; }

    /// <summary>
    /// Creates a new Client Profile containing the nessisary keys for the user.
    /// </summary>
    /// <returns>True if the operation was completed successfully.</returns>
    /// <exception cref="Exception">Thrown if the profile fails to create. Will not stop the program.</exception>
    public bool Create()
    {
        try
        {
            this.LoadUserTokens(this.userTokenFileLocation);
        }
        catch (Exception)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Stores the current tokens in te profile object into the file.
    /// </summary>
    /// <param name="path">Path to the user tokens file.</param>
    /// <exception cref="IOException">Thrown if the file containing the user tokens could not be created.</exception>
    /// <returns>True of tokens were stored.</returns>
    public bool StoreUserTokens(string path)
    {
        try
        {
            string userTokensJSON = JsonSerializer.Serialize<Tokens>(this.tokens);

            File.WriteAllText(path, userTokensJSON);

            return true;
        }
        catch (Exception e)
        {
            return false;
            throw new IOException($"Could not write tokens to user tokens file: {e.Message}");
        }
    }

    /// <summary>
    /// Loads the user tokens stored in the file.
    /// </summary>
    /// <param name="path">Path to the user tokens file.</param>
    /// <exception cref="IOException">Thrown if the tokens could not be loaded from the file.</exception>
    private void LoadUserTokens(string path)
    {
        try
        {
            this.tokens = JsonSerializer.Deserialize<Tokens>(path) ?? new Tokens(); // Find better solution then just assigning empty token object.
        }
        catch (Exception e)
        {
            throw new IOException($"Could not load tokens from the specified user tokens file: {e.Message}");
        }
    }
}
