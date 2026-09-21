// <copyright file="Item.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace GuardianServices.Models;

using System.Text.Json.Serialization;

/// <summary>
/// Object that represents an item in destiny 2.
/// </summary>
public class Item
{
    /// <summary>
    /// Gets or sets the item hash.
    /// </summary>
    [JsonPropertyName("itemHash")]
    public uint ItemHash { get; set; }

    /// <summary>
    /// Gets or sets the item instance ID.
    /// </summary>
    [JsonPropertyName("itemInstanceId")]
    public string? ItemInstanceId { get; set; }

    /// <summary>
    /// Gets or sets the quantity.
    /// </summary>
    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }

    /// <summary>
    /// Gets or sets the bind status.
    /// </summary>
    [JsonPropertyName("bindStatus")]
    public int BindStatus { get; set; }

    /// <summary>
    /// Gets or sets the location.
    /// </summary>
    [JsonPropertyName("location")]
    public int Location { get; set; }

    /// <summary>
    /// Gets or sets the bucket hash.
    /// </summary>
    [JsonPropertyName("bucketHash")]
    public uint BucketHash { get; set; }

    /// <summary>
    /// Gets or sets the transfer status.
    /// </summary>
    [JsonPropertyName("transferStatus")]
    public int TransferStatus { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the item is lockable.
    /// </summary>
    [JsonPropertyName("lockable")]
    public bool Lockable { get; set; }

    /// <summary>
    /// Gets or sets the state.
    /// </summary>
    [JsonPropertyName("state")]
    public int State { get; set; }

    /// <summary>
    /// Gets or sets the dismantleable permission.
    /// </summary>
    [JsonPropertyName("dismantlePermission")]
    public int DismantlePermission { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the item is a wrapper.
    /// </summary>
    [JsonPropertyName("isWrapper")]
    public bool IsWrapper { get; set; }

    /// <summary>
    /// Gets or sets the tooltip notification indexes.
    /// </summary>
    [JsonPropertyName("tooltipNotificationIndexes")]
    public List<int>? TooltipNotificationIndexes { get; set; }

    /// <summary>
    /// Gets or sets the version number.
    /// </summary>
    [JsonPropertyName("versionNumber")]
    public int? VersionNumber { get; set; }

    /// <summary>
    /// Copies all information from the object this is called on to the provided target Item.
    /// </summary>
    /// <param name="target">Target to copy to.</param>
    public void CopyTo(Item target)
    {
        target.ItemHash = this.ItemHash;
        target.ItemInstanceId = this.ItemInstanceId;
        target.Quantity = this.Quantity;
        target.BindStatus = this.BindStatus;
        target.Location = this.Location;
        target.BucketHash = this.BucketHash;
        target.TransferStatus = this.TransferStatus;
        target.Lockable = this.Lockable;
        target.State = this.State;
        target.DismantlePermission = this.DismantlePermission;
        target.IsWrapper = this.IsWrapper;
        target.TooltipNotificationIndexes = this.TooltipNotificationIndexes;
        target.VersionNumber = this.VersionNumber;
    }
}