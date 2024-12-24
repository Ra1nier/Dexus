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
                this.logger.LogTrace($"QUERY MANIFEST: ItemHash={itemHash}");

                // Convert itemHash to signed int
                long id = unchecked((int)itemHash);

                string query = $@"
                    SELECT json
                    FROM DestinyInventoryItemDefinition
                    WHERE id = @ID;";

                using (var command = new SqliteCommand(query, connection))
                {
                    // Add parameters for signed and unsigned interpretations
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
}