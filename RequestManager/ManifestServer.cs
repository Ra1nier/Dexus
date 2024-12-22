// <copyright file="ManifestServer.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace RequestManager;

using Microsoft.Extensions.Logging;
using Microsoft.Data.Sqlite;

/// <summary>
/// Handles interactions with the Destiny 2 Manifest.
/// </summary>
public class ManifestServer
{
    /// <summary>
    /// Path to the downloaded Destiny 2 Manifest file.
    /// </summary>
    private readonly string manifestPath = "C:\\Users\\maxbr\\source\\repos\\Ra1nier\\GuardianVaultManager\\RequestManager\\Manifest\\world_sql_content_en.content";

    /// <summary>
    /// Logger used to log actions.
    /// </summary>
    private ILogger logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ManifestServer"/> class.
    /// </summary>
    /// <param name="logger">Logger to use.</param>
    public ManifestServer(ILogger logger)
    {
        this.logger = logger;
    }

    /// <summary>
    /// Queries the manifest for the associated weapon photo.
    /// </summary>
    /// <returns>?.</returns>
    public async Task Manifest_Query_WeaponPhoto()
    {

    }
}
