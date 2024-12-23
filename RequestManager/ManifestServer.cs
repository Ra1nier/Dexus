// <copyright file="ManifestServer.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace RequestManager;

using Microsoft.Extensions.Logging;
using Microsoft.Data.Sqlite;
using System.Text.Json;
using Dapper;
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
    /// Queries the manifest for the associated weapon details.
    /// </summary>
    /// <param name="itemHash">Item to query.</param>
    /// <returns>A <see cref="Task{WeaponDetails}"/> representing the result of the asynchronous operation.</returns>
    public async Task<WeaponDetails> Manifest_Query_WeaponDetails(long itemHash)
    {
        string connectionString = $"Data Source={this.ManifestPath};";

        try
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                await connection.OpenAsync();
                this.logger.LogDebug("Database connection opened.");

                string query = "SELECT json FROM DestinyInventoryItemDefinition WHERE id = @ID;";

                int sql3ID = unchecked((int)itemHash);

                this.logger.LogTrace($"QUERY MANIFEST: ItemHash={itemHash}, ID={sql3ID}");

                string json = await connection.QueryFirstOrDefaultAsync<string>(query, new { ID = sql3ID });

                if (string.IsNullOrEmpty(json))
                {
                    throw new Exception($"No data found for itemHash: {itemHash}");
                }

                // Deserialize the JSON into the WeaponDetails class
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var weaponDetails = JsonSerializer.Deserialize<WeaponDetails>(json, options);

                if (weaponDetails == null)
                {
                    throw new Exception($"Failed to deserialize weapon details for itemHash: {itemHash}");
                }

                // Append the base URL to the icon path
                weaponDetails.IconPath = $"{this.baseUrl}{weaponDetails.IconPath}";

                return weaponDetails;
            }
        }
        catch (Exception ex)
        {
            this.logger.LogError($"Error querying manifest: {ex.Message}");
            throw;
        }
    }
}
