// <copyright file="ContentManager.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace ContentManager;

using Microsoft.Extensions.Logging;
using Models;
using RequestManager;
using System.Text.Json;

/// <summary>
/// The ContentManager class is responsible for managing the content of GVM.
/// </summary>
public class ContentManager
{
    /// <summary>
    /// Logger for GVM.
    /// </summary>
    private ILogger logger;

    /// <summary>
    /// Request Server for GVM.
    /// </summary>
    private RequestServer request;

    /// <summary>
    /// Manifest Server for GVM.
    /// </summary>
    private ManifestServer manifest;

    // Client specific objects
    private List<Character> characters = new List<Character>();

    // Sub Components for each character
    private List<Item> weapons = new List<Item>();
    private List<Item> kineticWeapons = new List<Item>();
    private List<Item> energyWeapons = new List<Item>();
    private List<Item> powerWeapons = new List<Item>();

    private List<Item> armor = new List<Item>();
    private List<Item> helmets = new List<Item>();
    private List<Item> gauntlets = new List<Item>();
    private List<Item> chests = new List<Item>();
    private List<Item> legs = new List<Item>();
    private List<Item> skirts = new List<Item>();

    private List<Item> accessories = new List<Item>();
    private List<Item> postmaster = new List<Item>();
    private List<Item> engrams = new List<Item>();
    private List<Item> quests = new List<Item>();

    // Non Specific Sub Components
    private List<Item> clientComponents = new List<Item>();

    private List<Item> consumables = new List<Item>();
    private List<Item> extras = new List<Item>();

    // Complete Components
    private List<Item> dropzoneBackingStore = new List<Item>();
    private Dictionary<uint, ItemDetails> itemDetails = new Dictionary<uint, ItemDetails>();

    /// <summary>
    /// Initializes a new instance of the <see cref="ContentManager"/> class.
    /// </summary>
    /// <param name="logger">GVM Logger.</param>
    /// <param name="request">GVM request server.</param>
    /// <param name="manifest">GVM manifest server.</param>
    public ContentManager(ILogger logger, RequestServer request, ManifestServer manifest)
    {
        this.logger = logger;
        this.request = request;
        this.manifest = manifest;
    }

    /// <summary>
    /// Gets the Characters List.
    /// </summary>
    public List<Character> Characters { get => this.characters; }

    /// <summary>
    /// Gets the Weapons List.
    /// </summary>
    public List<Item> Weapons { get => this.weapons; }

    /// <summary>
    /// Gets the KineticWeapons List.
    /// </summary>
    public List<Item> KineticWeapons { get => this.kineticWeapons; }

    /// <summary>
    /// Gets the EnergyWeapons List.
    /// </summary>
    public List<Item> EnergyWeapons { get => this.energyWeapons; }

    /// <summary>
    /// Gets the PowerWeapons List.
    /// </summary>
    public List<Item> PowerWeapons { get => this.powerWeapons; }

    /// <summary>
    /// Gets the Armor List.
    /// </summary>
    public List<Item> Armor { get => this.armor; }

    /// <summary>
    /// Gets the Helmets List.
    /// </summary>
    public List<Item> Helmets { get => this.helmets; }

    /// <summary>
    /// Gets the Gauntlets List.
    /// </summary>
    public List<Item> Gauntlets { get => this.gauntlets; }

    /// <summary>
    /// Gets the Chests List.
    /// </summary>
    public List<Item> Chests { get => this.chests; }

    /// <summary>
    /// Gets the Legs List.
    /// </summary>
    public List<Item> Legs { get => this.legs; }

    /// <summary>
    /// Gets the Skirts List.
    /// </summary>
    public List<Item> Skirts { get => this.skirts; }

    /// <summary>
    /// Gets the Accessories List.
    /// </summary>
    public List<Item> Accessories { get => this.accessories; }

    /// <summary>
    /// Gets the Postmaster List.
    /// </summary>
    public List<Item> Postmaster { get => this.postmaster; }

    /// <summary>
    /// Gets the Engrams List.
    /// </summary>
    public List<Item> Engrams { get => this.engrams; }

    /// <summary>
    /// Gets the Quests List.
    /// </summary>
    public List<Item> Quests { get => this.quests; }

    /// <summary>
    /// Gets the ClientComponents List.
    /// </summary>
    public List<Item> ClientComponents { get => this.clientComponents; }

    /// <summary>
    /// Gets the consumables List.
    /// </summary>
    public List<Item> Consumables { get => this.consumables; }

    /// <summary>
    /// Gets the Extras List.
    /// </summary>
    public List<Item> Extras { get => this.extras; }

    /// <summary>
    /// Gets the DropzoneBackingStore List.
    /// </summary>
    public List<Item> DropzoneBackingStore { get => this.dropzoneBackingStore; }

    /// <summary>
    /// Gets the Details List.
    /// </summary>
    public Dictionary<uint, ItemDetails> Details { get => this.itemDetails; }

    /// <summary>
    /// Gets the clients content.
    /// </summary>
    /// <returns>A Task representing the function.</returns>
    public async Task InitaliseClientContent()
    {
        // Fetch Characters
        await this.FetchClientCharacters();

        // Load Vault
        await this.LoadClientVault();

        // Load Inventories
        await this.LoadClientInventories();

        // Load Item Details
        await this.QueryManifestForDetails(this.dropzoneBackingStore);

        // Load Bucket Details and Sort
        await this.QueryManifestForBuckets(this.dropzoneBackingStore);
    }

    /// <summary>
    /// Gets clients character information from bungie.
    /// </summary>
    private async Task FetchClientCharacters()
    {
        try
        {
            string charactersJson = await this.request.PlatformDestiny2_Request_Contents("Characters", true);
            JsonDocument charactersContent = JsonDocument.Parse(charactersJson);
            JsonElement characters = charactersContent.RootElement.GetProperty("Response").GetProperty("characters").GetProperty("data");

            foreach (JsonProperty characterJson in characters.EnumerateObject())
            {
                if (characterJson.Value.ValueKind != JsonValueKind.Null)
                {
                    var characterDetails = characterJson.Value;

                    // Deserialize character details
                    var character = JsonSerializer.Deserialize<Character>(characterDetails);

                    if (character != null)
                    {
                        // Assign the character ID manually
                        character.CharacterId = characterJson.Name;
                        // character.Class = await core.GetCharacterClass(character.ClassHash);
                        this.Characters.Add(character);
                    }
                }
                else
                {
                    this.logger.LogWarning($"Character details for ID {characterJson.Name} are null.");
                }
            }

            // if (Characters.Count == 0)
            // {
            //    GVM.DisplaySnackbar($"GVM Could not load you Guardians.", Severity.Error);
            // }
        }
        catch (Exception e)
        {
            this.logger.LogError($"GVM was unable to retrieve the client's characters: {e.Message}");
        }
    }

    /// <summary>
    /// Creates the clients character inventory contents from bungie.
    /// </summary>
    private async Task LoadClientInventories()
    {
        try
        {
            // Fetch Inventories contents
            string inventoriesContentsJson = await this.request.PlatformDestiny2_Request_Contents("CharacterInventories", true);
            JsonDocument inventoriesContents = JsonDocument.Parse(inventoriesContentsJson);
            JsonElement charactersArray = inventoriesContents.RootElement.GetProperty("Response").GetProperty("characterInventories").GetProperty("data");

            // Populate Inventories list
            int itemsLoaded = 0;
            foreach (Character character in this.characters)
            {
                if (character.CharacterId != null)
                {
                    JsonElement characterInventory = charactersArray.GetProperty(character.CharacterId).GetProperty("items");

                    foreach (JsonElement itemJson in characterInventory.EnumerateArray())
                    {
                        Item item = JsonSerializer.Deserialize<Item>(itemJson) ?? new Item();
                        item.Identifier = character.CharacterId;
                        this.dropzoneBackingStore.Add(item);

                        itemsLoaded++;
                    }
                }
            }

            this.logger.LogInformation($"Loaded {itemsLoaded} items into Inventories.");

            // if (itemsLoaded != 0)
            // {
            //    DisplaySnackbar($"Loaded {itemsLoaded} items into Inventories.");
            // }
            // else
            // {
            //    DisplaySnackbar($"Could not load your Inventories. Try reloading the page.", Severity.Warning);
            // }
        }
        catch (Exception e)
        {
            this.logger.LogError($"GVM was unable to retrieve the client's inventories: {e.Message}");
        }
    }

    /// <summary>
    /// Creates the clients vault contents from bungie.
    /// </summary>
    private async Task LoadClientVault()
    {
        try
        {
            // Fetch vault contents
            string vaultContentsJson = await this.request.PlatformDestiny2_Request_Contents("ProfileInventories", true);
            JsonDocument vaultContents = JsonDocument.Parse(vaultContentsJson);
            JsonElement vaultArray = vaultContents.RootElement.GetProperty("Response").GetProperty("profileInventory").GetProperty("data").GetProperty("items");

            // Populate Vault list
            int itemsLoaded = 0;
            foreach (JsonElement itemElement in vaultArray.EnumerateArray())
            {
                Item item = JsonSerializer.Deserialize<Item>(itemElement) ?? new Item();
                item.Identifier = "Vault";
                this.dropzoneBackingStore.Add(item);

                itemsLoaded++;
            }

            this.logger.LogInformation($"Loaded {itemsLoaded} items into Vault.");

            // if (itemsLoaded != 0)
            // {
            //    DisplaySnackbar($"Loaded {itemsLoaded} items into Vault.");
            // }
            // else
            // {
            //    DisplaySnackbar($"Could not load your Vault. Try reloading the page.", Severity.Warning);
            // }
        }
        catch (Exception e)
        {
            this.logger.LogError($"GVM was unable to retrieve the client's vault: {e.Message}");
        }
    }

    /// <summary>
    /// Creates a dictionary for the clients weapons.
    /// </summary>
    /// <param name="items">List of the clients weapons.</param>
    private async Task QueryManifestForDetails(List<Item> items)
    {
        this.logger.LogInformation("Getting Clients Weapon Information.");
        foreach (Item item in items)
        {
            uint itemHash = item.ItemHash;

            if (!this.itemDetails.TryGetValue(itemHash, out _))
            {
                var itemDetails = await this.GetItemDetails(itemHash);

                // if (itemDetails.Equals(new ItemDetails()))
                // {
                //    DisplaySnackbar("GVM is unable to define an item.");
                // }

                this.itemDetails.Add(itemHash, itemDetails);
            }
        }
    }

    /// <summary>
    /// Assigns each item in the given list into their buckets.
    /// </summary>
    /// <param name="items">List to assign buckets to.</param>
    private async Task QueryManifestForBuckets(List<Item> items)
    {
        this.logger.LogInformation("Getting Bucket Information.");
        foreach (Item item in items)
        {
            uint hash = item.ItemHash;
            if (this.itemDetails.ContainsKey(hash))
            {
                var itemDetails = this.itemDetails[hash];
                item.Bucket = await this.GetBucketDefinition(itemDetails.Inventory.BucketTypeHash);
                this.SortIntoSubComponent(item);
            }
            else
            {
                this.logger.LogWarning($"Bucket not found for item: {item.ItemHash}, BucketHash: {item.BucketHash}");
            }
        }

        // Combine appropriate Lists (TODO May not be needed)
        this.weapons.AddRange(this.kineticWeapons);
        this.weapons.AddRange(this.energyWeapons);
        this.weapons.AddRange(this.powerWeapons);

        this.armor.AddRange(this.helmets);
        this.armor.AddRange(this.gauntlets);
        this.armor.AddRange(this.chests);
        this.armor.AddRange(this.legs);
        this.armor.AddRange(this.skirts);

        this.clientComponents.AddRange(this.consumables);
        this.clientComponents.AddRange(this.extras);
    }

    /// <summary>
    /// Sorts everything in the list to the appropriate sub categories.
    /// </summary>
    /// <param name="item">Item to sort into components.</param>
    private void SortIntoSubComponent(Item item)
    {
        // Sort Items into buckets
        if (!string.IsNullOrEmpty(item.Bucket))
        {
            switch (item.Bucket)
            {
                case "Kinetic Weapons":
                    this.kineticWeapons.Add(item);
                    break;
                case "Energy Weapons":
                    this.energyWeapons.Add(item);
                    break;
                case "Power Weapons":
                    this.powerWeapons.Add(item);
                    break;

                case "Helmet":
                    this.helmets.Add(item);
                    break;
                case "Gauntlets":
                    this.gauntlets.Add(item);
                    break;
                case "Chest Armor":
                    this.chests.Add(item);
                    break;
                case "Leg Armor":
                    this.legs.Add(item);
                    break;
                case "Class Armor":
                    this.skirts.Add(item);
                    break;

                case "Lost Items":
                    this.postmaster.Add(item);
                    break;

                case "Ghost":
                case "Ships":
                case "Vehicle":
                case "Emblems":
                case "Accessories":
                    this.accessories.Add(item);
                    break;

                case "Engrams":
                    this.engrams.Add(item);
                    break;

                case "Strange Coin":
                case "Glimmer":
                case "Legendary Shards":
                case "Silver":
                case "Bright Dust":
                case "Wrapped Items":
                case "Materials":
                case "Synthweave Bolt":
                case "Synthweave Strap":
                case "Synthweave Plate":
                case "Synthweave Template":
                case "Consumables":
                    this.consumables.Add(item);
                    break;

                case "Messages":
                case "Special Orders":
                case "Quests":
                    this.quests.Add(item);
                    break;

                default:
                    this.extras.Add(item);
                    break;
            }
        }
    }

    /// <summary>
    /// Gets the details of the itemHash provided.
    /// </summary>
    /// <param name="itemHash">ItemHash to use in the search.</param>
    /// <returns>The URL for the photo.</returns>
    private async Task<ItemDetails> GetItemDetails(uint itemHash)
    {
        ItemDetails details;
        details = await this.manifest.Manifest_Query_ItemDetails(itemHash);

        if (details.Equals(new ItemDetails()))
        {
            details = await this.request.PlatformDestiny2_Request_ItemDefinition(itemHash);
        }

        if (details.Equals(new ItemDetails()))
        {
            this.logger.LogError($"GVM could not find any ItemDetails for the itemHash={itemHash} in both the manifest or from Bungie.");
        }

        return details;
    }

    /// <summary>
    /// Retrieves the bucket definition of the bucketHash.
    /// </summary>
    /// <param name="bucketHash">BucketHash to find.</param>
    /// <returns>The name of the associated bucket.</returns>
    private async Task<string> GetBucketDefinition(uint bucketHash)
    {
        JsonDocument bucketDefinition = await this.manifest.Manifest_Query_BucketDefinition(bucketHash);

        // Validate if the JSON is empty
        if (bucketDefinition.RootElement.ValueKind == JsonValueKind.Object &&
            bucketDefinition.RootElement.TryGetProperty("displayProperties", out JsonElement displayProperties))
        {
            // Validate if the "name" property exists in "displayProperties"
            if (displayProperties.TryGetProperty("name", out JsonElement nameElement))
            {
                return nameElement.GetString() ?? string.Empty;
            }
            else
            {
                this.logger.LogWarning($"Bucket Definition found, but 'name' property is missing for bucketHash={bucketHash}");
            }
        }
        else
        {
            this.logger.LogWarning($"Bucket Definition not found or invalid for bucketHash={bucketHash}");
        }

        return string.Empty;
    }
}
