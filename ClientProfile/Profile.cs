// <copyright file="Profile.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace ClientProfile;

using System.Text.Json;

/// <summary>
/// Contains the definition of a Profile created from Bungie.
/// </summary>
public class Profile
{
    /// <summary>
    /// Path to store files related to user tokens.
    /// </summary>
    private string dataFolderPath;
    private string userTokenFileLocation;
    private string destinyMembershipsFileLocation;
    private string expiryDatesFileLocation;

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

        // Make user paths
        this.dataFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ClientProfile", "data");
        this.userTokenFileLocation = Path.Combine(this.dataFolderPath, "UserTokens.json");
        this.destinyMembershipsFileLocation = Path.Combine(this.dataFolderPath, "DestinyMemberships.json");
        this.expiryDatesFileLocation = Path.Combine(this.dataFolderPath, "ExpiryDates.json");
    }

    /// <summary>
    /// Gets the Access Token for the client.
    /// </summary>
    public string AccessToken { get => this.tokens.AccessToken ?? string.Empty; }

    /// <summary>
    /// Gets the Access Token Expire Date for the client.
    /// </summary>
    public DateTime AccessTokenExpireDate { get; private set; }

    /// <summary>
    /// Gets the Refresh Token for the client.
    /// </summary>
    public string RefreshToken { get => this.tokens.RefreshToken ?? string.Empty; }

    /// <summary>
    /// Gets the Refresh Token Expire Date for the client.
    /// </summary>
    public DateTime RefreshTokenExpireDate { get; private set; }

    /// <summary>
    /// Gets the Membership ID for the client.
    /// </summary>
    public string MembershipID { get => this.tokens.MembershipId ?? string.Empty; }

    /// <summary>
    /// Gets the Destiny Membership ID for the client.
    /// </summary>
    public string? DestinyMembershipID { get; private set; }

    /// <summary>
    /// Gets the Membership Type for the client.
    /// </summary>
    public string? MembershipType { get; private set; }

    /// <summary>
    /// Creates a new Client Profile containing the necessary keys for the user.
    /// </summary>
    /// <returns>True if the operation was completed successfully.</returns>
    /// <exception cref="Exception">Thrown if the profile fails to create. Will not stop the program.</exception>
    public bool Create()
    {
        try
        {
            this.EnsureDataFolderExists();

            this.LoadUserTokens(this.userTokenFileLocation);

            this.LoadExpiryDates(this.expiryDatesFileLocation);
        }
        catch (Exception)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Loads the Destiny Membership ID from file.
    /// </summary>
    /// <returns>True if the operation completed successfully, False otherwise.</returns>
    public bool LoadDestinyID()
    {
        try
        {
            this.LoadDestinyID(this.destinyMembershipsFileLocation);
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
    /// <exception cref="IOException">Thrown if the file containing the user tokens could not be created.</exception>
    /// <returns>True of tokens were stored.</returns>
    public bool StoreUserTokens()
    {
        try
        {
            string userTokensJSON = JsonSerializer.Serialize(this.tokens);

            File.WriteAllText(this.userTokenFileLocation, userTokensJSON);

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// Stores the current tokens in the profile object into the file. When stored the method will also calculate and the expiry dates of the tokens.
    /// </summary>
    /// <param name="json">Json text to write file to.</param>
    /// <returns>Thrown if the file containing the user tokens could not be created.</returns>
    /// <exception cref="IOException">The file could not be written to.</exception>
    public bool StoreUserTokens(string json)
    {
        try
        {
            File.WriteAllText(this.userTokenFileLocation, json);

            JsonDocument tokens = JsonDocument.Parse(json);

            int accessExpiryTime = tokens.RootElement.GetProperty("expires_in").GetInt32();
            int refreshExpiryTime = tokens.RootElement.GetProperty("refresh_expires_in").GetInt32();

            this.StoreExpiryDates(accessExpiryTime, refreshExpiryTime);

            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Exception occurred when attempting to store user tokens: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// Stores the DestinyMembershipID into the file.
    /// </summary>
    /// <param name="destinyMembershipID">Destiny ID retrieved from Bungie.</param>
    /// <param name="membershipType">Membership Type retrieved from Bungie.</param>
    /// <returns>Thrown if the file containing the user tokens could not be created.</returns>
    /// <exception cref="IOException">The file could not be written to.</exception>
    public bool StoreDestinyMembershipID(string destinyMembershipID, string membershipType)
    {
        try
        {
            string jsonString = $"{{\"MembershipID\":\"{destinyMembershipID}\",\"MembershipType\":\"{membershipType}\"}}";

            File.WriteAllText(this.destinyMembershipsFileLocation, jsonString);

            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Exception occurred when trying to write the users membershipID: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// Loads the user tokens stored in the file.
    /// </summary>
    /// <param name="tokenPath">Path to the user tokens file.</param>
    /// <exception cref="IOException">Thrown if the tokens could not be loaded from the file.</exception>
    private void LoadUserTokens(string tokenPath)
    {
        try
        {
            string json = File.ReadAllText(tokenPath);

            this.tokens = JsonSerializer.Deserialize<Tokens>(json) ?? new Tokens(); // Find better solution then just assigning empty token object.
        }
        catch (Exception e)
        {
            throw new IOException($"Could not load tokens from the specified user tokens file: {e.Message}");
        }
    }

    /// <summary>
    /// Loads the user tokens stored in the file.
    /// </summary>
    /// <param name="destinyIDPath">Path to the user tokens file.</param>
    /// <exception cref="IOException">Thrown if the tokens could not be loaded from the file.</exception>
    private void LoadDestinyID(string destinyIDPath)
    {
        try
        {
            JsonDocument membershipsFile = JsonDocument.Parse(File.ReadAllText(destinyIDPath));

            this.DestinyMembershipID = membershipsFile.RootElement.GetProperty("MembershipID").GetString();
            this.MembershipType = membershipsFile.RootElement.GetProperty("MembershipType").GetString();
        }
        catch (Exception e)
        {
            throw new IOException($"Could not load tokens from the specified user tokens file: {e.Message}");
        }
    }

    /// <summary>
    /// Stores the calculated expiry times for the access and refresh tokens.
    /// </summary>
    /// <param name="accessExpiryTime">Time until the access token expires.</param>
    /// <param name="refreshExpiryTime">Time until the refresh token expires.</param>
    private bool StoreExpiryDates(int accessExpiryTime, int refreshExpiryTime)
    {
        try
        {
            DateTime accessExpiryDate = DateTime.Now.AddSeconds(accessExpiryTime);
            DateTime refreshExpiryDate = DateTime.Now.AddSeconds(refreshExpiryTime);

            // Manually construct the JSON string
            string jsonString = $"{{\"AccessExpiryDate\":\"{accessExpiryDate:O}\",\"RefreshExpiryDate\":\"{refreshExpiryDate:O}\"}}";

            File.WriteAllText(this.expiryDatesFileLocation, jsonString);

            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Exception occurred when trying to store key expiry dates: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// Stores the calculated expiry times for the access and refresh tokens.
    /// </summary>
    private bool LoadExpiryDates(string expiryDatesLocation)
    {
        try
        {
            JsonDocument expiryTimes = JsonDocument.Parse(File.ReadAllText(expiryDatesLocation));

            this.AccessTokenExpireDate = expiryTimes.RootElement.GetProperty("AccessExpiryDate").GetDateTime();
            this.RefreshTokenExpireDate = expiryTimes.RootElement.GetProperty("RefreshExpiryDate").GetDateTime();

            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Exception occurred when attempting to read key expiry dates: {e.Message}");
            return false;
        }
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
