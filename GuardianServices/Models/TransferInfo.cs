// <copyright file="TransferInfo.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace GuardianServices.Models;

/// <summary>
/// Contains information about a queued transfer.
/// </summary>
public class TransferInfo
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TransferInfo"/> class.
    /// </summary>
    /// <param name="type">Type of transfer to invoke.</param>
    /// <param name="item">Item to transfer.</param>
    /// <param name="origin">Where the item is.</param>
    /// <param name="destination">Where the item should go.</param>
    public TransferInfo(TransferType type, DItem item, string origin, string destination)
    {
        this.Type = type;
        this.Item = item;
        this.Origin = origin;
        this.Destination = destination;
    }

    /// <summary>
    /// Gets or sets the type of transfer requested.
    /// </summary>
    public TransferType Type { get; set; }

    /// <summary>
    /// Gets or sets the item involved in the transfer.
    /// </summary>
    public DItem Item { get; set; }

    /// <summary>
    /// Gets or sets the origin of the transfer.
    /// </summary>
    public string Origin { get; set; }

    /// <summary>
    /// Gets or sets the destination of the transfer.
    /// </summary>
    public string Destination { get; set; }
}

/// <summary>
/// Enum that indicated what type of transfer should be invoked by Content Manager.
/// </summary>
public enum TransferType
{
    /// <summary>
    /// Push indicates that Content Manager should invoke a CTC or CTV or VTC transfer.
    /// </summary>
    Push,

    /// <summary>
    /// Pull indicates that Content Manager should invoke a Postmaster transfer.
    /// </summary>
    Pull,
}
