// <copyright file="TransferInfo.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace ContentManager.Models;

/// <summary>
/// Contains information about a queued transfer.
/// </summary>
internal class TransferInfo
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TransferInfo"/> class.
    /// </summary>
    /// <param name="item">Item to transfer</param>
    /// <param name="origin">Where the item is.</param>
    /// <param name="destination">Where the item should go.</param>
    public TransferInfo(Item item, string origin, string destination)
    {
        this.Item = item;
        this.Origin = origin;
        this.Destination = destination;
    }

    /// <summary>
    /// Gets or sets the item involved in the transfer.
    /// </summary>
    public Item Item { get; set; }

    /// <summary>
    /// Gets or sets the origin of the transfer.
    /// </summary>
    public string Origin { get; set; }

    /// <summary>
    /// Gets or sets the destination of the transfer.
    /// </summary>
    public string Destination { get; set; }
}
