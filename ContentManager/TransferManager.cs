// <copyright file="TransferManager.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace ContentManager;

using GuardianServices.Models;
using Microsoft.Extensions.Logging;
using GuardianServices;
using ClientProfile;
using RequestManager;

/// <summary>
/// Handles and processes transfer tasks.
/// </summary>
public class TransferManager
{
    /// <summary>
    /// Logger for Dexus.
    /// </summary>
    private readonly ILogger logger;

    /// <summary>
    /// Snackbar Service for Dexus.
    /// </summary>
    private readonly SnackbarService snackbar;

    /// <summary>
    /// User profile.
    /// </summary>
    private readonly Profile profile;

    /// <summary>
    /// Request Server for Dexus.
    /// </summary>
    private readonly RequestServer request;

    /// <summary>
    /// Contains queued transfers that have not been processed yet.
    /// </summary>
    private Queue<TransferInfo> queuedTransfers = new Queue<TransferInfo>();

    /// <summary>
    /// Contains a Dictionary of transfers handled in the last 10 seconds.
    /// </summary>
    private TimedDictionary<string, bool> recentlyHandledTransfers = new TimedDictionary<string, bool>(TimeSpan.FromSeconds(10));

    /// <summary>
    /// Task that handles the proccessing of transfers.
    /// </summary>
    private Task? transferTask;
    private CancellationTokenSource? cancellationTokenSource;

    /// <summary>
    /// Initializes a new instance of the <see cref="TransferManager"/> class.
    /// </summary>
    /// <param name="logger">Logger for TransferManager to use.</param>
    /// <param name="profile">Profile for the user.</param>
    /// <param name="request">RequestService used by Dexus.</param>
    /// <param name="snackbar">Snackbar for TransferManager to use.</param>
    /// <param name="dropzoneBackingStore">Dropzone Backing Store managed by content manager.</param>
    public TransferManager(ILogger logger, Profile profile, RequestServer request, SnackbarService snackbar, Dictionary<string, DItem> dropzoneBackingStore)
    {
        this.logger = logger;
        this.snackbar = snackbar;
        this.profile = profile;
        this.request = request;
        this.DropzoneBackingStore = dropzoneBackingStore;
    }

    /// <summary>
    /// Signal that informs all connected slots that a transfer has occured.
    /// </summary>
    public event Action? TransferOccured;

    /// <summary>
    /// Signal that informs the content manager that a change in identifier is needed.
    /// </summary>
    public event Action<(DItem, string)>? SetIdentifier;

    /// <summary>
    /// Gets a value indicating whether if transfers are currently being handled.
    /// </summary>
    public bool TransfersAllowed { get; private set; }

    /// <summary>
    /// Gets a reference to the dropzone backing store used by content manager.
    /// </summary>
    private Dictionary<string, DItem> DropzoneBackingStore { get; }

    /// <summary>
    /// Adds the request to the list of transfers. This overload should only be used for a transfer type of "Pull".
    /// </summary>
    /// <param name="type">Type of transfer wanted.</param>
    /// <param name="item">Item to transfer.</param>
    /// <returns>The Item instance ID of the item. Can be used to track the progress of the transfer.</returns>
    public string QueueTransfer(TransferType type, DItem item)
    {
        this.queuedTransfers.Enqueue(new TransferInfo(type, item, string.Empty));
        return item.ItemInstanceId ?? string.Empty;
    }

    /// <summary>
    /// Adds the request to the list of transfers.
    /// </summary>
    /// <param name="type">Type of transfer wanted.</param>
    /// <param name="item">Item to transfer.</param>
    /// <param name="destination">Where the item should go.</param>
    /// <returns>The Item instance ID of the item. Can be used to track the progress of the transfer.</returns>
    public string QueueTransfer(TransferType type, DItem item, string destination)
    {
        this.queuedTransfers.Enqueue(new TransferInfo(type, item, destination));
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
    /// Allows transfer manager to start processing transfers.
    /// </summary>
    public void AllowTransfers()
    {
        this.TransfersAllowed = true;

        // Create a cancellation token source to signal task cancellation
        this.cancellationTokenSource = new CancellationTokenSource();
        CancellationToken token = this.cancellationTokenSource.Token;

        // If transfers are already being processed, return as to not overwrite the task.
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
                                    transferSuccess = await this.Transfer(transfer.Item, transfer.Item.Identifier, transfer.Destination);
                                    break;
                                case TransferType.Pull:
                                    transferSuccess = await this.PullFromPostmaster(transfer.Item);
                                    break;
                            }

                            this.recentlyHandledTransfers.Add(
                                transfer.Item.ItemInstanceId ?? string.Empty, transferSuccess);
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
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task StopTransfers()
    {
        this.TransfersAllowed = false;

        this.cancellationTokenSource?.Cancel();
        this.transferTask?.Wait();

        this.cancellationTokenSource?.Dispose();
        this.transferTask?.Dispose();

        while (this.recentlyHandledTransfers.Any())
        {
            await Task.Delay(1000);
        }

        this.recentlyHandledTransfers.StopAutoCleanup();
    }

    /// <summary>
    /// Pulls the requested item from the postmaster.
    /// </summary>
    /// <param name="item">Item to pull from postmaster.</param>
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

        this.TransferOccured?.Invoke();
        return await this.request.ActionItem_Request_PullFromPostmaster(this.profile.MembershipType, item.ItemHash,
            item.ItemInstanceId ?? string.Empty, item.Quantity, item.Identifier.Replace("Equipped", string.Empty));
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
        if (itemBeingReplaced != null && await this.request.ActionItem_Request_EquipItem(
                this.profile.MembershipType, item.ItemInstanceId ?? string.Empty, location))
        {
            this.SetIdentifier?.Invoke((itemBeingReplaced, location));
            this.SetIdentifier?.Invoke((item, $"{location}Equipped"));
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
            this.SetIdentifier?.Invoke((item, destination));
        }

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
                "Something went wrong. Deuxs is unable to access your Bungie data. Transfer cannot be completed. Try logging back in to solve this issue.",
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

        if (origin == "Vault")
        {
            // Transfer to Character
            return await this.request.ActionItem_Request_TransferItem(this.profile.MembershipType, itemHash, itemID,
                stackSize, destination, false);
        }

        // Transfer to Vault then to the Character
        bool firstTransfer = await this.request.ActionItem_Request_TransferItem(
            this.profile.MembershipType, itemHash, itemID, stackSize, origin, true);

        bool secondTransfer = false;
        if (firstTransfer)
        {
            secondTransfer = await this.request.ActionItem_Request_TransferItem(
                this.profile.MembershipType, itemHash, itemID, stackSize, destination, false);
        }

        if (firstTransfer && secondTransfer)
        {
            return true;
        }

        if (!secondTransfer)
        {
            this.snackbar.AddMessage(
                $"Transfer of {transfer.Details.DisplayProperties.Name} failed. Returning the item to the origin.",
                "Warning");
            bool returnSuccessful = await this.request.ActionItem_Request_TransferItem(
                this.profile.MembershipType, itemHash, itemID, stackSize, origin, false);

            if (!returnSuccessful)
            {
                this.SetIdentifier?.Invoke((transfer, "Vault"));
                this.snackbar.AddMessage(
                    $"Transfer failed. {transfer.Details.DisplayProperties.Name} " +
                    "has ended up in the Vault as GVM was unable to return it.", "Warning");
            }

            return returnSuccessful;
        }

        return false;
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

        this.snackbar.AddMessage("GVM was unable to equip a different item to replace the item you wanted to transfer.", "Warning");
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
}