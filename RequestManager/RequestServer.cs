// <copyright file="RequestServer.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace RequestManager;

using ClientProfile;
using Microsoft.Extensions.Logging;
using OAuthService;
using System.IO.Compression;
using System.Text;
using System.Text.Json;

/// <summary>
/// Handles requests to bungie and returning the result to the application.
/// </summary>
public class RequestServer
{
    private HttpClient client;
    private Profile profile;
    private OAuthProfile auth;

    // Logger
    private ILogger logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RequestServer"/> class.
    /// </summary>
    /// <param name="profile">Client Profile.</param>
    /// <param name="auth">OAuth Profile.</param>
    /// <param name="logger">logging service.</param>
    public RequestServer(ILogger logger, Profile profile, OAuthProfile auth)
    {
        this.client = new HttpClient();
        this.profile = profile;
        this.auth = auth;
        this.logger = logger;
    }

    /// <summary>
    /// Sends a request to Bungie for the user's MembershipID.
    /// </summary>
    /// <returns>The primary membershipID or an error message.</returns>
    public async Task<string[]> PlatformDestiny2_Request_MembershipIDAsync()
    {
        string[] result = new string[2];
        this.logger.LogInformation("REQUEST: Destiny Membership ID");

        // Construct HTTP request
        string requestURI = $"https://www.bungie.net/Platform/Destiny2/-1/Profile/{this.profile.MembershipID}/LinkedProfiles/?getAllMemberships=false";
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestURI);

        // Add headers
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", this.profile.AccessToken);
        request.Headers.Add("X-API-Key", this.auth.API_Key);

        // Send request
        try
        {
            HttpResponseMessage response = await this.client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                string jsonResponse = await response.Content.ReadAsStringAsync();

                // Parse the JSON response
                using JsonDocument document = JsonDocument.Parse(jsonResponse);
                JsonElement responseRoot = document.RootElement.GetProperty("Response");

                string membershipId = "Error";

                // Look for the primary profile where isCrossSavePrimary == true
                if (responseRoot.TryGetProperty("profiles", out JsonElement profilesElement))
                {
                    foreach (var profile in profilesElement.EnumerateArray())
                    {
                        bool isCrossSavePrimary = profile.GetProperty("isCrossSavePrimary").GetBoolean();
                        int crossSaveOverride = profile.GetProperty("crossSaveOverride").GetInt32();

                        if (isCrossSavePrimary || crossSaveOverride != 0)
                        {
                            membershipId = profile.GetProperty("membershipId").GetString();
                            this.logger.LogDebug($"Primary Membership ID found: {membershipId}");

                            result[1] = crossSaveOverride.ToString();
                            result[0] = membershipId;

                            return result;
                        }
                    }
                }

                this.logger.LogDebug($"JSON Response: No valid membership ID found. {jsonResponse}");
                result[0] = "Error";

                return result;
            }
            else
            {
                string errorResponse = await response.Content.ReadAsStringAsync();
                this.logger.LogError($"Error Response: {errorResponse}");
                result[0] = "Error";

                return result;
            }
        }
        catch (Exception e)
        {
            this.logger.LogError($"REQUEST FAILED: Destiny Membership ID, {e.Message}");
            result[0] = "Error";

            return result;
        }
    }

    /// <summary>
    /// Sends a request to Bungie for the given content type.
    /// </summary>
    /// <param name="contentType">Content to request from bungie.</param>
    /// <param name="includeDefinitions">Includes the definitions if true, ignores if false.</param>
    /// <returns>The JSON response from bungie.</returns>
    public async Task<string> PlatformDestiny2_Request_Contents(string contentType, bool includeDefinitions)
    {
        // Convert componentType to a component
        int content = ComponentTypeLookup.GetContent(contentType);
        this.logger.LogInformation($"REQUEST: Destiny Profile Content ({contentType}={content})");

        // Construct HTTP request
        string requestURI = $"https://www.bungie.net/Platform/Destiny2/{this.profile.MembershipType}/Profile/{this.profile.DestinyMembershipID}/?components={content}&definitions={includeDefinitions}";
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestURI);

        // Add headers
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", this.profile.AccessToken);
        request.Headers.Add("X-API-Key", this.auth.API_Key);

        // Send request
        try
        {
            HttpResponseMessage response = await this.client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                string jsonResponse = await response.Content.ReadAsStringAsync();

                this.logger.LogDebug($"Successful JSON Response: Content ({contentType}={content})");

                return jsonResponse;
            }
            else
            {
                string errorResponse = await response.Content.ReadAsStringAsync();
                this.logger.LogError($"Error Response: {errorResponse}");
                return "Error";
            }
        }
        catch (Exception e)
        {
            this.logger.LogError($"REQUEST FAILED: Destiny Profile Content {contentType}={content}, {e.Message}");
            return "Error";
        }
    }

    /// <summary>
    /// Pings bungie to request a item transfer to a new location.
    /// </summary>
    /// <param name="membershipType">MembershipType of the client.</param>
    /// <param name="itemHash">The ItemHash of the item to transfer.</param>
    /// <param name="itemID">The ItemInstanceID of the item to transfer.</param>
    /// <param name="stackSize">Stack Size of the item.</param>
    /// <param name="characterID">Character ID of either the origin point to character to transfer to based on vaultTransfer.</param>
    /// <param name="vaultTransfer">Flag that indicates whether the item is being transferred is Character->Vault.</param>
    /// <returns>True if bungie returns a successful response, false otherwise.</returns>
    public async Task<bool> ActionItem_Request_TransferItem(string membershipType, long itemHash, string itemID, int stackSize, string characterID, bool vaultTransfer)
    {
        this.logger.LogInformation($"REQUEST: Destiny Item Transfer - {itemID} TO " + (vaultTransfer ? "Vault" : $"Character({characterID})"));

        // Construct HTTP request
        string requestURI = $"https://www.bungie.net/Platform/Destiny2/Actions/Items/TransferItem/";
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, requestURI);

        // Construct request body in JSON format
        var body = new
        {
            membershipType = membershipType,
            itemHash = itemHash,
            itemID = itemID,
            stackSize = stackSize,
            characterID = characterID,
            vaultTransfer = vaultTransfer,
        };
        string jsonBody = JsonSerializer.Serialize(body);
        request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

        // Add headers
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", this.profile.AccessToken);
        request.Headers.Add("X-API-Key", this.auth.API_Key);

        // Send request
        try
        {
            HttpResponseMessage response = await this.client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                string jsonResponse = await response.Content.ReadAsStringAsync();

                this.logger.LogDebug($"Successful Response: Transfer of {itemID} successful");

                return true;
            }
            else
            {
                string errorResponse = await response.Content.ReadAsStringAsync();
                this.logger.LogError($"Error Response: {errorResponse}");
                return false;
            }
        }
        catch (Exception e)
        {
            this.logger.LogError($"REQUEST FAILED: Transfer - {itemID} TO " + (vaultTransfer ? "Vault" : $"Character({characterID})") + ", {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// Get the most current Destiny 2 manifest from Bungie.
    /// </summary>
    /// <returns>The path to the saved manifest.</returns>
    public async Task<string> Destiny2Manifest_Request_GetCurrentManifest()
    {
        this.logger.LogInformation("REQUEST: Destiny Manifest");

        string requestURI = "https://www.bungie.net/Platform/Destiny2/Manifest/";
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestURI);

        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", this.profile.AccessToken);
        request.Headers.Add("X-API-Key", this.auth.API_Key);

        try
        {
            HttpResponseMessage response = await this.client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                string errorResponse = await response.Content.ReadAsStringAsync();
                this.logger.LogError("Failed to retrieve manifest dictionary: {ErrorResponse}", errorResponse);
                return string.Empty;
            }

            // Parse manifest dictionary
            string manifestDictionaryResponse = await response.Content.ReadAsStringAsync();
            using JsonDocument manifestDictionaryDocument = JsonDocument.Parse(manifestDictionaryResponse);
            JsonElement englishManifestPathElement = manifestDictionaryDocument
                .RootElement
                .GetProperty("Response")
                .GetProperty("mobileWorldContentPaths")
                .GetProperty("en");

            string englishManifestPath = englishManifestPathElement.GetString();
            if (string.IsNullOrEmpty(englishManifestPath))
            {
                this.logger.LogError("Error: English content path not found in manifest.");
                return string.Empty;
            }

            string databaseUrl = $"https://www.bungie.net{englishManifestPath}";

            // Request manifest file
            HttpResponseMessage manifestResponse = await this.client.GetAsync(databaseUrl);
            if (!manifestResponse.IsSuccessStatusCode)
            {
                string errorResponse = await manifestResponse.Content.ReadAsStringAsync();
                this.logger.LogError("Manifest could not be downloaded: {ErrorResponse}", errorResponse);
                return string.Empty;
            }

            // Setup directories and file paths
            string saveDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "RequestManager", "Manifest");
            string zipFilePath = Path.Combine(saveDir, "manifest.zip");

            if (Directory.Exists(saveDir))
            {
                Directory.Delete(saveDir, true);
            }
            Directory.CreateDirectory(saveDir);

            // Save the downloaded manifest zip file
            await using (Stream manifestFileStream = await manifestResponse.Content.ReadAsStreamAsync())
            {
                await using FileStream fileStream = new FileStream(zipFilePath, FileMode.Create, FileAccess.Write);
                await manifestFileStream.CopyToAsync(fileStream);
            }

            // Extract the manifest zip file
            try
            {
                ZipFile.ExtractToDirectory(zipFilePath, saveDir, overwriteFiles: true);
                File.Delete(zipFilePath);
            }
            catch (Exception ex)
            {
                this.logger.LogError("Error extracting manifest zip file: {ErrorMessage}", ex.Message);
                return string.Empty;
            }

            // Find and return the extracted manifest file
            string[] manifestFiles = Directory.GetFiles(saveDir, "*.content", SearchOption.TopDirectoryOnly);
            if (manifestFiles.Length == 0)
            {
                this.logger.LogError("Error: No manifest database file found after extraction.");
                return string.Empty;
            }

            string manifestFilePath = manifestFiles[0];
            this.logger.LogInformation("Manifest saved successfully: {ManifestFilePath}", manifestFilePath);
            return manifestFilePath;
        }
        catch (Exception ex)
        {
            this.logger.LogError("REQUEST FAILED: Destiny Manifest - {ErrorMessage}", ex.Message);
            return string.Empty;
        }
    }

    /// <summary>
    /// Sends a request to Bungie for the given content type.
    /// </summary>
    /// <returns>The JSON response from bungie.</returns>
    public async Task<ItemDetails> PlatformDestiny2_Request_ItemDefinition(uint itemHash)
    {
        this.logger.LogInformation($"REQUEST: Destiny Item Definition: {itemHash}");

        // Construct HTTP request
        string requestURI = $"https://www.bungie.net/Platform/Destiny2/Manifest/DestinyInventoryItemDefinition/{itemHash}";
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestURI);

        // Add headers
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", this.profile.AccessToken);
        request.Headers.Add("X-API-Key", this.auth.API_Key);

        // Send request
        try
        {
            HttpResponseMessage response = await this.client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                string jsonResponse = await response.Content.ReadAsStringAsync();

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                ItemDetails details = JsonSerializer.Deserialize<ItemDetails>(jsonResponse);

                this.logger.LogDebug($"Successful Response: {details.DisplayProperties.Name}");

                return details;
            }
            else
            {
                string errorResponse = await response.Content.ReadAsStringAsync();
                this.logger.LogError($"Error Response: {errorResponse}");
                return new ItemDetails();
            }
        }
        catch (Exception e)
        {
            this.logger.LogError($"REQUEST FAILED: Destiny Item Definition {itemHash}, {e.Message}");
            return new ItemDetails();
        }
    }
}