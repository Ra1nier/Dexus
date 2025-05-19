// <copyright file="Extensions.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace GuardianServices;

using GuardianServices.Models.ItemDetails;
using GuardianServices.Models;

/// <summary>
/// Provides Extensions to the List class.
/// </summary>
public static class ListExtensions
{
    /// <summary>
    /// Prints the contents of a List in the following format: "item1, item2, item3, ...".
    /// </summary>
    /// <typeparam name="T">Type used for the list.</typeparam>
    /// <param name="list">List to print.</param>
    /// <returns>A string representing the list.</returns>
    public static string Print<T>(this List<T> list)
    {
        string printedString = string.Empty;
        if (list.Count > 0)
        {
            foreach (var component in list)
            {
                printedString += component.ToString() + ", ";
            }
        }

        return printedString.TrimEnd(',', ' ');
    }

    /// <summary>
    /// Prints the contents of a List in the following CSV format: "item1,item2,item3, ...".
    /// </summary>
    /// <typeparam name="T">Type used for the list.</typeparam>
    /// <param name="list">List to print.</param>
    /// <returns>A string in CSV format representing the list.</returns>
    public static string PrintCSV<T>(this List<T> list)
    {
        string printedString = string.Empty;
        if (list.Count > 0)
        {
            foreach (var component in list)
            {
                printedString += component.ToString() + ",";
            }
        }

        return printedString.TrimEnd(',', ' ');
    }
}

/// <summary>
/// Defines a list of extensions for the Item class.
/// </summary>
public static class ItemExtensions
{
    /// <summary>
    /// Determines if the item associated with the itemInstanceID provided is a weapon.
    /// </summary>
    /// <param name="item">Item to check.</param>
    /// <returns>True if the item is a weapon. False if not or the item was not found.</returns>
    public static bool IsWeapon(this Item item)
    {
        bool isWeapon = false;
        string[] weaponDef = ["Kinetic Weapons", "Energy Weapons", "Power Weapons"];
        isWeapon = weaponDef.Contains(item.Bucket);

        return isWeapon || item.Perks.Any();
    }

    /// <summary>
    /// Determines if the item associated with the itemInstanceID provided is armor.
    /// </summary>
    /// <param name="item">Item to check.</param>
    /// <returns>True if the item is armor. False if not or the item was not found.</returns>
    public static bool IsArmor(this Item item)
    {
        string[] armorDef = ["Helmet", "Gauntlets", "Chest Armor", "Leg Armor", "Class Armor"];
        return armorDef.Contains(item.Bucket);
    }

    /// <summary>
    /// Determines if the item associated with the itemInstanceID provided is a Unit. Unit means the item is neither a Weapon nor Armor.
    /// </summary>
    /// <param name="item">Item to check.</param>
    /// <returns>True if the item is a Unit. False if not or the item was not found.</returns>
    public static bool IsUnit(this Item item)
    {
        return (!item.IsWeapon()) && (!item.IsArmor());
    }
}

/// <summary>
/// Defines a list of extensions for the ItemDetails class.
/// </summary>
public static class ItemDetailsExtensions
{
    /// <summary>
    /// Determines if the item associated with an itemHash is an exotic item.
    /// </summary>
    /// <param name="itemDetails">ItemDetails to check.</param>
    /// <returns>True if the item is Exotic. False if not or the item's details were not found.</returns>
    public static bool IsExotic(this ItemDetails itemDetails)
    {
        return itemDetails.Inventory.TierType == 6;
    }
}