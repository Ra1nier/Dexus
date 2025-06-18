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
    /// Gets or sets the Item Details for the DItem.
    /// </summary>
    public ItemDetails Details { get; set; }

    /// <summary>
    /// Gets or sets the perks for the item.
    /// </summary>
    public List<Perk>? Perks { get; set; }

    /// <summary>
    /// Gets or sets the sockets for the item.
    /// </summary>
    public DItemPlugs? Plugs { get; set; }

    /// <summary>
    /// Gets or sets the bucket.
    /// </summary>
    public string Bucket { get; set; }

    /// <summary>
    /// Gets or sets the identifier.
    /// </summary>
    public string Identifier { get; set; }

    /// <summary>
    /// Gets or sets the Power.
    /// </summary>
    public int Power { get; set; } = 0;

    /// <summary>
    /// Creates a DItem from the provided Item and Details object.
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
        [JsonIgnore]
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