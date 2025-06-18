// <copyright file="ContentManager.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace ContentManager;

using ClientProfile;
using GuardianServices;
using GuardianServices.Models;
using Microsoft.Extensions.Logging;
using RequestManager;
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
    private readonly ILogger logger;

    /// <summary>
    /// User profile.
    /// </summary>
    private readonly Profile profile;

    /// <summary>
    /// Request Server for GVM.
    /// </summary>
    private readonly RequestServer request;

    /// <summary>
    /// Manifest Server for GVM.
    /// </summary>
    private readonly ManifestServer manifest;

    /// <summary>
    /// Snackbar Service for GVM.
    /// </summary>
    private readonly SnackbarService snackbar;

    /// <summary>
    /// Loadingbar service for GVM.
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
    private Dictionary<long, SocketTypeDetails> socketDetails = new Dictionary<long, SocketTypeDetails>();

    // Content Management Containers
    private Queue<TransferInfo> queuedTransfers = new Queue<TransferInfo>();

    private TimedDictionary<string, bool> recentlyHandledTransfers =
        new TimedDictionary<string, bool>(TimeSpan.FromSeconds(10));

    // Content Tasks
    private Task? transferTask;
    private CancellationTokenSource? cancellationTokenSource;

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
    }

    /// <summary>
    /// Signal that informs all connected slots that a transfer has occured.
    /// </summary>
    public event Action? TransferOccured;

    /// <summary>
    /// Gets the Characters List.
    /// </summary>
    public Dictionary<string, Character> Characters { get => this.characters; }

    /// <summary>
    /// Gets the KineticWeapons List.
    /// </summary>
    public Dictionary<string, DItem> KineticWeapons
    {
        get
        {
            return this.dropzoneBackingStore.Where(i => i.Value.Bucket == "Kinetic Weapons")
                .ToDictionary(i => i.Key, i => i.Value);
        }
    }

    /// <summary>
    /// Gets the EnergyWeapons List.
    /// </summary>
    public Dictionary<string, DItem> EnergyWeapons
    {
        get
        {
            return this.dropzoneBackingStore.Where(i => i.Value.Bucket == "Energy Weapons")
                .ToDictionary(i => i.Key, i => i.Value);
        }
    }

    /// <summary>
    /// Gets the PowerWeapons List.
    /// </summary>
    public Dictionary<string, DItem> PowerWeapons
    {
        get
        {
            return this.dropzoneBackingStore.Where(i => i.Value.Bucket == "Power Weapons")
                .ToDictionary(i => i.Key, i => i.Value);
        }
    }

    /// <summary>
    /// Gets the Helmets List.
    /// </summary>
    public Dictionary<string, DItem> Helmets
    {
        get
        {
            return this.dropzoneBackingStore.Where(i => i.Value.Bucket == "Helmet")
                .ToDictionary(i => i.Key, i => i.Value);
        }
    }

    /// <summary>
    /// Gets the Gauntlets List.
    /// </summary>
    public Dictionary<string, DItem> Gauntlets
    {
        get
        {
            return this.dropzoneBackingStore.Where(i => i.Value.Bucket == "Gauntlets")
                .ToDictionary(i => i.Key, i => i.Value);
        }
    }

    /// <summary>
    /// Gets the Chests List.
    /// </summary>
    public Dictionary<string, DItem> Chests
    {
        get
        {
            return this.dropzoneBackingStore.Where(i => i.Value.Bucket == "Chest Armor")
                .ToDictionary(i => i.Key, i => i.Value);
        }
    }

    /// <summary>
    /// Gets the Legs List.
    /// </summary>
    public Dictionary<string, DItem> Legs
    {
        get
        {
            return this.dropzoneBackingStore.Where(i => i.Value.Bucket == "Leg Armor")
                .ToDictionary(i => i.Key, i => i.Value);
        }
    }

    /// <summary>
    /// Gets the Skirts List.
    /// </summary>
    public Dictionary<string, DItem> Skirts
    {
        get
        {
            return this.dropzoneBackingStore.Where(i => i.Value.Bucket == "Class Armor")
                .ToDictionary(i => i.Key, i => i.Value);
        }
    }

    /// <summary>
    /// Gets the Accessories List.
    /// </summary>
    public Dictionary<string, DItem> Accessories
    {
        get
        {
            string[] buckets =
            [
                "Ghost",
                "Ships",
                "Vehicle",
                "Emblems",
                "Accessories"
            ];

            return this.dropzoneBackingStore.Where(i => buckets.Contains(i.Value.Bucket))
                .ToDictionary(i => i.Key, i => i.Value);
        }
    }

    /// <summary>
    /// Gets the Postmaster List.
    /// </summary>
    public Dictionary<string, DItem> Postmaster
    {
        get
        {
            return this.dropzoneBackingStore.Where(i => i.Value.Bucket == "Lost Items")
                .ToDictionary(i => i.Key, i => i.Value);
        }
    }

    /// <summary>
    /// Gets the Engrams List.
    /// </summary>
    public Dictionary<string, DItem> Engrams
    {
        get
        {
            return this.dropzoneBackingStore.Where(i => i.Value.Bucket == "Engrams")
                .ToDictionary(i => i.Key, i => i.Value);
        }
    }

    /// <summary>
    /// Gets the Quests List.
    /// </summary>
    public Dictionary<string, DItem> Quests
    {
        get
        {
            string[] buckets =
            [
                "Messages",
                "Special Orders",
                "Quests"
            ];

            return this.dropzoneBackingStore.Where(i => buckets.Contains(i.Value.Bucket))
                .ToDictionary(i => i.Key, i => i.Value);
        }
    }

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
    /// Gets a value indicating whether if transfers are currently being handled.
    /// </summary>
    public bool TransfersAllowed { get; private set; }

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
    /// Adds the request to the list of transfers. This overload should only be used for a transfer type of "Pull".
    /// </summary>
    /// <param name="type">Type of transfer wanted.</param>
    /// <param name="item">Item to transfer.</param>
    /// <returns>The Item instance ID of the item. Can be used to track the progress of the transfer.</returns>
    public string QueueTransfer(TransferType type, DItem item)
    {
        this.queuedTransfers.Enqueue(new TransferInfo(type, item, string.Empty, string.Empty));
        return item.ItemInstanceId ?? string.Empty;
    }

    /// <summary>
    /// Adds the request to the list of transfers.
    /// </summary>
    /// <param name="type">Type of transfer wanted.</param>
    /// <param name="item">Item to transfer.</param>
    /// <param name="origin">Where the item is.</param>
    /// <param name="destination">Where the item should go.</param>
    /// <returns>The Item instance ID of the item. Can be used to track the progress of the transfer.</returns>
    public string QueueTransfer(TransferType type, DItem item, string origin, string destination)
    {
        this.queuedTransfers.Enqueue(new TransferInfo(type, item, origin, destination));
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

        // If transfers are already being processed return as to not overwrite the task.
        if (this.transferTask != null && this.transferTask.Status == TaskStatus.Running)
        {
            return;
        }

        this.logger.LogInformation(
            "Transfer processing task was started. Content Manager will now handle transfer requests.");
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
                            bool transferSuccess = false;

                            switch (transfer.Type)
                            {
                                case TransferType.Push:
                                    transferSuccess = await this.Transfer(transfer.Item, transfer.Origin,
                                        transfer.Destination);
                                    break;
                                case TransferType.Pull:
                                    transferSuccess = await this.PullFromPostmaster(transfer.Item);
                                    break;
                            }

                            this.recentlyHandledTransfers.Add(transfer.Item.ItemInstanceId ?? string.Empty,
                                transferSuccess);
                        }

                        await Task.Delay(100, token);
                    }
                }
                catch (OperationCanceledException)
                {
                    this.logger.LogInformation(
                        "Transfer processing task was canceled. Content Manager will not handle anymore transfer requests.");
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

        while (this.recentlyHandledTransfers.Any())
        {
            Task.Delay(1000);
        }

        this.recentlyHandledTransfers.StopAutoCleanup();
    }

    /// <summary>
    /// Performs all tasks related to loading or refreshing the user's content.
    /// </summary>
    /// <returns>A Task representing the operation.</returns>
    private async Task LoadUserContents()
    {
        // Erase old information if needed.
        if (this.dropzoneBackingStore.Count != 0)
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

        // Inform all dependents that information from the DBS has changed
        this.itemManagementRenderKey = Guid.NewGuid();
    }

    /// <summary>
    /// Equips the item to the provided character.
    /// </summary>
    /// <param name="item">Item to equip.</param>
    /// <param name="location">Character id of the item location.</param>
    /// <returns>True if the transfer operation was successful. False otherwise.</returns>
    private async Task<bool> Equip(DItem item, string location)
    {
        if (this.profile.MembershipType == null)
        {
            this.logger.LogCritical("MembershipType is null. Unable to transfer item.");
            this.snackbar.AddMessage(
                "Something went wrong. GVM is unable to access your bungie data. Equip cannot be completed. Try logging back in to solve this issue.",
                "Error");
            return false;
        }

        // Equip the item.
        DItem? itemBeingReplaced =
            this.DropzoneBackingStore.Where(i => i.Value.Identifier == $"{location}Equipped").Select(i => i.Value)
                .FirstOrDefault();
        if (itemBeingReplaced != null && await this.request.ActionItem_Request_EquipItem(this.profile.MembershipType,
                item.ItemInstanceId ?? string.Empty, location))
        {
            this.SetIdentifier(itemBeingReplaced, location);
            this.SetIdentifier(item, $"{location}Equipped");
            return true;
        }

        return false;
    }

    /// <summary>
    /// Pulls the requested item from the postmaster.
    /// </summary>
    /// <param name="item">Item to fetch.</param>
    /// <returns>True if the operation was completed successfully. False Otherwise.</returns>
    private async Task<bool> PullFromPostmaster(DItem item)
    {
        if (this.profile.MembershipType == null)
        {
            this.logger.LogCritical("MembershipType is null. Unable to transfer item.");
            this.snackbar.AddMessage(
                "Something went wrong. GVM is unable to access your bungie data. Transfer cannot be completed. Try logging back in to solve this issue.",
                "Error");
            return false;
        }

        this.itemManagementRenderKey = Guid.NewGuid();
        return await this.request.ActionItem_Request_PullFromPostmaster(this.profile.MembershipType, item.ItemHash,
            item.ItemInstanceId ?? string.Empty, item.Quantity, item.Identifier ?? string.Empty);
    }

    /// <summary>
    /// Transfers the provided item to the provided destination.
    /// </summary>
    /// <param name="item">Item to transfer.</param>
    /// <param name="origin">Origin location (CharacterID or Vault) of the item.</param>
    /// <param name="destination">Destination location (CharacterID or Vault) of the item.</param>
    /// <returns>True if the transfer operation was successful. False otherwise.</returns>
    private async Task<bool> Transfer(DItem item, string origin, string destination)
    {
        bool transferSuccess;
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
        this.TransferOccured?.Invoke();
        return transferSuccess;
    }

    /// <summary>
    /// Handles the transfer of an item between buckets.
    /// </summary>
    /// <param name="transfer">Item to transfer.</param>
    /// <param name="origin">Origin of the transfer.</param>
    /// <param name="destination">Destination of the transfer.</param>
    /// <returns>True if the transfer operation was successful. False otherwise.</returns>
    private async Task<bool> TransferItem(DItem transfer, string origin, string destination)
    {
        if (this.profile.MembershipType == null)
        {
            this.logger.LogCritical("MembershipType is null. Unable to transfer item.");
            this.snackbar.AddMessage(
                "Something went wrong. GVM is unable to access your bungie data. Transfer cannot be completed. Try logging back in to solve this issue.",
                "Error");
            return false;
        }

        this.GenerateTransferData(transfer, out uint itemHash, out string itemID, out int stackSize);

        if (destination == "Vault")
        {
            // Transfer to Vault
            return await this.request.ActionItem_Request_TransferItem(this.profile.MembershipType, itemHash, itemID,
                stackSize, origin, true);
        }
        else if (origin == "Vault")
        {
            // Transfer to Character
            return await this.request.ActionItem_Request_TransferItem(this.profile.MembershipType, itemHash, itemID,
                stackSize, destination, false);
        }
        else
        {
            // Transfer to Vault then to the Character
            bool firstTransfer = await this.request.ActionItem_Request_TransferItem(this.profile.MembershipType,
                itemHash, itemID, stackSize, origin, true);

            bool secondTransfer = false;
            if (firstTransfer)
            {
                secondTransfer = await this.request.ActionItem_Request_TransferItem(this.profile.MembershipType,
                    itemHash, itemID, stackSize, destination, false);
            }

            if (firstTransfer && secondTransfer)
            {
                return true;
            }
            else if (!secondTransfer)
            {
                this.snackbar.AddMessage(
                    $"Transfer of {transfer.Details.DisplayProperties.Name} failed. Returning the item to the origin.",
                    "Warning");
                bool returnSuccessful = await this.request.ActionItem_Request_TransferItem(this.profile.MembershipType,
                    itemHash, itemID, stackSize, origin, false);

                if (!returnSuccessful)
                {
                    this.SetIdentifier(transfer, "Vault");
                    this.snackbar.AddMessage(
                        $"Transfer failed. {transfer.Details.DisplayProperties.Name} " +
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
    private async Task<bool> TransferEquipped(DItem transfer, string origin, string destination)
    {
        string trueDestination = destination.Replace("Equipped", string.Empty);
        bool transferSuccess;
        if (origin.Contains("Equipped") && destination.Contains("Equipped"))
        {
            transferSuccess = await this.HandelTransferOfEquippedItem(transfer, origin, destination);
        }
        else if (origin.Contains("Equipped"))
        {
            return await this.HandelTransferOfEquippedItem(transfer, origin, destination);
        }
        else
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
    private async Task<bool> HandelTransferOfEquippedItem(DItem item, string origin, string destination)
    {
        string trueOrigin = origin.Replace("Equipped", string.Empty);
        string trueDestination = destination.Replace("Equipped", string.Empty);

        // Get all items eligible to replace equipped item
        Dictionary<string, DItem> charactersItemBucket = this.DropzoneBackingStore
            .Where(b => b.Value.Bucket == item.Bucket).Where(i => i.Value.Identifier == trueOrigin)
            .ToDictionary(i => i.Key, i => i.Value);

        if (!charactersItemBucket.Any())
        {
            this.snackbar.AddMessage(
                "Character has no items to to replace the equipped item. Transfer another weapon into your inventory to fix this.",
                "Warning");
            return false; // TODO: Not the best solution. Could pull item from vault to equip, but I am too lazy.
        }

        DItem itemToEquip = charactersItemBucket.Values.First();

        bool equipCompleted = await this.Equip(itemToEquip, trueOrigin);
        if (equipCompleted)
        {
            return await this.TransferItem(item, trueOrigin, trueDestination);
        }

        this.snackbar.AddMessage("GVM was unable to equip a different item to replace the item you wanted to transfer.",
            "Warning");
        return false;
    }

    /// <summary>
    /// Generates the data required for a transfer.
    /// </summary>
    /// <param name="transfer">Item to extract data for.</param>
    /// <param name="itemHash">The item hash of the item.</param>
    /// <param name="itemID">The item ID of the item.</param>
    /// <param name="stackSize">The item stack size of the item.</param>
    private void GenerateTransferData(DItem transfer, out uint itemHash, out string itemID, out int stackSize)
    {
        itemHash = transfer.ItemHash;
        itemID = transfer.ItemInstanceId ?? "-1";
        stackSize = transfer.Quantity;
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
                "ItemReusablePlugs",
            ];
            string equipmentContentsJson = await this.request.PlatformDestiny2_Request_ProfileContents(requests, true);
            await File.WriteAllTextAsync(
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FullItemComponents.json"),
                equipmentContentsJson);

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
                            plugsDataArray, instanceDataArray);
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
                "ItemReusablePlugs",
            ];
            string inventoriesContentsJson =
                await this.request.PlatformDestiny2_Request_ProfileContents(requests, true);

            JsonDocument inventoriesContents = JsonDocument.Parse(inventoriesContentsJson);
            JsonElement inventoriesArray = inventoriesContents.RootElement.GetProperty("Response")
                .GetProperty("characterInventories").GetProperty("data");
            JsonElement instanceDataArray = inventoriesContents.RootElement.GetProperty("Response")
                .GetProperty("itemComponents").GetProperty("instances").GetProperty("data");
            JsonElement perksDataArray = inventoriesContents.RootElement.GetProperty("Response")
                .GetProperty("itemComponents").GetProperty("perks").GetProperty("data");
            JsonElement plugsDataArray = inventoriesContents.RootElement.GetProperty("Response")
                .GetProperty("itemComponents").GetProperty("reusablePlugs").GetProperty("data");

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
                            plugsDataArray,
                            instanceDataArray);
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
                "ItemReusablePlugs",
            ];
            string vaultContentsJson = await this.request.PlatformDestiny2_Request_ProfileContents(requests, true);

            JsonDocument vaultContents = JsonDocument.Parse(vaultContentsJson);
            JsonElement vaultArray = vaultContents.RootElement.GetProperty("Response").GetProperty("profileInventory")
                .GetProperty("data").GetProperty("items");
            JsonElement instanceDataArray = vaultContents.RootElement.GetProperty("Response")
                .GetProperty("itemComponents").GetProperty("instances").GetProperty("data");
            JsonElement perksDataArray = vaultContents.RootElement.GetProperty("Response").GetProperty("itemComponents")
                .GetProperty("perks").GetProperty("data");
            JsonElement plugsDataArray = vaultContents.RootElement.GetProperty("Response")
                .GetProperty("itemComponents").GetProperty("reusablePlugs").GetProperty("data");

            // Populate Vault list
            int itemsLoaded = 0;
            foreach (JsonElement itemElement in vaultArray.EnumerateArray())
            {
                DItem item = await this.BuildDItem(itemElement, "Vault", perksDataArray, plugsDataArray, instanceDataArray);
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
    /// Builds an Item based on the provided data. TODO: Now needs to handel fetching item information from the Manifest.
    /// </summary>
    /// <param name="itemJson">The base item data from bungie.</param>
    /// <param name="itemIdentifier">Where the item is located.</param>
    /// <param name="perksDataArray">Data for all the items perks.</param>
    /// <param name="plugsDataArray">Data for all the items plugs.</param>
    /// <param name="instanceDataArray">Data for all the items instance data.</param>
    /// <returns>The Item object that was built.</returns>
    private async Task<DItem> BuildDItem(JsonElement itemJson, string itemIdentifier, JsonElement perksDataArray,
        JsonElement plugsDataArray, JsonElement instanceDataArray)
    {
        Item bungieItem = itemJson.Deserialize<Item>() ?? new Item();
        DItem? item = null;
        if (!bungieItem.Equals(new Item()))
        {
            ItemDetails details = await this.QueryManifestForGeneralItemDetails(bungieItem);
            string bucket = await this.QueryManifestForBucket(bungieItem, itemIdentifier);
            item = DItem.FromBungieItem(bungieItem, details, bucket, itemIdentifier);

            item.Power = this.GetItemPrimaryStat(instanceDataArray, bungieItem.ItemInstanceId ?? string.Empty);
            await this.GetItemPerks(item, perksDataArray);
            await this.GetItemPlugs(item, plugsDataArray);
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
                            this.snackbar.AddMessage("GVM is unable to define a perk.", "warning");
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
    /// Gets the Items Sockets and the Sockets perks for the item from the provided data.
    /// </summary>
    /// <param name="item">Item to assign the sockets to.</param>
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
                            this.snackbar.AddMessage("GVM could not define a plug.", "warning");
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
    /// Creates an ItemDetail for the client's weapon and adds it to the Item Details list.
    /// </summary>
    /// <param name="item">Item to get information for.</param>
    /// <returns>Task representing the operation.</returns>
    private async Task<ItemDetails> QueryManifestForGeneralItemDetails(Item item)
    {
        uint itemHash = item.ItemHash;

        ItemDetails? details = null;
        if (this.itemDetails.TryGetValue(itemHash, out var itemDetails))
        {
            return itemDetails;
        }

        details = await this.GetItemDetailsFromManifest(itemHash);
        if (details.Equals(new ItemDetails()))
        {
            this.snackbar.AddMessage("GVM is unable to define an item.", "warning");
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
                $"GVM could not find any ItemDetails for the itemHash={itemHash} in the manifest or and has attempted to get it from Bungie.");
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
    /// <param name="item">Item to set identifier of.</param>
    /// <param name="identifier">Identifier to set.</param>
    private void SetIdentifier(DItem item, string identifier)
    {
        this.dropzoneBackingStore[item.ItemInstanceId ?? string.Empty].Identifier = identifier;
    }
}