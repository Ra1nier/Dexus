// <copyright file="ManifestDetails.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace RequestManager;

/// <summary>
/// Represents the details of a weapon item in the Destiny manifest.
/// </summary>
public class WeaponDetails
{
    /// <summary>
    /// Gets or sets the name of the weapon.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the description of the weapon.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Gets or sets the path to the weapon's icon.
    /// </summary>
    public string IconPath { get; set; }

    /// <summary>
    /// Gets or sets the item type of the weapon.
    /// </summary>
    public int ItemType { get; set; }

    /// <summary>
    /// Gets or sets the item subtype of the weapon.
    /// </summary>
    public int ItemSubType { get; set; }

    /// <summary>
    /// Gets or sets the class type of the weapon.
    /// </summary>
    public int ClassType { get; set; }

    /// <summary>
    /// Gets or sets the damage type of the weapon.
    /// </summary>
    public string DamageType { get; set; }

    /// <summary>
    /// Gets or sets the default damage type of the weapon.
    /// </summary>
    public string DefaultDamageType { get; set; }

    /// <summary>
    /// Gets or sets the stats of the weapon as a dictionary.
    /// </summary>
    public Dictionary<string, int> Stats { get; set; }

    /// <summary>
    /// Gets or sets the maximum stack size for the weapon.
    /// </summary>
    public int MaxStackSize { get; set; }

    /// <summary>
    /// Gets or sets the tier type of the weapon.
    /// </summary>
    public string TierType { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the weapon is transferrable.
    /// </summary>
    public bool IsTransferrable { get; set; }

    /// <summary>
    /// Gets or sets the flavor text associated with the weapon.
    /// </summary>
    public string FlavorText { get; set; }

    /// <summary>
    /// Gets or sets the hash of the weapon's lore.
    /// </summary>
    public string LoreHash { get; set; }

    /// <summary>
    /// Gets or sets the sockets associated with the weapon.
    /// </summary>
    public List<string> Sockets { get; set; }

    /// <summary>
    /// Gets or sets the perks associated with the weapon.
    /// </summary>
    public List<string> Perks { get; set; }

    /// <summary>
    /// Gets or sets the weapon's pattern hash.
    /// </summary>
    public string PatternHash { get; set; }

    /// <summary>
    /// Gets or sets the crafting requirements for the weapon.
    /// </summary>
    public string CraftingRequirements { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="WeaponDetails"/> class.
    /// </summary>
    public WeaponDetails()
    {
        this.Stats = new Dictionary<string, int>();
        this.Sockets = new List<string>();
        this.Perks = new List<string>();
    }
}