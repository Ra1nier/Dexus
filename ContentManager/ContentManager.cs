// <copyright file="ContentManager.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using MudBlazor;

namespace ContentManager;

using ClientProfile;
using GuardianServices;
using GuardianServices.Models;
using Microsoft.Extensions.Logging;
using RequestManager;
using System.Text.Json;

/// <summary>
/// The ContentManager class is responsible for managing the content of a user's account.
/// For now, the only things handled by content manager are character and profile inventories.
/// However, this class should be used for all things bungie-client related.
/// </summary>
public class ContentManager
{
    /// <summary>
    /// Base bungie URL.
    /// </summary>
    private readonly string baseUrl = "https://www.bungie.net";

    /// <summary>
    /// Logger for Dexus.
    /// </summary>
    private readonly ILogger logger;

    /// <summary>
    /// User profile.
    /// </summary>
    private readonly Profile profile;

    /// <summary>
    /// Request Server for Dexus.
    /// </summary>
    private readonly RequestServer request;

    /// <summary>
    /// Manifest Server for Dexus.
    /// </summary>
    private readonly ManifestServer manifest;

    /// <summary>
    /// Snackbar Service for Dexus.
    /// </summary>
    private readonly SnackbarService snackbar;

    /// <summary>
    /// Loadingbar service for Dexus.
    /// </summary>
    private readonly LoadingbarService loadingbars;

    // Content Manager Variables
    private bool isInitialized;

    // Content Containers
    private Dictionary<string, Character> characters = new Dictionary<string, Character>();
    private Dictionary<string, DItem> dropzoneBackingStore = new Dictionary<string, DItem>();
    private List<DItem> uninstancatedItems = new List<DItem>();

    // Content Detail Dictionaries.
    private Dictionary<uint, ItemDetails> itemDetails = new Dictionary<uint, ItemDetails>();
    private Dictionary<uint, ItemDetails> perkDetails = new Dictionary<uint, ItemDetails>();
    private Dictionary<uint, ItemDetails> plugDetails = new Dictionary<uint, ItemDetails>();
    private Dictionary<uint, ItemDetails> socketDetails = new Dictionary<uint, ItemDetails>();

    // Render Keys
    private Guid itemManagementRenderKey = Guid.NewGuid();

    /// <summary>
    /// Initializes a new instance of the <see cref="ContentManager"/> class.
    /// </summary>
    /// <param name="logger">Dexus Logger.</param>
    /// <param name="profile">Dexus user profile.</param>
    /// <param name="request">Dexus request server.</param>
    /// <param name="manifest">Dexus manifest server.</param>
    /// <param name="snackbar">Dexus snackbar provider.</param>
    /// <param name="loadingbars">Dexus loadingbar service.</param>
    public ContentManager(ILogger logger, Profile profile, RequestServer request, ManifestServer manifest,
        SnackbarService snackbar, LoadingbarService loadingbars)
    {
        this.logger = logger;
        this.profile = profile;
        this.request = request;
        this.manifest = manifest;
        this.snackbar = snackbar;
        this.loadingbars = loadingbars;
        this.Transfer = new TransferManager(logger, profile, request, snackbar, this.dropzoneBackingStore);

        // connectors
        this.Transfer.TransferOccured += this.RefreshRenderKey;
        this.Transfer.SetIdentifier += this.SetIdentifier;
    }

    /// <summary>
    /// Gets the transfer manager used by ContentManager.
    /// </summary>
    public TransferManager Transfer { get; }

    /// <summary>
    /// Gets the Characters List.
    /// </summary>
    public Dictionary<string, Character> Characters { get => this.characters; }

    /// <summary>
    /// Gets the KineticWeapons List.
    /// </summary>
    public Dictionary<string, DItem> KineticWeapons => this.GetItemsByBucket("Kinetic Weapons");

    /// <summary>
    /// Gets the EnergyWeapons List.
    /// </summary>
    public Dictionary<string, DItem> EnergyWeapons => this.GetItemsByBucket("Energy Weapons");

    /// <summary>
    /// Gets the PowerWeapons List.
    /// </summary>
    public Dictionary<string, DItem> PowerWeapons => this.GetItemsByBucket("Power Weapons");

    /// <summary>
    /// Gets the Helmets List.
    /// </summary>
    public Dictionary<string, DItem> Helmets => this.GetItemsByBucket("Helmet");

    /// <summary>
    /// Gets the Gauntlets List.
    /// </summary>
    public Dictionary<string, DItem> Gauntlets => this.GetItemsByBucket("Gauntlets");

    /// <summary>
    /// Gets the Chests List.
    /// </summary>
    public Dictionary<string, DItem> Chests => this.GetItemsByBucket("Chest Armor");

    /// <summary>
    /// Gets the Legs List.
    /// </summary>
    public Dictionary<string, DItem> Legs => this.GetItemsByBucket("Leg Armor");

    /// <summary>
    /// Gets the Skirts List.
    /// </summary>
    public Dictionary<string, DItem> Skirts => this.GetItemsByBucket("Class Armor");

    /// <summary>
    /// Gets the Accessories List.
    /// </summary>
    public Dictionary<string, DItem> Accessories =>
        this.GetItemsByBucket("Ghost", "Ships", "Vehicle", "Emblems", "Accessories");

    /// <summary>
    /// Gets the Postmaster List.
    /// </summary>
    public Dictionary<string, DItem> Postmaster => this.GetItemsByBucket("Lost Items");

    /// <summary>
    /// Gets the Engrams List.
    /// </summary>
    public Dictionary<string, DItem> Engrams => this.GetItemsByBucket("Engrams");

    /// <summary>
    /// Gets the Quests List.
    /// </summary>
    public Dictionary<string, DItem> Quests => this.GetItemsByBucket("Messages", "Special Orders", "Quests");

    /// <summary>
    /// Gets the consumables List.
    /// </summary>
    public List<DItem> Consumables
    {
        get
        {
            string[] buckets =
            [
                "Strange Coin",
                "Glimmer",
                "Legendary Shards",
                "Silver",
                "Bright Dust",
                "Wrapped Items",
                "Materials",
                "Synthweave Bolt",
                "Synthweave Strap",
                "Synthweave Plate",
                "Synthweave Template",
                "Consumables"
            ];

            return this.uninstancatedItems.Where(i => buckets.Contains(i.Bucket)).ToList();
        }
    }

    /// <summary>
    /// Gets the DropzoneBackingStore List.
    /// </summary>
    public Dictionary<string, DItem> DropzoneBackingStore { get => this.dropzoneBackingStore; }

    /// <summary>
    /// Gets the UninstantiatedItems List.
    /// </summary>
    public List<DItem> UninstantiatedItems { get => this.uninstancatedItems; }

    /// <summary>
    /// Gets the render key that indicates whether item management lists need to be rendered again.
    /// </summary>
    public Guid ItemManagementRenderKey { get => this.itemManagementRenderKey; }

    /// <summary>
    /// Retrieves all information needed to have a fully functioning content manager.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task InitializeUserContent()
    {
        if (this.isInitialized)
        {
            this.logger.LogWarning("Content Manager has already been initialized.");
            return;
        }

        this.logger.LogDebug("Content Manager Initializing.");
        await this.FetchClientCharacters();
        await this.LoadUserContents();

        this.isInitialized = true;
        this.logger.LogDebug("Content Manager Initialization Done.");
    }

    /// <summary>
    /// Refreshes the user content information.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task RefreshUserContent()
    {
        if (!this.isInitialized)
        {
            this.logger.LogWarning(
                "Content Manager has not been initialized. It must be initialized before user contents can be refreshed.");
            return;
        }

        this.logger.LogDebug("Refreshing user content.");
        await this.LoadUserContents();
        this.logger.LogDebug("User content refreshed.");
    }

    /// <summary>
    /// Returns a Dictionary(string, DItem) containing every item that is a part of the provided bucket array.
    /// </summary>
    /// <param name="bucket">Array of strings containing the buckets requested.</param>
    /// <returns>The dictionary containing the items.</returns>
    public Dictionary<string, DItem> GetItemsByBucket(params string[] bucket)
    {
        return this.dropzoneBackingStore.Where(i => bucket.Contains(i.Value.Bucket))
            .ToDictionary(i => i.Key, i => i.Value);
    }

    /// <summary>
    /// Performs all tasks related to loading or refreshing the user's content.
    /// </summary>
    /// <returns>A Task representing the operation.</returns>
    private async Task LoadUserContents()
    {
        this.ClearDropzoneBackingStore();

        // Load client contents from bungie.
        List<Task> contentTasks =
        [
            this.LoadClientInventories(),
            this.LoadClientVault(),
            this.LoadClientEquipment()
        ];
        await Task.WhenAll(contentTasks);

        // Inform all dependents that information from the DBS has changed
        this.itemManagementRenderKey = Guid.NewGuid();
    }

    /// <summary>
    /// Clears both dropzone backing stores if they contain items.
    /// </summary>
    private void ClearDropzoneBackingStore()
    {
        if (this.dropzoneBackingStore.Count != 0)
        {
            this.dropzoneBackingStore.Clear();
        }

        if (this.uninstancatedItems.Count != 0)
        {
            this.uninstancatedItems.Clear();
        }
    }

    /// <summary>
    /// Gets clients character information from Bungie.
    /// </summary>
    /// <returns>Task representing the operation.</returns>
    private async Task FetchClientCharacters()
    {
        try
        {
            string charactersJson = await this.request.PlatformDestiny2_Request_ProfileContents("Characters", true);
            JsonDocument charactersContent = JsonDocument.Parse(charactersJson);
            JsonElement charactersArray = charactersContent.RootElement.GetProperty("Response")
                .GetProperty("characters").GetProperty("data");

            foreach (JsonProperty characterJson in charactersArray.EnumerateObject())
            {
                if (characterJson.Value.ValueKind != JsonValueKind.Null)
                {
                    var characterDetails = characterJson.Value;

                    // Deserialize character details
                    var character = characterDetails.Deserialize<Character>();

                    if (character != null)
                    {
                        // Assign the character ID manually
                        character.CharacterId = characterJson.Name;
                        switch (character.ClassType)
                        {
                            case 0:
                                character.Class = "Titan";
                                break;
                            case 1:
                                character.Class = "Hunter";
                                break;
                            case 2:
                                character.Class = "Warlock";
                                break;
                            default:
                                character.Class = "Unknown";
                                break;
                        }

                        character.EmblemPath = this.baseUrl + character.EmblemPath;
                        character.EmblemBackgroundPath = this.baseUrl + character.EmblemBackgroundPath;

                        this.characters.TryAdd(character.CharacterId ?? "-1", character);
                    }
                }
                else
                {
                    this.logger.LogWarning($"Character details for ID {characterJson.Name} are null.");
                }
            }

            if (this.Characters.Count == 0)
            {
                this.snackbar.AddMessage($"Dexus Could not load your Guardians.", "Error");
            }
        }
        catch (Exception e)
        {
            this.logger.LogError($"Dexus was unable to retrieve the client's characters: {e.Message}");
        }
    }

    /// <summary>
    /// Builds the appropriate component request header for the given destiny 2 bucket.
    /// </summary>
    /// <param name="destiny2Comp">Destiny 2 Bucket to build the header around.</param>
    /// <returns>String[] containing the requests.</returns>
    private string[] BuildRequestHeader(string destiny2Comp)
    {
        return
        [
            destiny2Comp,
            "ItemInstances",
            "ItemPerks",
            "ItemSockets",
            "ItemReusablePlugs",
        ];
    }

    /// <summary>
    /// Creates the clients character equipment from Bungie.
    /// </summary>
    /// <returns>Task representing the operation.</returns>
    private async Task LoadClientEquipment()
    {
        try
        {
            string equipmentContentsJson = await this.request.PlatformDestiny2_Request_ProfileContents(BuildRequestHeader("CharacterEquipment"), true);
            // await File.WriteAllTextAsync(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FullItemComponents.json"), equipmentContentsJson); // TODO: remove

            JsonDocument equipmentContents = JsonDocument.Parse(equipmentContentsJson);
            JsonElement equipmentArray = equipmentContents.RootElement.GetProperty("Response")
                .GetProperty("characterEquipment").GetProperty("data");
            JsonElement instanceDataArray = equipmentContents.RootElement.GetProperty("Response")
                .GetProperty("itemComponents").GetProperty("instances").GetProperty("data");
            JsonElement perksDataArray = equipmentContents.RootElement.GetProperty("Response")
                .GetProperty("itemComponents").GetProperty("perks")
                .GetProperty("data"); // TODO: Likely not needed anymore but keeping just in case
            JsonElement plugsDataArray = equipmentContents.RootElement.GetProperty("Response")
                .GetProperty("itemComponents").GetProperty("reusablePlugs").GetProperty("data");
            JsonElement socketsDataArray = equipmentContents.RootElement.GetProperty("Response")
                .GetProperty("itemComponents").GetProperty("sockets").GetProperty("data");

            // Populate Inventories list
            int itemsLoaded = 0;
            foreach (Character character in this.characters.Values)
            {
                if (character.CharacterId != null)
                {
                    JsonElement characterInventory =
                        equipmentArray.GetProperty(character.CharacterId).GetProperty("items");

                    foreach (JsonElement itemJson in characterInventory.EnumerateArray())
                    {
                        DItem item = await this.BuildDItem(itemJson, $"{character.CharacterId}Equipped", perksDataArray,
                            plugsDataArray, socketsDataArray, instanceDataArray);
                        this.AddItemToStore(item);
                        itemsLoaded++;
                    }
                }
            }

            this.logger.LogInformation($"Loaded {itemsLoaded} items from Equipment.");

            if (itemsLoaded == 0)
            {
                this.snackbar.AddMessage($"Could not load your equipment. Try reloading the page.", "Warning");
            }
        }
        catch (Exception e)
        {
            this.logger.LogError($"Dexus was unable to retrieve the client's equipment: {e.Message}");
        }
    }

    /// <summary>
    /// Creates the clients character inventory contents from Bungie.
    /// </summary>
    /// <returns>Task representing the operation.</returns>
    private async Task LoadClientInventories()
    {
        try
        {
            string inventoriesContentsJson =
                await this.request.PlatformDestiny2_Request_ProfileContents(BuildRequestHeader("CharacterInventories"), true);

            JsonDocument inventoriesContents = JsonDocument.Parse(inventoriesContentsJson);
            JsonElement inventoriesArray = inventoriesContents.RootElement.GetProperty("Response")
                .GetProperty("characterInventories").GetProperty("data");
            JsonElement instanceDataArray = inventoriesContents.RootElement.GetProperty("Response")
                .GetProperty("itemComponents").GetProperty("instances").GetProperty("data");
            JsonElement perksDataArray = inventoriesContents.RootElement.GetProperty("Response")
                .GetProperty("itemComponents").GetProperty("perks").GetProperty("data");
            JsonElement plugsDataArray = inventoriesContents.RootElement.GetProperty("Response")
                .GetProperty("itemComponents").GetProperty("reusablePlugs").GetProperty("data");
            JsonElement socketsDataArray = inventoriesContents.RootElement.GetProperty("Response")
                .GetProperty("itemComponents").GetProperty("sockets").GetProperty("data");

            // Populate Inventories list
            int itemsLoaded = 0;
            foreach (Character character in this.characters.Values)
            {
                if (character.CharacterId != null)
                {
                    JsonElement characterInventory =
                        inventoriesArray.GetProperty(character.CharacterId).GetProperty("items");

                    foreach (JsonElement itemJson in characterInventory.EnumerateArray())
                    {
                        DItem item = await this.BuildDItem(itemJson, character.CharacterId, perksDataArray,
                            plugsDataArray, socketsDataArray, instanceDataArray);
                        this.AddItemToStore(item);
                        itemsLoaded++;
                    }
                }
            }

            this.logger.LogInformation($"Loaded {itemsLoaded} items into Inventories.");

            if (itemsLoaded != 0)
            {
                this.snackbar.AddMessage($"Loaded {itemsLoaded} items into Inventories.", "success");
            }
            else
            {
                this.snackbar.AddMessage($"Could not load your Inventories. Try reloading the page.", "Warning");
            }
        }
        catch (Exception e)
        {
            this.logger.LogError($"Dexus was unable to retrieve the client's inventories: {e.Message}");
        }
    }

    /// <summary>
    /// Creates the clients vault contents from bungie.
    /// </summary>
    /// <returns>Task representing the operation.</returns>
    private async Task LoadClientVault()
    {
        try
        {
            string vaultContentsJson = await this.request.PlatformDestiny2_Request_ProfileContents(BuildRequestHeader("ProfileInventories"), true);

            JsonDocument vaultContents = JsonDocument.Parse(vaultContentsJson);
            JsonElement vaultArray = vaultContents.RootElement.GetProperty("Response").GetProperty("profileInventory")
                .GetProperty("data").GetProperty("items");
            JsonElement instanceDataArray = vaultContents.RootElement.GetProperty("Response")
                .GetProperty("itemComponents").GetProperty("instances").GetProperty("data");
            JsonElement perksDataArray = vaultContents.RootElement.GetProperty("Response").GetProperty("itemComponents")
                .GetProperty("perks").GetProperty("data");
            JsonElement plugsDataArray = vaultContents.RootElement.GetProperty("Response")
                .GetProperty("itemComponents").GetProperty("reusablePlugs").GetProperty("data");
            JsonElement socketsDataArray = vaultContents.RootElement.GetProperty("Response")
                .GetProperty("itemComponents").GetProperty("sockets").GetProperty("data");

            // Populate Vault list
            int itemsLoaded = 0;
            foreach (JsonElement itemElement in vaultArray.EnumerateArray())
            {
                DItem item = await this.BuildDItem(itemElement, "Vault", perksDataArray, plugsDataArray,
                    socketsDataArray, instanceDataArray);
                this.AddItemToStore(item);
                itemsLoaded++;
            }

            this.logger.LogInformation($"Loaded {itemsLoaded} items into Vault.");

            if (itemsLoaded != 0)
            {
                this.snackbar.AddMessage($"Loaded {itemsLoaded} items into Vault.", "success");
            }
            else
            {
                this.snackbar.AddMessage($"Could not load your Vault. Try reloading the page.", "Warning");
            }
        }
        catch (Exception e)
        {
            this.logger.LogError($"Dexus was unable to retrieve the client's vault: {e.Message}");
        }
    }

    /// <summary>
    /// Adds the item to the correct store.
    /// </summary>
    /// <param name="item">Item to add.</param>
    private void AddItemToStore(DItem item)
    {
        if (!string.IsNullOrEmpty(item.ItemInstanceId))
        {
            lock (this.dropzoneBackingStore)
            {
                this.dropzoneBackingStore.TryAdd(item.ItemInstanceId, item);
            }
        }
        else
        {
            lock (this.uninstancatedItems)
            {
                if (!this.uninstancatedItems.Contains(item))
                {
                    this.uninstancatedItems.Add(item);
                }
            }
        }
    }

    /// <summary>
    /// Builds an Item based on the provided data.
    /// </summary>
    /// <param name="itemJson">The base item data from bungie.</param>
    /// <param name="itemIdentifier">Where the item is located.</param>
    /// <param name="perksDataArray">Data for all the item's perks.</param> // TODO: Needs to be removed and switched to a need-to-know basis.
    /// <param name="plugsDataArray">Data for all the item's plugs.</param>
    /// <param name="socketDataArray">Data for the item's sockets.</param>
    /// <param name="instanceDataArray">Data for all the items instance data.</param>
    /// <returns>The Item object that was built.</returns>
    private async Task<DItem> BuildDItem(JsonElement itemJson, string itemIdentifier, JsonElement perksDataArray,
        JsonElement plugsDataArray, JsonElement socketDataArray, JsonElement instanceDataArray)
    {
        Item bungieItem = itemJson.Deserialize<Item>() ?? new Item();
        DItem? item = null;
        if (!bungieItem.Equals(new Item()))
        {
            // This operation needs to occur first as many operations below depend on this being done.
            ItemDetails details = await this.QueryManifestForGeneralItemDetails(bungieItem);
            string bucket = await this.QueryManifestForBucket(bungieItem, itemIdentifier);
            item = DItem.FromBungieItem(bungieItem, details, bucket, itemIdentifier);

            item.Power = this.GetItemPrimaryStat(instanceDataArray, bungieItem.ItemInstanceId ?? string.Empty);
            await this.GetItemPerks(item,
                perksDataArray); // TODO: This only exists for weapons pre Forsaken and is now deprecated. Most weapons will not have this functionality, therefore find a way to request this only when dealing with an older Item.
            await this.GetItemPlugs(item, plugsDataArray);
            await this.GetItemSockets(item, socketDataArray);
        }

        return item;
    }

    /// <summary>
    /// Gets the perks for the item associated with the item instance ID. TODO: Look into using deserialization to tone this function down.
    /// </summary>
    /// <param name="item">Item to assign the perks to.</param>
    /// <param name="perkData">Perk data received from bungie.</param>
    /// <returns>A Task representing the operation.</returns>
    private async Task GetItemPerks(DItem item, JsonElement perkData)
    {
        if (string.IsNullOrEmpty(item.ItemInstanceId))
        {
            return;
        }

        if (perkData.TryGetProperty(item.ItemInstanceId, out JsonElement itemPerks))
        {
            if (itemPerks.TryGetProperty("perks", out JsonElement perksArray))
            {
                List<Perk> perks = new List<Perk>();
                foreach (var perk in perksArray.EnumerateArray())
                {
                    uint perkHash = perk.GetProperty("perkHash").GetUInt32();
                    bool detailsExist;
                    ItemDetails? perkDetails;
                    lock (this.perkDetails)
                    {
                        detailsExist = this.perkDetails.TryGetValue(perkHash, out perkDetails);
                    }

                    if (!detailsExist)
                    {
                        perkDetails = await this.GetItemDetailsFromManifest(perkHash);
                        if (perkDetails.Equals(new ItemDetails()))
                        {
                            this.snackbar.AddMessage("Dexus is unable to define a perk.", "warning");
                        }
                        else
                        {
                            lock (this.perkDetails)
                            {
                                this.perkDetails.Add(perkHash, perkDetails);
                            }
                        }
                    }

                    if (perkDetails != null && !perkDetails.Equals(new ItemDetails()))
                    {
                        perks.Add(new Perk(
                            perkHash,
                            perk.GetProperty("iconPath").GetString() ?? string.Empty,
                            perk.GetProperty("isActive").GetBoolean(),
                            perk.GetProperty("visible").GetBoolean(),
                            perkDetails));
                    }
                }

                item.Perks = perks;
            }
        }
    }

    /// <summary>
    /// Gets the Items Plugs for the item from the provided data.
    /// </summary>
    /// <param name="item">Item to assign the plugs to.</param>
    /// <param name="plugData">JsonElement containing all the plug information.</param>
    private async Task GetItemPlugs(DItem item, JsonElement plugData)
    {
        if (string.IsNullOrEmpty(item.ItemInstanceId))
        {
            return;
        }

        if (plugData.TryGetProperty(item.ItemInstanceId, out JsonElement itemPlugs))
        {
            var plugs = itemPlugs.Deserialize<DItemPlugs>() ?? new DItemPlugs();

            foreach (var plug in plugs.ReusablePlugs)
            {
                foreach (var reusablePlug in plug.Value)
                {
                    ItemDetails? plugDetails;
                    bool detailsExist;

                    lock (this.plugDetails)
                    {
                        detailsExist = this.plugDetails.TryGetValue(reusablePlug.PlugItemHash, out plugDetails);
                    }

                    if (!detailsExist)
                    {
                        plugDetails = await this.GetItemDetailsFromManifest(reusablePlug.PlugItemHash);

                        if (plugDetails != null && !plugDetails.Equals(new ItemDetails()))
                        {
                            lock (this.plugDetails)
                            {
                                this.plugDetails.TryAdd(reusablePlug.PlugItemHash, plugDetails);
                            }
                        }
                        else
                        {
                            this.snackbar.AddMessage("Dexus could not define a plug.", "warning");
                            plugDetails = new ItemDetails();
                        }
                    }

                    reusablePlug.Details = plugDetails!;
                }
            }

            item.Plugs = plugs;
        }
    }

    /// <summary>
    /// Gets the Items Sockets for the item from the provided data.
    /// </summary>
    /// <param name="item">Item to assign the sockets to.</param>
    /// <param name="socketData">Data received from Bungie containing information about the user's sockets.</param>
    private async Task GetItemSockets(DItem item, JsonElement socketData)
    {
        if (string.IsNullOrEmpty(item.ItemInstanceId))
        {
            return;
        }

        Dictionary<uint, DItemSockets.Socket> builtSockets = new Dictionary<uint, DItemSockets.Socket>();

        if (socketData.TryGetProperty(item.ItemInstanceId, out JsonElement itemSockets) && itemSockets.TryGetProperty(item.ItemInstanceId, out JsonElement socketList))
        {
            var sockets = socketList.Deserialize<List<BungieSocket>>() ?? new List<BungieSocket>();

            foreach (var socket in sockets)
            {
                ItemDetails? socketDetails;
                bool detailsExist;

                lock (this.socketDetails)
                {
                    detailsExist = this.socketDetails.TryGetValue(socket.PlugHash, out socketDetails);
                }

                if (!detailsExist)
                {
                    socketDetails = await this.GetItemDetailsFromManifest(socket.PlugHash);

                    if (socketDetails != null && !socketDetails.Equals(new ItemDetails()))
                    {
                        lock (this.socketDetails)
                        {
                            this.socketDetails.TryAdd(socket.PlugHash, socketDetails);
                        }
                    }
                    else
                    {
                        this.snackbar.AddMessage("Dexus could not define a plug.", "warning");
                        socketDetails = new ItemDetails();
                    }
                }

                builtSockets.Add(socket.PlugHash, new DItemSockets.Socket(socket.IsEnabled, socket.IsVisible, socketDetails!));
            }

            item.Sockets.Data = builtSockets;
        }
    }

    /// <summary>
    /// Creates an ItemDetail for the client's weapon and adds it to the Item Details list.
    /// </summary>
    /// <param name="item">Item to get information for.</param>
    /// <returns>Task representing the operation.</returns>
    private async Task<ItemDetails> QueryManifestForGeneralItemDetails(Item item)
    {
        uint itemHash = item.ItemHash;

        ItemDetails? details;
        if (this.itemDetails.TryGetValue(itemHash, out var itemDetails))
        {
            return itemDetails;
        }

        details = await this.GetItemDetailsFromManifest(itemHash);
        if (details.Equals(new ItemDetails()))
        {
            this.snackbar.AddMessage("Dexus is unable to define an item.", "warning");
            return details;
        }

        lock (this.itemDetails)
        {
            this.itemDetails.Add(itemHash, details);
        }

        return details;
    }

    /// <summary>
    /// Assigns each item in the given list into their buckets.
    /// </summary>
    /// <param name="item">Item to get information for.</param>
    /// <param name="identifier">Where the item is currently located.</param>
    /// <returns>Task representing the operation.</returns>
    private async Task<string> QueryManifestForBucket(Item item, string identifier)
    {
        uint hash = item.ItemHash;

        if (identifier == "Vault")
        {
            return await this.GetBucketDefinition(this.itemDetails[hash].Inventory.BucketTypeHash);
        }
        else
        {
            return await this.GetBucketDefinition(item.BucketHash);
        }
    }

    /// <summary>
    /// Gets the details of the itemHash provided.
    /// </summary>
    /// <param name="itemHash">ItemHash to use in the search.</param>
    /// <returns>An ItemDetails containing the info from the manifest.</returns>
    private async Task<ItemDetails> GetItemDetailsFromManifest(uint itemHash)
    {
        ItemDetails details = await this.manifest.Manifest_Query_ItemDetails(itemHash);

        if (details.Equals(new ItemDetails()))
        {
            details = await this.request.PlatformDestiny2_Request_ItemDefinition(itemHash);
            this.logger.LogError(
                $"Dexus could not find any ItemDetails for the itemHash={itemHash} in the manifest or and has attempted to get it from Bungie.");
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

            this.logger.LogWarning(
                $"Bucket Definition found, but 'name' property is missing for bucketHash={bucketHash}");
        }
        else
        {
            this.logger.LogWarning($"Bucket Definition not found or invalid for bucketHash={bucketHash}");
        }

        return string.Empty;
    }

    /// <summary>
    /// Gets and item's primary stat.
    /// </summary>
    /// <returns>The value of the items primary stat.</returns>
    private int GetItemPrimaryStat(JsonElement instanceDataArray, string instanceID)
    {
        if (string.IsNullOrEmpty(instanceID))
        {
            return 0;
        }

        // Get needed data
        if (!instanceDataArray.TryGetProperty(instanceID, out JsonElement itemInstanceData) ||
            !itemInstanceData.TryGetProperty("primaryStat", out JsonElement primaryStat))
        {
            return 0;
        }

        // Retrieve items power level and assign it to the item
        return primaryStat.TryGetProperty("value", out JsonElement powerLevel) ? powerLevel.GetInt32() : 0;
    }

    /// <summary>
    /// Sets the identifier of the item.
    /// </summary>
    private void SetIdentifier((DItem, string) information)
    {
        this.dropzoneBackingStore[information.Item1.ItemInstanceId ?? string.Empty].Identifier = information.Item2;
    }

    /// <summary>
    /// Assigns a new value to the item management render key to indicate that the state has changed.
    /// </summary>
    private void RefreshRenderKey()
    {
        this.itemManagementRenderKey = Guid.NewGuid();
    }
}