using System.Net.Sockets;

namespace GuardianServices.Models;

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
    /// <param name="perkDetails">PerkDetails for the Item.</param>
    /// <param name="plugDetails">PlugDetails for the Item.</param>
    /// <param name="socketDetails">SocketTypeDetails for the Item.</param>
    private DItem(Item bungieItem, ItemDetails details, PerkDetails perkDetails, PlugDetails plugDetails, SocketTypeDetails socketDetails)
    {
        bungieItem.CopyTo(this);
        this.Details = details;
        this.PerkDetails = perkDetails;
        this.PlugDetails = plugDetails;
        this.SocketDetails = socketDetails;
    }

    /// <summary>
    /// Gets or sets the Item Details for the DItem.
    /// </summary>
    public ItemDetails Details { get; set; }

    /// <summary>
    /// Gets or sets the Perks for the DItem.
    /// </summary>
    public PerkDetails PerkDetails { get; set; }

    /// <summary>
    /// Gets or sets the Plugs for the DItem.
    /// </summary>
    public PlugDetails PlugDetails { get; set; }

    /// <summary>
    /// Gets or sets the SocketTypeDetails for the DItem.
    /// </summary>
    public SocketTypeDetails SocketDetails { get; set; }

    /// <summary>
    /// Creates a DItem from the provided Item and Details object.
    /// </summary>
    /// <param name="item">Item to convert to DItem.</param>
    /// <param name="details">Details for the Item.</param>
    /// <param name="perkDetails">PerkDetails for the Item.</param>
    /// <param name="plugDetails">PlugDetails for the Item.</param>
    /// <param name="socketDetails">SocketTypeDetails for the Item.</param>
    /// <returns>The constructed DItem.</returns>
    public static DItem FromBungieItem(Item item, ItemDetails details, PerkDetails perkDetails, PlugDetails plugDetails, SocketTypeDetails socketDetails)
    {
        return new DItem(item, details, perkDetails, plugDetails, socketDetails);
    }
}