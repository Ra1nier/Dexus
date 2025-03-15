// <copyright file="ManifestServer.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace RequestManager;

using Microsoft.Extensions.Logging;
using Microsoft.Data.Sqlite;
using System.Text.Json;
using SQLitePCL;
using Models.ItemDetails;
using Models.PerkDetails;

/// <summary>
/// Handles interactions with the Destiny 2 Manifest.
/// </summary>
public class ManifestServer
{
    /// <summary>
    /// Logger used to log actions.
    /// </summary>
    private readonly ILogger logger;

    /// <summary>
    /// Initializes static members of the <see cref="ManifestServer"/> class.
    /// Constructs the SQLite aspect.
    /// </summary>
    static ManifestServer()
    {
        // This is needed I guess for the SQLite provider to work.
        Batteries.Init();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ManifestServer"/> class.
    /// </summary>
    /// <param name="logger">Logger to use.</param>
    public ManifestServer(ILogger logger)
    {
        this.logger = logger;
        this.ManifestPath = string.Empty;
    }

    /// <summary>
    /// Gets or sets the path to the destiny 2 manifest file.
    /// </summary>
    public string ManifestPath { get; set; }

    /// <summary>
    /// Queries the manifest for the associated weapon.
    /// </summary>
    /// <param name="itemHash">Item hash to query.</param>
    /// <returns>A WeaponDetails class representing the manifest details of the specific weapon.</returns>
    public async Task<ItemDetails> Manifest_Query_ItemDetails(uint itemHash)
    {
        // Convert itemHash to manifest id and construct query.
        int id = this.ConvertHashToID(itemHash);
        string query = $"SELECT json FROM DestinyInventoryItemDefinition WHERE id = {id};";

        // Execute Query and get response
        (bool success, string json) = await this.QueryManifest(query);

        if (!success)
        {
            this.logger.LogError($"No data found for ItemHash={itemHash} with ID={id}");
            return new ItemDetails();
        }

        return JsonSerializer.Deserialize<ItemDetails>(json) ?? new ItemDetails();
    }

    /// <summary>
    /// Queries the manifest for the associated bucket.
    /// </summary>
    /// <param name="bucketHash">Bucket hash of the item.</param>
    /// <returns>A JsonDocument that contains the definition for the requested bucket. An empty JsonDocument is returned if the bucket could not be found.</returns>
    public async Task<JsonDocument> Manifest_Query_BucketDefinition(uint bucketHash)
    {
        // Convert itemHash to manifest id and construct query.
        int id = this.ConvertHashToID(bucketHash);
        string query = $"SELECT json FROM DestinyInventoryBucketDefinition WHERE id = {id};";

        // Execute Query and get response
        (bool success, string json) = await this.QueryManifest(query);

        if (!success)
        {
            this.logger.LogError($"No data found for BucketHash={bucketHash} with ID={id}");
        }

        return JsonDocument.Parse(json);
    }

    /// <summary>
    /// Gets the character details of the provided characterHash.
    /// </summary>
    /// <param name="characterHash">CharacterHash to lookup.</param>
    /// <returns>A JsonDocument of the character details. An empty JsonDocument is returned if the character could not be found.</returns>
    public async Task<JsonDocument> Manifest_Query_CharacterDetails(uint characterHash)
    {
        // Convert characterHash to manifest id and construct query.
        int id = this.ConvertHashToID(characterHash);
        string query = $"SELECT json FROM DestinyClassDefinition WHERE id = {id};";

        // Execute Query and get response
        (bool success, string json) = await this.QueryManifest(query);

        if (!success)
        {
            this.logger.LogError($"No data found for CharacterHash={characterHash} with ID={id}");
        }

        return JsonDocument.Parse(json);
    }

    /// <summary>
    /// Queries the manifest for the associated weapon.
    /// </summary>
    /// <param name="perkHash">Item hash to query.</param>
    /// <returns>A WeaponDetails class representing the manifest details of the specific weapon.</returns>
    public async Task<PerkDetails> Manifest_Query_PerkDetails(uint perkHash)
    {
        // Convert itemHash to manifest id and construct query.
        int id = this.ConvertHashToID(perkHash);
        string query = $"SELECT json FROM DestinySandboxPerkDefinition WHERE id = {id};";

        // Execute Query and get response
        (bool success, string json) = await this.QueryManifest(query);

        if (!success)
        {
            this.logger.LogError($"No data found for PerkHash={perkHash} with ID={id}");
            return new PerkDetails();
        }

        return JsonSerializer.Deserialize<PerkDetails>(json) ?? new PerkDetails();
    }

    /// <summary>
    /// Executes the given query on the destiny 2 manifest DB.
    /// </summary>
    /// <param name="query">Query string to execute.</param>
    /// <returns>A Tuple where Item1 is a bool indicating whether the associated JSON was found, and Item2 is a string containing the JSON.</returns>
    private async Task<Tuple<bool, string>> QueryManifest(string query)
    {
        string connectionString = $"Data Source={this.ManifestPath};";

        try
        {
            // Open DB connection
            await using var connection = new SqliteConnection(connectionString);
            await connection.OpenAsync();

            // Build query command.
            await using var command = new SqliteCommand(query, connection);

            // Execute the query
            await using var reader = await command.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
            {
                return new Tuple<bool, string>(false, "{}");
            }

            string json = reader.GetString(0);
            return new Tuple<bool, string>(true, json);
        }
        catch (Exception ex)
        {
            this.logger.LogError($"Error querying manifest: {ex.Message}");
            return new Tuple<bool, string>(false, "{}");
        }
    }

    /// <summary>
    /// Converts the given unsigned 32-bit Hash into its signed 32-bit int ID counterpart.
    /// </summary>
    /// <param name="hash">Hash to convert.</param>
    /// <returns>The signed 32-bit int ID.</returns>
    private int ConvertHashToID(uint hash)
    {
        return unchecked((int)hash);
    }
}