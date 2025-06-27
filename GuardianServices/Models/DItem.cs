using System.Net.Sockets;

namespace GuardianServices.Models;

using System.Text.Json.Serialization;

/// <summary>
/// Dexus Item (DItem) combines the various components, system, details of an Item into one easy-to-access Model.
/// </summary>
public class DItem : Item
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DItem"/> class.
    /// Converts a Bungie Item into a DItem.
    /// </summary>
    /// <param name="bungieItem">Bungie item to convert into a DItem.</param>
    /// <param name="details">ItemDetails for the item.</param>
    /// <param name="bucket">Bucket that the item belongs to.</param>
    /// <param name="identifier">Location identifier for the item.</param>
    private DItem(Item bungieItem, ItemDetails details, string bucket, string identifier)
    {
        bungieItem.CopyTo(this);
        this.Details = details;
        this.Bucket = bucket;
        this.Identifier = identifier;
    }

    /// <summary>
    /// Gets or sets the Item Details for the DItem. This structure holds all the details retrieved from the manifest for the item. Details include names, descriptions, images, and more.
    /// </summary>
    public ItemDetails Details { get; set; }

    // TODO: This set only exists for weapons pre Forsaken and is now deprecated, Find a way to determine if needed for the DItem and request it separately when creating a DItem.
    /// <summary>
    /// Gets or sets the perks for the item. This list contains the Perks for the item retrieved from the API. Only items with APIVersion = 1 will contain this structure.
    /// </summary>
    public List<Perk>? Perks { get; set; }

    /// <summary>
    /// Gets or sets the plugs for the item. This structure holds and organises the Reusable Plugs for the item retrieved from the API.
    /// </summary>
    public DItemPlugs? Plugs { get; set; }

    /// <summary>
    /// Gets or sets the sockets for an item. This structure holds and organises the sockets for the item retrieved from the API.
    /// </summary>
    public DItemSockets? Sockets { get; set; }

    /// <summary>
    /// Gets or sets the bucket. The bucket tells Dexus what grouping (Kinetic, Energy, Power, etc.) the item is a part of.
    /// </summary>
    public string Bucket { get; set; }

    /// <summary>
    /// Gets or sets the identifier. The Identifier tells Dexus where the item is located when it is retrieved from the API.
    /// </summary>
    public string Identifier { get; set; }

    /// <summary>
    /// Gets or sets the Power. Power is only used for weapons, or armor to determine the strength of the item.
    /// </summary>
    public int Power { get; set; } = 0;

    /// <summary>
    /// Gets or sets the API version of the DItem.
    /// 1 - Pre-Forsaken: This item follows the Perks standard.
    /// 2 - Final Shape: This item follows the Slots and Reusable Socket model.
    /// 3 - Nine: This item follows a new standard for tiers of 1 to 5.
    /// </summary>
    public int APIVersion { get; set; } = 0;

    /// <summary>
    /// Creates a DItem from the provided Item and Details object. This will also assign the items bucket and identifier for future contexts.
    /// </summary>
    /// <param name="item">Item to convert to DItem.</param>
    /// <param name="details">Details for the Item.</param>
    /// <param name="bucket">Bucket that the item belongs to.</param>
    /// <param name="identifier">Location identifier for the item.</param>
    /// <returns>The constructed DItem.</returns>
    public static DItem FromBungieItem(Item item, ItemDetails details, string bucket, string identifier)
    {
        return new DItem(item, details, bucket, identifier);
    }
}

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
    /// <param name="details">Details for the perk.</param>
    public Perk(uint perkHash, string iconPath, bool isActive, bool visible, ItemDetails details)
    {
        this.PerkHash = perkHash;
        this.IconPath = iconPath;
        this.IsActive = isActive;
        this.IsVisible = visible;
        this.Details = details;
    }

    /// <summary>
    /// Gets or sets the hash of the Perk.
    /// </summary>
    public uint PerkHash { get; set; }

    /// <summary>
    /// Gets or sets the PerkDetails for the Perk.
    /// </summary>
    public ItemDetails Details { get; set; }

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

/// <summary>
/// Represents an Item's reusable plugs.
/// </summary>
public class DItemPlugs
{
    /// <summary>
    /// Gets or sets a list of plugs associated with their socket number. (Review description could be incorrect.)
    /// </summary>
    [JsonPropertyName("plugs")]
    public Dictionary<int, List<Plug>> ReusablePlugs { get; set; }

    /// <summary>
    /// Represents a Plug for an Item.
    /// </summary>
    public class Plug
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Plug"/> class.
        /// </summary>
        public Plug() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Plug"/> class.
        /// </summary>
        /// <param name="plugHash">PlugItemHash for the plug.</param>
        /// <param name="canInsert">Whether the plug can be inserted.</param>
        /// <param name="enabled">Whether the plug is enabled.</param>
        public Plug(uint plugHash, bool canInsert, bool enabled)
        {
            this.PlugItemHash = plugHash;
            this.CanInsert = canInsert;
            this.Enabled = enabled;
            this.InsertFailIndexes = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Plug"/> class.
        /// </summary>
        /// <param name="plugHash">PlugItemHash for the plug.</param>
        /// <param name="canInsert">Whether the plug can be inserted.</param>
        /// <param name="enabled">Whether the plug is enabled.</param>
        /// <param name="insertFailIndexes">TODO: I have no clue. Need to figure out.</param>
        public Plug(uint plugHash, bool canInsert, bool enabled, List<int> insertFailIndexes)
        {
            this.PlugItemHash = plugHash;
            this.CanInsert = canInsert;
            this.Enabled = enabled;
            this.InsertFailIndexes = insertFailIndexes;
        }

        /// <summary>
        /// Gets or sets the plugItemHash.
        /// </summary>
        [JsonPropertyName("plugItemHash")]
        public uint PlugItemHash { get; set; }

        /// <summary>
        /// Gets or sets the Details for the Plug.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public ItemDetails Details { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the plug can be inserted.
        /// </summary>
        [JsonPropertyName("canInsert")]
        public bool CanInsert { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the plug is enabled.
        /// </summary>
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets a list of ints indicating the InsertFailIndexes.
        /// </summary>
        [JsonPropertyName("insertFailIndexes")]
        public List<int>? InsertFailIndexes { get; set; }
    }
}

/// <summary>
/// Represents an items sockets
/// </summary>
public class DItemSockets
{
    /// <summary>
    /// Gets or sets a dictionary that contains the socket data from bungie about an item.
    /// </summary>
    [JsonPropertyName("sockets")]
    public Dictionary<uint, Socket> Data { get; set; }

    /// <summary>
    /// Represents socket information.
    /// </summary>
    public class Socket
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Socket"/> class.
        /// </summary>
        /// <param name="isEnabled">Indicates whether this socket is enabled.</param>
        /// <param name="isVisible">Indicates whether this socket is visible.</param>
        /// <param name="details">ItemDetails for the socket.</param>
        public Socket(bool isEnabled, bool isVisible, ItemDetails details)
        {
            this.IsEnabled = isEnabled;
            this.IsVisible = isVisible;
            this.Details = details;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the socket is enabled.
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the socket is visible.
        /// </summary>
        public bool IsVisible { get; set; }

        /// <summary>
        /// Gets or sets the details relating to the socket.
        /// </summary>
        public ItemDetails Details { get; set; }
    }
}

// ----- DItem Extension Classes -----

/// <summary>
/// Defines a list of extensions for the DItem class.
/// </summary>
public static class DItemExtensions
{
    /// <summary>
    /// Determines if the item's associated bucket is a weapon.
    /// </summary>
    /// <param name="item">Item the function is called on.</param>
    /// <returns>True if the item is a weapon. False if not or the item was not found.</returns>
    public static bool IsWeapon(this DItem item)
    {
        bool isWeapon = false;
        string[] weaponDef = ["Kinetic Weapons", "Energy Weapons", "Power Weapons"];
        isWeapon = weaponDef.Contains(item.Bucket);

        return isWeapon || (item.Perks != null && item.Perks.Any());
    }

    /// <summary>
    /// Determines if the item's associated bucket is armor.
    /// </summary>
    /// <param name="item">Item the function is called on.</param>
    /// <returns>True if the item is armor. False if not or the item was not found.</returns>
    public static bool IsArmor(this DItem item)
    {
        string[] armorDef = ["Helmet", "Gauntlets", "Chest Armor", "Leg Armor", "Class Armor"];
        return armorDef.Contains(item.Bucket);
    }

    /// <summary>
    /// Determines if the item's associated bucket is a Unit. Unit means the item is neither a Weapon nor Armor.
    /// </summary>
    /// <param name="item">Item the function is called on.</param>
    /// <returns>True if the item is a Unit. False if not or the item was not found.</returns>
    public static bool IsUnit(this DItem item)
    {
        return (!item.IsWeapon()) && (!item.IsArmor());
    }

    /// <summary>
    /// Checks is the item is an exotic item.
    /// </summary>
    /// <param name="item">Item the function is called on.</param>
    /// <returns>True if the item is exotic, false otherwise.</returns>
    public static bool IsExotic(this DItem item)
    {
        return item.Details.Inventory.TierType == 6;
    }

    /// <summary>
    /// Not functional.
    /// </summary>
    /// <param name="item">Item the function is called on.</param>
    /// <returns>True if the item is masterworked, False otherwise.</returns>
    public static bool IsMasterworked(this DItem item)
    {
        if (item.Plugs?.ReusablePlugs == null || !item.IsWeapon())
        {
            return false;
        }

        foreach (var plugs in item.Plugs.ReusablePlugs.Values)
        {
            foreach (var plug in plugs)
            {
                // TODO: Figure out how to tell if a weapon is masterworked from its details
                return false;
            }
        }

        return false;
    }

    /// <summary>
    /// Determines if the item is crafted by looking for the ability to level the weapon.
    /// </summary>
    /// <param name="item">Item the function is called on.</param>
    /// <returns>True if the item is crafted, False otherwise.</returns>
    public static bool IsCrafted(this DItem item)
    {
        if (item.Plugs?.ReusablePlugs == null || !item.IsWeapon())
        {
            return false;
        }

        bool isCrafted = true;
        bool canBeCrafted = false;
        foreach (var plugs in item.Plugs.ReusablePlugs.Values)
        {
            foreach (var plug in plugs)
            {
                if (plug.Details.DisplayProperties.Name.Contains("Tier 1 Enhancement"))
                {
                    isCrafted = false;
                }

                if (plug.Details.DisplayProperties.Name.Contains("Weapon Level"))
                {
                    canBeCrafted = true;
                }
            }
        }

        return isCrafted && canBeCrafted;
    }
}

/// <summary>
/// Defines extensions for the plug class.
/// </summary>
public static class DItemPlugsExtensions
{
    /// <summary>
    /// Determines if a plug is enhanced.
    /// </summary>
    /// <param name="plug">Plug this function is called on.</param>
    /// <returns>True if the plug is enhanced, False otherwise.</returns>
    public static bool IsEnhanced(this DItemPlugs.Plug plug)
    {
        return plug.Details.ItemTypeDisplayName.Contains("Enhanced");
    }

    /// <summary>
    /// Determines if a plug is a main plug. Main plugs are defined as plugs that are crucial to the weapon or what the user usually wants to know about a weapon. This does not include origin traits
    /// This consists of plugs containing either one of these words: "Trait", "Barrel", "Magazine", "Sight" ,"Battery", "Guard", "Blade", "Arrow", "Bowstring", "Haft".
    /// </summary>
    /// <param name="plug">Plug this function is called on.</param>
    /// <returns>True if the plug is a main plug, False otherwise.</returns>
    public static bool IsMainPlug(this DItemPlugs.Plug plug)
    {
        string[] mainPlugTypes = [ "Trait", "Barrel", "Magazine", "Sight", "Battery", "Guard", "Blade", "Arrow", "Bowstring", "Haft" ];

        foreach (string plugType in mainPlugTypes)
        {
            if (plug.Details.ItemTypeDisplayName.Contains(plugType) && !plug.IsOriginTrait())
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Determines if a plug is an origin trait.
    /// </summary>
    /// <param name="plug">Plug this function is called on.</param>
    /// <returns>True if the plug is an origin trait, False otherwise.</returns>
    public static bool IsOriginTrait(this DItemPlugs.Plug plug)
    {
        return plug.Details.ItemTypeDisplayName.Contains("Origin");
    }
}

// ----- Item Conversion Helper Classes -----

/// <summary>
/// Represents the socket structure Bungie uses in order to help convert it to a Dexus Socket structure. Just used for JSON Serialization.
/// </summary>
public class BungieSocket
{
    /// <summary>
    /// Gets or sets the plugHash for the socket.
    /// </summary>
    [JsonPropertyName("plugHash")]
    public uint PlugHash { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the socket is enabled.
    /// </summary>
    [JsonPropertyName("isEnabled")]
    public bool IsEnabled { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the socket is visible.
    /// </summary>
    [JsonPropertyName("isVisible")]
    public bool IsVisible { get; set; }
}