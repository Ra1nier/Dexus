// <copyright file="ContentManager.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace ContentManager;

using ClientProfile;
using GuardianServices;
using Microsoft.Extensions.Logging;
using Models;
using RequestManager;
using RequestManager.Models;
using System.Text.Json;

/// <summary>
/// The ContentManager class is responsible for managing the content of GVM.
/// </summary>
public class ContentManager
{
    /// <summary>
    /// Base bungie URL.
    /// </summary>
    private readonly string baseUrl = "https://www.bungie.net";

    /// <summary>
    /// Logger for GVM.
    /// </summary>
    private ILogger logger;

    /// <summary>
    /// User profile.
    /// </summary>
    private Profile profile;

    /// <summary>
    /// Request Server for GVM.
    /// </summary>
    private RequestServer request;

    /// <summary>
    /// Manifest Server for GVM.
    /// </summary>
    private ManifestServer manifest;

    /// <summary>
    /// Snackbar Service for GVM.
    /// </summary>
    private SnackbarService snackbar;

    // Content Manager Variables
    private bool isInitialized = false;

    // Content Containers
    private Dictionary<string, Character> characters = new Dictionary<string, Character>();
    private Dictionary<string, Item> dropzoneBackingStore = new Dictionary<string, Item>();
    private List<Item> uninstancatedItems = new List<Item>();
    private Dictionary<uint, ItemDetails> itemDetails = new Dictionary<uint, ItemDetails>();

    // Content Management Containers
    private Queue<TransferInfo> queuedTransfers = new Queue<TransferInfo>();
    private TimedDictionary<string, bool> recentlyHandledTransfers = new TimedDictionary<string, bool>(TimeSpan.FromSeconds(10));

    // Content Tasks
    private Task? transferTask;
    private CancellationTokenSource? cancellationTokenSource;

    // Render Keys
    private Guid itemManagementRenderKey = Guid.NewGuid();

    /// <summary>
    /// Initializes a new instance of the <see cref="ContentManager"/> class.
    /// This instance will be NON-FUNCTIONAL. This should only be used for nullable purposes.
    /// </summary>
    public ContentManager()
    {
        this.logger = null!;
        this.profile = null!;
        this.request = null!;
        this.manifest = null!;
        this.snackbar = null!;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ContentManager"/> class.
    /// </summary>
    /// <param name="logger">GVM Logger.</param>
    /// <param name="profile">GVM user profile.</param>
    /// <param name="request">GVM request server.</param>
    /// <param name="manifest">GVM manifest server.</param>
    /// <param name="snackbar">GVM snackbar provider.</param>
    public ContentManager(ILogger logger, Profile profile, RequestServer request, ManifestServer manifest, SnackbarService snackbar)
    {
        this.logger = logger;
        this.profile = profile;
        this.request = request;
        this.manifest = manifest;
        this.snackbar = snackbar;
    }

    /// <summary>
    /// Gets the Characters List.
    /// </summary>
    public Dictionary<string, Character> Characters { get => this.characters; }

    /// <summary>
    /// Gets the KineticWeapons List.
    /// </summary>
    public Dictionary<string, Item> KineticWeapons
    {
        get { return this.dropzoneBackingStore.Where(i => i.Value.Bucket == "Kinetic Weapons").ToDictionary(i => i.Key, i => i.Value); }
    }

    /// <summary>
    /// Gets the EnergyWeapons List.
    /// </summary>
    public Dictionary<string, Item> EnergyWeapons
    {
        get { return this.dropzoneBackingStore.Where(i => i.Value.Bucket == "Energy Weapons").ToDictionary(i => i.Key, i => i.Value); }
    }

    /// <summary>
    /// Gets the PowerWeapons List.
    /// </summary>
    public Dictionary<string, Item> PowerWeapons
    {
        get { return this.dropzoneBackingStore.Where(i => i.Value.Bucket == "Power Weapons").ToDictionary(i => i.Key, i => i.Value); }
    }

    /// <summary>
    /// Gets the Helmets List.
    /// </summary>
    public Dictionary<string, Item> Helmets
    {
        get { return this.dropzoneBackingStore.Where(i => i.Value.Bucket == "Helmet").ToDictionary(i => i.Key, i => i.Value); }
    }

    /// <summary>
    /// Gets the Gauntlets List.
    /// </summary>
    public Dictionary<string, Item> Gauntlets
    {
        get { return this.dropzoneBackingStore.Where(i => i.Value.Bucket == "Gauntlets").ToDictionary(i => i.Key, i => i.Value); }
    }

    /// <summary>
    /// Gets the Chests List.
    /// </summary>
    public Dictionary<string, Item> Chests
    {
        get { return this.dropzoneBackingStore.Where(i => i.Value.Bucket == "Chest Armor").ToDictionary(i => i.Key, i => i.Value); }
    }

    /// <summary>
    /// Gets the Legs List.
    /// </summary>
    public Dictionary<string, Item> Legs
    {
        get { return this.dropzoneBackingStore.Where(i => i.Value.Bucket == "Leg Armor").ToDictionary(i => i.Key, i => i.Value); }
    }

    /// <summary>
    /// Gets the Skirts List.
    /// </summary>
    public Dictionary<string, Item> Skirts
    {
        get { return this.dropzoneBackingStore.Where(i => i.Value.Bucket == "Class Armor").ToDictionary(i => i.Key, i => i.Value); }
    }

    /// <summary>
    /// Gets the Accessories List.
    /// </summary>
    public Dictionary<string, Item> Accessories
    {
        get
        {
            string[] buckets =
            {
                "Ghost",
                "Ships",
                "Vehicle",
                "Emblems",
                "Accessories",
            };

            return this.dropzoneBackingStore.Where(i => buckets.Contains(i.Value.Bucket)).ToDictionary(i => i.Key, i => i.Value);
        }
    }

    /// <summary>
    /// Gets the Postmaster List.
    /// </summary>
    public Dictionary<string, Item> Postmaster
    {
        get { return this.dropzoneBackingStore.Where(i => i.Value.Bucket == "Lost Items").ToDictionary(i => i.Key, i => i.Value); }
    }

    /// <summary>
    /// Gets the Engrams List.
    /// </summary>
    public Dictionary<string, Item> Engrams
    {
        get { return this.dropzoneBackingStore.Where(i => i.Value.Bucket == "Engrams").ToDictionary(i => i.Key, i => i.Value); }
    }

    /// <summary>
    /// Gets the Quests List.
    /// </summary>
    public Dictionary<string, Item> Quests
    {
        get
        {
            string[] buckets =
            [
                "Messages",
                "Special Orders",
                "Quests"
            ];

            return this.dropzoneBackingStore.Where(i => buckets.Contains(i.Value.Bucket)).ToDictionary(i => i.Key, i => i.Value);
        }
    }

    /// <summary>
    /// Gets the consumables List.
    /// </summary>
    public List<Item> Consumables
    {
        get
        {
            string[] buckets =
            {
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
                "Consumables",
            };

            return this.uninstancatedItems.Where(i => buckets.Contains(i.Bucket)).ToList();
        }
    }

    /// <summary>
    /// Gets the DropzoneBackingStore List.
    /// </summary>
    public Dictionary<string, Item> DropzoneBackingStore { get => this.dropzoneBackingStore; }

    /// <summary>
    /// Gets the UninstantiatedItems List.
    /// </summary>
    public List<Item> UninstantiatedItems { get => this.uninstancatedItems; }

    /// <summary>
    /// Gets the Details List.
    /// </summary>
    public Dictionary<uint, ItemDetails> Details { get => this.itemDetails; }

    /// <summary>
    /// Gets a value indicating whether if transfers are currently being handled.
    /// </summary>
    public bool TransfersAllowed { get; private set; }

    /// <summary>
    /// Provides a keu for when item management lists need to be rendered again.
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
            this.logger.LogWarning("Content Manager has not been initialized. It must be initialized before user contents can be refreshed.");
            return;
        }

        this.logger.LogDebug("Refreshing user content.");
        await this.LoadUserContents();
        this.logger.LogDebug("User content refreshed.");
    }

    /// <summary>
    /// Sets the identifier of the item.
    /// </summary>
    /// <param name="item">Item to set identifier of.</param>
    /// <param name="identifier">Identifier to set.</param>
    public void SetIdentifier(Item item, string identifier)
    {
        this.dropzoneBackingStore[item.ItemInstanceId ?? string.Empty].Identifier = identifier;
    }

    /// <summary>
    /// Adds the request to the list of transfers.
    /// </summary>
    /// <param name="item">Item to transfer.</param>
    /// <param name="origin">Where the item is.</param>
    /// <param name="destination">Where the item should go.</param>
    /// <returns>The Item instance ID of the item. Can be used to track the progress of the transfer.</returns>
    public string QueueTransfer(Item item, string origin, string destination)
    {
        this.queuedTransfers.Enqueue(new TransferInfo(item, origin, destination));
        return item.ItemInstanceId ?? string.Empty;
    }

    /// <summary>
    /// Gets the transfer status of the item.
    /// </summary>
    /// <param name="itemInstanceId">ID of the item.</param>
    /// <returns>Returns 0 if the transfer was successful, 1 if the transfer was unsuccessful, 2 if the key was not found.</returns>
    public int GetTransferStatus(string itemInstanceId)
    {
        if (this.recentlyHandledTransfers.TryGetValue(itemInstanceId, out bool status))
        {
            return status ? 0 : 1;
        }

        return 2;
    }

    /// <summary>
    /// Allows content manager to start processing transfers.
    /// </summary>
    public void AllowTransfers()
    {
        this.TransfersAllowed = true;

        // Create a cancellation token source to signal task cancellation
        this.cancellationTokenSource = new CancellationTokenSource();
        CancellationToken token = this.cancellationTokenSource.Token;

        this.logger.LogInformation("Transfer processing task was started. Content Manager will now handle transfer requests.");
        this.recentlyHandledTransfers.StartAutoCleanup(TimeSpan.FromSeconds(5));
        this.transferTask = Task.Run(
            async () =>
        {
            try
            {
                while (true)
                {
                    token.ThrowIfCancellationRequested();

                    while (this.queuedTransfers.Count > 0)
                    {
                        TransferInfo transfer = this.queuedTransfers.Dequeue();
                        bool transferSuccess = await this.Transfer(transfer.Item, transfer.Origin, transfer.Destination);

                        this.recentlyHandledTransfers.Add(transfer.Item.ItemInstanceId ?? string.Empty, transferSuccess);
                    }

                    await Task.Delay(100, token);
                }
            }
            catch (OperationCanceledException)
            {
                this.logger.LogInformation("Transfer processing task was canceled. Content Manager will not handle anymore transfer requests.");
            }
            catch (Exception e)
            {
                this.logger.LogError(e, "Error occurred while processing transfers.");
            }
        }, token);
    }

    /// <summary>
    /// Stops content manager from processing transfers.
    /// </summary>
    public void StopTransfers()
    {
        this.TransfersAllowed = false;

        this.cancellationTokenSource?.Cancel();
        this.transferTask?.Wait();

        this.cancellationTokenSource?.Dispose();
        this.transferTask?.Dispose();

        this.recentlyHandledTransfers.StopAutoCleanup();
        this.recentlyHandledTransfers.Clear();
    }

    /// <summary>
    /// Pulls the requested item from the postmaster.
    /// </summary>
    /// <param name="item">Item to fetch.</param>
    /// <returns>True if the operation was completed successfully. False Otherwise.</returns>
    public async Task<bool> PullFromPostmaster(Item item)
    {
        if (this.profile.MembershipType == null)
        {
            this.logger.LogCritical("MembershipType is null. Unable to transfer item.");
            this.snackbar.AddMessage("Something went wrong. GVM is unable to access your bungie data. Transfer cannot be completed. Try logging back in to solve this issue.", "Error");
            return false;
        }

        this.itemManagementRenderKey = Guid.NewGuid();
        return await this.request.ActionItem_Request_PullFromPostmaster(this.profile.MembershipType, item.ItemHash, item.ItemInstanceId ?? string.Empty, item.Quantity, item.Identifier ?? string.Empty);
    }

    /// <summary>
    /// Performs all tasks related to loading the users content.
    /// </summary>
    /// <returns>A Task representing the operation.</returns>
    private async Task LoadUserContents()
    {
        // Erase old information if needed.
        if (this.dropzoneBackingStore.Any())
        {
            this.dropzoneBackingStore.Clear();
        }

        // Load client contents from bungie.
        List<Task> contentTasks =
        [
            this.LoadClientInventories(),
            this.LoadClientVault(),
            this.LoadClientEquipment()
        ];
        await Task.WhenAll(contentTasks);

        await this.FetchItemInformation(this.dropzoneBackingStore.Values.ToList());
        this.itemManagementRenderKey = Guid.NewGuid();
    }

    /// <summary>
    /// Equips the item to the provided character.
    /// </summary>
    /// <param name="item">Item to equip.</param>
    /// <param name="location">Character id of the items location.</param>
    /// <returns>True if the transfer operation was successful. False otherwise.</returns>
    private async Task<bool> Equip(Item item, string location)
    {
        if (this.profile.MembershipType == null)
        {
            this.logger.LogCritical("MembershipType is null. Unable to transfer item.");
            this.snackbar.AddMessage("Something went wrong. GVM is unable to access your bungie data. Equip cannot be completed. Try logging back in to solve this issue.", "Error");
            return false;
        }

        // Equip the item.
        Item itemBeingReplaced = this.DropzoneBackingStore.DistinctBy(i => i.Value.Identifier == $"{location}Equipped").First().Value ?? new Item();
        if (await this.request.ActionItem_Request_EquipItem(this.profile.MembershipType, item.ItemInstanceId ?? string.Empty, location))
        {
            this.SetIdentifier(itemBeingReplaced, location);
            this.SetIdentifier(item, $"{location}Equipped");
            return true;
        }

        return false;
    }

    /// <summary>
    /// Transfers the provided item to the provided destination.
    /// </summary>
    /// <param name="item">Item to transfer.</param>
    /// <param name="origin">Origin location (CharacterID or Vault) of the item.</param>
    /// <param name="destination">Destination location (CharacterID or Vault) of the item.</param>
    /// <returns>True if the transfer operation was successful. False otherwise.</returns>
    private async Task<bool> Transfer(Item item, string origin, string destination)
    {
        bool transferSuccess = false;
        if (origin.Contains("Equipped") || destination.Contains("Equipped"))
        {
            transferSuccess = await this.TransferEquipped(item, origin, destination);
        }
        else
        {
            transferSuccess = await this.TransferItem(item, origin, destination);
        }

        if (transferSuccess)
        {
            // Update the item's location
            this.SetIdentifier(item, destination);
        }

        this.itemManagementRenderKey = Guid.NewGuid();
        return transferSuccess;
    }

    /// <summary>
    /// Handles the transfer of an item between buckets.
    /// </summary>
    /// <param name="transfer">Item to transfer.</param>
    /// <param name="origin">Origin of the transfer.</param>
    /// <param name="destination">Destination of the transfer.</param>
    /// <returns>True if the transfer operation was successful. False otherwise.</returns>
    private async Task<bool> TransferItem(Item transfer, string origin, string destination)
    {
        if (this.profile.MembershipType == null)
        {
            this.logger.LogCritical("MembershipType is null. Unable to transfer item.");
            this.snackbar.AddMessage("Something went wrong. GVM is unable to access your bungie data. Transfer cannot be completed. Try logging back in to solve this issue.", "Error");
            return false;
        }

        this.GenerateTransferData(transfer, out uint itemHash, out string itemID, out int stackSize);

        if (destination == "Vault") // Character -> Vault
        {
            // Transfer to Vault
            return await this.request.ActionItem_Request_TransferItem(this.profile.MembershipType, itemHash, itemID, stackSize, origin, true);
        }
        else if (origin == "Vault") // Vault -> Character
        {
            // Transfer to Character
            return await this.request.ActionItem_Request_TransferItem(this.profile.MembershipType, itemHash, itemID, stackSize, destination, false);
        }
        else // Character -> Character
        {
            // Transfer to Vault then to the Character
            bool firstTransfer = await this.request.ActionItem_Request_TransferItem(this.profile.MembershipType, itemHash, itemID, stackSize, origin, true);

            bool secondTransfer = false;
            if (firstTransfer)
            {
                secondTransfer = await this.request.ActionItem_Request_TransferItem(this.profile.MembershipType, itemHash, itemID, stackSize, destination, false);
            }

            if (firstTransfer && secondTransfer)
            {
                return true;
            }
            else if (!secondTransfer) // If the second transfer failed return the item to the origin.
            {
                this.snackbar.AddMessage($"Transfer of {this.Details[transfer.ItemHash].DisplayProperties.Name} failed. Returning the item to the origin.", "Warning");
                bool returnSuccessful = await this.request.ActionItem_Request_TransferItem(this.profile.MembershipType, itemHash, itemID, stackSize, origin, false);

                if (!returnSuccessful)
                {
                    this.SetIdentifier(transfer, "Vault");
                    this.snackbar.AddMessage(
                        $"Transfer failed. {this.Details[transfer.ItemHash].DisplayProperties.Name} " +
                        "has ended up in the Vault as GVM was unable to return it.", "Warning");
                }

                return returnSuccessful;
            }

            return false;
        }
    }

    /// <summary>
    /// Handles the transfer of an equipped item between buckets. The item will be replaced by a random item from the character's inventory.
    /// </summary>
    /// <param name="transfer">Item to transfer.</param>
    /// <param name="origin">Origin of the transfer.</param>
    /// <param name="destination">Destination of the transfer.</param>
    /// <returns>True if the transfer operation was successful. False otherwise.</returns>
    private async Task<bool> TransferEquipped(Item transfer, string origin, string destination)
    {
        string trueDestination = destination.Replace("Equipped", string.Empty);
        bool transferSuccess = false;
        if (origin.Contains("Equipped") && destination.Contains("Equipped")) // Transfer of an equipped item to another characters equipment slot.
        {
            transferSuccess = await this.HandelTransferOfEquippedItem(transfer, origin, destination);
        }
        else if (origin.Contains("Equipped")) // Transfer of an equipped item to a different character.
        {
            return await this.HandelTransferOfEquippedItem(transfer, origin, destination);
        }
        else // Transfer of an item into a characters equipment slot.
        {
            transferSuccess = await this.TransferItem(transfer, origin, trueDestination);
        }

        if (transferSuccess)
        {
            return await this.Equip(transfer, trueDestination);
        }

        return false;
    }

    /// <summary>
    /// Handles the transfer of an item from ann equipped slot bucket to another characters inventory bucket.
    /// </summary>
    /// <param name="item">Item to transfer.</param>
    /// <param name="origin">Location of the item.</param>
    /// <param name="destination">Where to send the item.</param>
    /// <returns>True if the transfer operation was successful. False otherwise.</returns>
    private async Task<bool> HandelTransferOfEquippedItem(Item item, string origin, string destination)
    {
        string trueOrigin = origin.Replace("Equipped", string.Empty);
        string trueDestination = destination.Replace("Equipped", string.Empty);

        Dictionary<string, Item> charactersItemBucket = this.DropzoneBackingStore.Where(b => b.Value.Bucket == item.Bucket).Where(i => i.Value.Identifier == trueOrigin).ToDictionary(i => i.Key, i => i.Value);
        if (charactersItemBucket.Any())
        {
            this.snackbar.AddMessage("Character has no items to to replace the equipped item. Transfer another weapon into your inventory to fix this.", "Warning");
            return false; // TODO: Not the best solution. Could pull item from vault to equip, but I am too lazy.
        }

        Item itemToEquip = charactersItemBucket.Values.First();

        bool equipCompleted = await this.Equip(itemToEquip, trueOrigin);
        if (equipCompleted)
        {
            return await this.TransferItem(item, trueOrigin, trueDestination);
        }
        else
        {
            this.snackbar.AddMessage("GVM was unable to equip a different item to replace the item you wanted to transfer.", "Warning");
        }

        return false;
    }

    /// <summary>
    /// Generates the data required for a transfer.
    /// </summary>
    /// <param name="transfer">Item to extract data for.</param>
    /// <param name="itemHash">The item hash of the item.</param>
    /// <param name="itemID">The item ID of the item.</param>
    /// <param name="stackSize">The item stack size of the item.</param>
    private void GenerateTransferData(Item transfer, out uint itemHash, out string itemID, out int stackSize)
    {
        itemHash = transfer.ItemHash;
        itemID = transfer.ItemInstanceId ?? "-1";
        stackSize = transfer.Quantity;
    }

    /// <summary>
    /// Gets clients character information from bungie.
    /// </summary>
    /// <returns>Task representing the operation.</returns>
    private async Task FetchClientCharacters()
    {
        try
        {
            string charactersJson = await this.request.PlatformDestiny2_Request_ProfileContents("Characters", true);
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

                        // character.Class = await this.GetCharacterClass(character.ClassHash); This is causing issues so using switch statement for now. Switch is also probably more efficient and what are the chances they change the numbers :).
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
                this.snackbar.AddMessage($"GVM Could not load your Guardians.", "Error");
            }
        }
        catch (Exception e)
        {
            this.logger.LogError($"GVM was unable to retrieve the client's characters: {e.Message}");
        }
    }

    /// <summary>
    /// Creates the clients character equipment from Bungie.
    /// </summary>
    /// <returns>Task representing the operation.</returns>
    private async Task LoadClientEquipment()
    {
        try
        {
            // Fetch Inventories contents and associated data.
            string[] requests =
            [
                "CharacterEquipment",
                "ItemInstances",
                "ItemPerks",
            ];
            string equipmentContentsJson = await this.request.PlatformDestiny2_Request_ProfileContents(requests, true);

            JsonDocument equipmentContents = JsonDocument.Parse(equipmentContentsJson);
            JsonElement equipmentArray = equipmentContents.RootElement.GetProperty("Response").GetProperty("characterEquipment").GetProperty("data");
            JsonElement instanceDataArray = equipmentContents.RootElement.GetProperty("Response").GetProperty("itemComponents").GetProperty("instances").GetProperty("data");
            JsonElement perksDataArray = equipmentContents.RootElement.GetProperty("Response").GetProperty("itemComponents").GetProperty("perks").GetProperty("data");

            // Populate Inventories list
            int itemsLoaded = 0;
            foreach (Character character in this.characters.Values)
            {
                if (character.CharacterId != null)
                {
                    JsonElement characterInventory = equipmentArray.GetProperty(character.CharacterId).GetProperty("items");

                    foreach (JsonElement itemJson in characterInventory.EnumerateArray())
                    {
                        Item item = itemJson.Deserialize<Item>() ?? new Item();
                        item.Identifier = $"{character.CharacterId}Equipped";

                        if (this.GetItemPerks(perksDataArray, item, out List<Item.Perk> perks))
                        {
                            item.Perks = perks;
                        }

                        // Get needed data
                        item.Power = this.GetItemPrimaryStat(instanceDataArray, item.ItemInstanceId ?? string.Empty);

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
            this.logger.LogError($"GVM was unable to retrieve the client's equipment: {e.Message}");
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
            // Fetch Inventories contents and associated data.
            string[] requests =
            [
                "CharacterInventories",
                "ItemInstances",
                "ItemPerks",
            ];
            string inventoriesContentsJson = await this.request.PlatformDestiny2_Request_ProfileContents(requests, true);

            JsonDocument inventoriesContents = JsonDocument.Parse(inventoriesContentsJson);
            JsonElement inventoriesArray = inventoriesContents.RootElement.GetProperty("Response").GetProperty("characterInventories").GetProperty("data");
            JsonElement instanceDataArray = inventoriesContents.RootElement.GetProperty("Response").GetProperty("itemComponents").GetProperty("instances").GetProperty("data");
            JsonElement perksDataArray = inventoriesContents.RootElement.GetProperty("Response").GetProperty("itemComponents").GetProperty("perks").GetProperty("data");

            // Populate Inventories list
            int itemsLoaded = 0;
            foreach (Character character in this.characters.Values)
            {
                if (character.CharacterId != null)
                {
                    JsonElement characterInventory = inventoriesArray.GetProperty(character.CharacterId).GetProperty("items");

                    foreach (JsonElement itemJson in characterInventory.EnumerateArray())
                    {
                        Item item = itemJson.Deserialize<Item>() ?? new Item();
                        item.Identifier = character.CharacterId;

                        if (this.GetItemPerks(perksDataArray, item, out List<Item.Perk> perks))
                        {
                            item.Perks = perks;
                        }

                        // Get needed data
                        item.Power = this.GetItemPrimaryStat(instanceDataArray, item.ItemInstanceId ?? string.Empty);

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
            this.logger.LogError($"GVM was unable to retrieve the client's inventories: {e.Message}");
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
            // Fetch Inventories contents and associated data.
            string[] requests =
            [
                "ProfileInventories",
                "ItemInstances",
                "ItemPerks",
            ];
            string vaultContentsJson = await this.request.PlatformDestiny2_Request_ProfileContents(requests, true);

            JsonDocument vaultContents = JsonDocument.Parse(vaultContentsJson);
            JsonElement vaultArray = vaultContents.RootElement.GetProperty("Response").GetProperty("profileInventory").GetProperty("data").GetProperty("items");
            JsonElement instanceDataArray = vaultContents.RootElement.GetProperty("Response").GetProperty("itemComponents").GetProperty("instances").GetProperty("data");
            JsonElement perksDataArray = vaultContents.RootElement.GetProperty("Response").GetProperty("itemComponents").GetProperty("perks").GetProperty("data");

            // Populate Vault list
            int itemsLoaded = 0;
            foreach (JsonElement itemElement in vaultArray.EnumerateArray())
            {
                Item item = itemElement.Deserialize<Item>() ?? new Item();
                item.Identifier = "Vault";

                if (this.GetItemPerks(perksDataArray, item, out List<Item.Perk> perks))
                {
                    item.Perks = perks;
                }

                // Get needed data
                item.Power = this.GetItemPrimaryStat(instanceDataArray, item.ItemInstanceId ?? string.Empty);
                
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
            this.logger.LogError($"GVM was unable to retrieve the client's vault: {e.Message}");
        }
    }

    /// <summary>
    /// Gets the perks for the item associated with the item instance ID.
    /// </summary>
    /// <param name="perkData">Perk data received from bungie.</param>
    /// <param name="item">Item to get the perks for.</param>
    /// <param name="perks">The list of perks for the item.</param>
    /// <returns>A Perk class constructed from the time.</returns>
    private bool GetItemPerks(JsonElement perkData, Item item, out List<Item.Perk> perks)
    {
        perks = new List<Item.Perk>();
        if (perkData.TryGetProperty(item.ItemInstanceId ?? string.Empty, out JsonElement itemPerks))
        {
            if (itemPerks.TryGetProperty("perks", out JsonElement perksArray))
            {
                foreach (var perk in perksArray.EnumerateArray())
                {
                    perks.Add(new Item.Perk(
                        perk.GetProperty("perkHash").GetUInt32(),
                        perk.GetProperty("iconPath").GetString(),
                        perk.GetProperty("isActive").GetBoolean(),
                        perk.GetProperty("visible").GetBoolean()));
                }
            }

            return true;
        }

        Console.WriteLine($"Perk data not found for item instance: {item.ItemInstanceId}");
        return false;
    }

    /// <summary>
    /// Adds the item to the correct store in content manager.
    /// </summary>
    /// <param name="item">Item to add.</param>
    private void AddItemToStore(Item item)
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
    /// Gets the item details and bucket information for the clients items.
    /// </summary>
    /// <param name="items">List of items to fetch details for.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task FetchItemInformation(List<Item> items)
    {
        this.logger.LogInformation("Getting Clients Item Information.");
        foreach (Item item in items)
        {
            await this.QueryManifestForDetails(item);
            await this.QueryManifestForBuckets(item);
        }
    }

    /// <summary>
    /// Creates a dictionary for the clients weapons.
    /// </summary>
    /// <param name="item">Item to get information for.</param>
    /// <returns>Task representing the operation.</returns>
    private async Task QueryManifestForDetails(Item item)
    {
        uint itemHash = item.ItemHash;
        if (!this.itemDetails.TryGetValue(itemHash, out _))
        {
            var itemDetails = await this.GetItemDetails(itemHash);

            if (itemDetails.Equals(new ItemDetails()))
            {
                this.snackbar.AddMessage("GVM is unable to define an item.", "warning");
            }

            lock (this.itemDetails)
            {
                this.itemDetails.Add(itemHash, itemDetails);
            }
        }
    }

    /// <summary>
    /// Assigns each item in the given list into their buckets.
    /// </summary>
    /// <param name="item">Item to get information for.</param>
    /// <returns>Task representing the operation.</returns>
    private async Task QueryManifestForBuckets(Item item)
    {
        uint hash = item.ItemHash;
        if (string.IsNullOrEmpty(item.Bucket))
        {
            if (item.Identifier == "Vault")
            {
                item.Bucket = await this.GetBucketDefinition(this.itemDetails[hash].Inventory.BucketTypeHash);
            }
            else
            {
                item.Bucket = await this.GetBucketDefinition(item.BucketHash);
            }
        }
        else
        {
            this.logger.LogWarning($"Bucket not found for item: {item.ItemHash}, BucketHash: {item.BucketHash}");
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
        if (bucketDefinition.RootElement.ValueKind == JsonValueKind.Object && bucketDefinition.RootElement.TryGetProperty("displayProperties", out JsonElement displayProperties))
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
        if (instanceDataArray.TryGetProperty(instanceID, out JsonElement itemInstanceData) && itemInstanceData.TryGetProperty("primaryStat", out JsonElement primaryStat))
        {
            // Retrieve items power level and assign it to the item
            if (primaryStat.TryGetProperty("value", out JsonElement powerLevel))
            {
                return powerLevel.GetInt32();
            }
        }

        return 0;
    }
}
