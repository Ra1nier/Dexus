// <copyright file="ComponentType.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace RequestManager;

/// <summary>
/// Contains a list to convert the requested component into the component number.
/// </summary>
public enum ComponentType
{
    None = 0,
    Profiles = 100,
    VendorReceipts = 101,
    ProfileInventories = 102,
    ProfileCurrencies = 103,
    ProfileProgression = 104,
    PlatformSilver = 105,
    Characters = 200,
    CharacterInventories = 201,
    CharacterProgressions = 202,
    CharacterRenderData = 203,
    CharacterActivities = 204,
    CharacterEquipment = 205,
    CharacterLoadouts = 206,
    ItemInstances = 300,
    ItemObjectives = 301,
    ItemPerks = 302,
    ItemRenderData = 303,
    ItemStats = 304,
    ItemSockets = 305,
    ItemTalentGrids = 306,
    ItemCommonData = 307,
    ItemPlugStates = 308,
    ItemPlugObjectives = 309,
    ItemReusablePlugs = 310,
    Vendors = 400,
    VendorCategories = 401,
    VendorSales = 402,
    Kiosks = 500,
    CurrencyLookups = 600,
    PresentationNodes = 700,
    Collectibles = 800,
    Records = 900,
    Transitory = 1000,
    Metrics = 1100,
    StringVariables = 1200,
    Craftables = 1300,
    SocialCommendations = 1400
}

/// <summary>
/// Converts the requested component type into the component number.
/// </summary>
public static class ComponentTypeLookup
{
    // Dictionary for quick string lookup
    private static readonly Dictionary<string, ComponentType> lookup = new()
    {
        { "None", ComponentType.None },
        { "Profiles", ComponentType.Profiles },
        { "VendorReceipts", ComponentType.VendorReceipts },
        { "ProfileInventories", ComponentType.ProfileInventories },
        { "ProfileCurrencies", ComponentType.ProfileCurrencies },
        { "ProfileProgression", ComponentType.ProfileProgression },
        { "PlatformSilver", ComponentType.PlatformSilver },
        { "Characters", ComponentType.Characters },
        { "CharacterInventories", ComponentType.CharacterInventories },
        { "CharacterProgressions", ComponentType.CharacterProgressions },
        { "CharacterRenderData", ComponentType.CharacterRenderData },
        { "CharacterActivities", ComponentType.CharacterActivities },
        { "CharacterEquipment", ComponentType.CharacterEquipment },
        { "CharacterLoadouts", ComponentType.CharacterLoadouts },
        { "ItemInstances", ComponentType.ItemInstances },
        { "ItemObjectives", ComponentType.ItemObjectives },
        { "ItemPerks", ComponentType.ItemPerks },
        { "ItemRenderData", ComponentType.ItemRenderData },
        { "ItemStats", ComponentType.ItemStats },
        { "ItemSockets", ComponentType.ItemSockets },
        { "ItemTalentGrids", ComponentType.ItemTalentGrids },
        { "ItemCommonData", ComponentType.ItemCommonData },
        { "ItemPlugStates", ComponentType.ItemPlugStates },
        { "ItemPlugObjectives", ComponentType.ItemPlugObjectives },
        { "ItemReusablePlugs", ComponentType.ItemReusablePlugs },
        { "Vendors", ComponentType.Vendors },
        { "VendorCategories", ComponentType.VendorCategories },
        { "VendorSales", ComponentType.VendorSales },
        { "Kiosks", ComponentType.Kiosks },
        { "CurrencyLookups", ComponentType.CurrencyLookups },
        { "PresentationNodes", ComponentType.PresentationNodes },
        { "Collectibles", ComponentType.Collectibles },
        { "Records", ComponentType.Records },
        { "Transitory", ComponentType.Transitory },
        { "Metrics", ComponentType.Metrics },
        { "StringVariables", ComponentType.StringVariables },
        { "Craftables", ComponentType.Craftables },
        { "SocialCommendations", ComponentType.SocialCommendations },
    };

    /// <summary>
    /// Converts the requested component type into the component number.
    /// </summary>
    /// <param name="component">Requested component</param>
    /// <returns>The corresponding ComponentType, or null if not found.</returns>
    public static int GetContent(string component)
    {
        if (lookup.TryGetValue(component, out ComponentType value))
        {
            return (int)value;
        }

        return 0;
    }
}
