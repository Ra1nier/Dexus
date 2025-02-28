// <copyright file="Item.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace ContentManager.Models;

using System.Text.Json.Serialization;
using System.Runtime.Serialization;

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
    public string ItemInstanceId { get; set; }

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
    /// Gets or sets the bucket.
    /// </summary>
    [JsonIgnore]
    public string? Bucket { get; set; }

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
    /// Gets or sets the dismantle permission.
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
    /// Gets or sets the perks for the item.
    /// </summary>
    [JsonIgnore]
    public List<Perk> Perks { get; set; }

    /// <summary>
    /// Gets or sets the identifier.
    /// </summary>
    [JsonIgnore]
    public string? Identifier { get; set; }

    /// <summary>
    /// Gets or sets the IconPath.
    /// </summary>
    [JsonIgnore]
    public string? IconPath { get; set; }

    /// <summary>
    /// Gets or sets the Power.
    /// </summary>
    [JsonIgnore]
    public int Power { get; set; }

    /// <summary>
    /// Perk represents a perk slot that an item holds.
    /// </summary>
    public class Perk
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Perk"/> class.
        /// </summary>
        /// <param name="perkHash">PerkHash.</param>
        /// <param name="iconPath">IconPath.</param>
        /// <param name="isActive">IsEquipped.</param>
        /// <param name="visible">IsVisible.</param>
        public Perk(uint perkHash, string iconPath, bool isActive, bool visible)
        {
            this.PerkHash = perkHash;
            this.IconPath = iconPath;
            this.IsActive = isActive;
            this.IsVisible = visible;
        }

        /// <summary>
        /// Gets or sets the hash of the Perk.
        /// </summary>
        public uint PerkHash { get; set; }

        /// <summary>
        /// Gets or sets the Icon Path of the Perk.
        /// </summary>
        public string IconPath { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a Bool indicating whether this is an active perk.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a Bool indicating whether this perk is visible on the weapon.
        /// </summary>
        public bool IsVisible { get; set; }
    }
}