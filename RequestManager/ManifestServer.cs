// <copyright file="ManifestServer.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace RequestManager;

using Microsoft.Extensions.Logging;
using Microsoft.Data.Sqlite;
using System.Text.Json;
using SQLitePCL;

/// <summary>
/// Handles interactions with the Destiny 2 Manifest.
/// </summary>
public class ManifestServer
{
    /// <summary>
    /// Base bungie URL.
    /// </summary>
    private readonly string baseUrl = "https://www.bungie.net";

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
        string connectionString = $"Data Source={this.ManifestPath};";

        try
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                await connection.OpenAsync();
                this.logger.LogTrace($"QUERY MANIFEST: DestinyInventoryItemDefinition of ItemHash={itemHash}");

                // Convert itemHash to signed int
                int id = this.ConvertHashToID(itemHash);

                string query = $@"
                    SELECT json
                    FROM DestinyInventoryItemDefinition
                    WHERE id = @ID;";

                using (var command = new SqliteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", id);

                    // Execute the query
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            string json = reader.GetString(0);

                            var itemDetails = JsonSerializer.Deserialize<ItemDetails>(json);

                            if (itemDetails == null)
                            {
                                throw new Exception($"Failed to deserialize weapon details for itemHash: {itemHash}");
                            }

                            itemDetails.DisplayProperties.Icon = $"{this.baseUrl}{itemDetails.DisplayProperties.Icon}";
                            return itemDetails;
                        }
                        else
                        {
                            this.logger.LogError($"No data found for itemHash={itemHash} with ID={id}");
                            return new ItemDetails();
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            this.logger.LogError($"Error querying manifest: {ex.Message}");
            return new ItemDetails();
        }
    }

    /// <summary>
    /// Queries the manifest for the associated bucket.
    /// </summary>
    /// <param name="bucketHash">Bucket hash of the item.</param>
    /// <returns>A JsonDocument that contains the definition for the requested bucket. An empty JsonDocument is returned if the bucket could not be found.</returns>
    public async Task<JsonDocument> Manifest_Query_BucketDefinition(uint bucketHash)
    {
        string connectionString = $"Data Source={this.ManifestPath};";
        try
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                await connection.OpenAsync();
                this.logger.LogTrace($"QUERY MANIFEST: DestinyInventoryItemDefinition of ItemHash={bucketHash}");

                int id = this.ConvertHashToID(bucketHash);

                string query = $@"
                    SELECT json
                    FROM DestinyInventoryBucketDefinition
                    WHERE id = @ID;";

                using (var command = new SqliteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", id);

                    // Execute the query
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            string json = reader.GetString(0);
                            return JsonDocument.Parse(json);
                        }
                        else
                        {
                            this.logger.LogError($"No data found for itemHash={bucketHash} with ID={id}");
                            return JsonDocument.Parse("{}");
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            this.logger.LogError($"Error querying manifest: {ex.Message}");
            return JsonDocument.Parse("{}");
        }
    }

    /// <summary>
    /// Gets the character details of the provided characterHash.
    /// </summary>
    /// <param name="characterHash">CharacterHash to lookup.</param>
    /// <returns>A JsonDocument of the character details. An empty JsonDocument is returned if the character could not be found.</returns>
    public async Task<JsonDocument> Manifest_Query_CharacterDetails(uint characterHash)
    {
        string connectionString = $"Data Source={this.ManifestPath};";

        try
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                await connection.OpenAsync();
                this.logger.LogTrace($"QUERY MANIFEST: DestinyClassDefinition of CharacterHash={characterHash}");

                int id = this.ConvertHashToID(characterHash);

                string query = $@"
                    SELECT json
                    FROM DestinyClassDefinition
                    WHERE id = @ID;";

                using (var command = new SqliteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", id);

                    // Execute the query
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            string json = reader.GetString(0);

                            return JsonDocument.Parse(json);
                        }
                        else
                        {
                            this.logger.LogError($"No data found for CharacterHash={characterHash} with ID={id}");
                            return JsonDocument.Parse("{}");
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            this.logger.LogError($"Error querying manifest: {ex.Message}");
            return JsonDocument.Parse("{}");
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