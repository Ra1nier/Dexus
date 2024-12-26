// <copyright file="ContentManager.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace ContentManager;

using Models;
using RequestManager;

/// <summary>
/// The ContentManager class is responsible for managing the content of GVM.
/// </summary>
public class ContentManager
{
    // Client specific objects
    private List<Character> Characters = new List<Character>();

    // Sub Components for each character
    private List<Item> Weapons = new List<Item>();
    private List<Item> KineticWeapons = new List<Item>();
    private List<Item> EnergyWeapons = new List<Item>();
    private List<Item> PowerWeapons = new List<Item>();

    private List<Item> Armor = new List<Item>();
    private List<Item> Helmets = new List<Item>();
    private List<Item> Gauntlets = new List<Item>();
    private List<Item> Chests = new List<Item>();
    private List<Item> Legs = new List<Item>();
    private List<Item> Skirts = new List<Item>();

    private List<Item> Accessories = new List<Item>();
    private List<Item> Postmaster = new List<Item>();
    private List<Item> Engrams = new List<Item>();
    private List<Item> Quests = new List<Item>();

    // Non Specific Sub Components
    private List<Item> ClientComponents = new List<Item>();

    private List<Item> Consumables = new List<Item>();
    private List<Item> Extras = new List<Item>();

    // Complete Components
    private List<Item> DropzoneBackingStore = new List<Item>();
    private Dictionary<uint, ItemDetails> ItemDetails = new Dictionary<uint, ItemDetails>();
}
