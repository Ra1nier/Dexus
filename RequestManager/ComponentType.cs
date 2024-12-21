// <copyright file="ComponentType.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace RequestManager;

/// <summary>
/// Contains a list to convert the requested component into the component number.
/// </summary>
public enum ComponentType
{
    /// <summary>
    /// No component.
    /// </summary>
    None = 0,

    /// <summary>
    /// Profiles component.
    /// </summary>
    Profiles = 100,

    /// <summary>
    /// Vendor receipts component.
    /// </summary>
    VendorReceipts = 101,

    /// <summary>
    /// Profile inventories component.
    /// </summary>
    ProfileInventories = 102,

    /// <summary>
    /// Profile currencies component.
    /// </summary>
    ProfileCurrencies = 103,

    /// <summary>
    /// Profile progression component.
    /// </summary>
    ProfileProgression = 104,

    /// <summary>
    /// Platform silver component.
    /// </summary>
    PlatformSilver = 105,

    /// <summary>
    /// Characters component.
    /// </summary>
    Characters = 200,

    /// <summary>
    /// Character inventories component.
    /// </summary>
    CharacterInventories = 201,

    /// <summary>
    /// Character progressions component.
    /// </summary>
    CharacterProgressions = 202,

    /// <summary>
    /// Character render data component.
    /// </summary>
    CharacterRenderData = 203,

    /// <summary>
    /// Character activities component.
    /// </summary>
    CharacterActivities = 204,

    /// <summary>
    /// Character equipment component.
    /// </summary>
    CharacterEquipment = 205,

    /// <summary>
    /// Character loadouts component.
    /// </summary>
    CharacterLoadouts = 206,

    /// <summary>
    /// Item instances component.
    /// </summary>
    ItemInstances = 300,

    /// <summary>
    /// Item objectives component.
    /// </summary>
    ItemObjectives = 301,

    /// <summary>
    /// Item perks component.
    /// </summary>
    ItemPerks = 302,

    /// <summary>
    /// Item render data component.
    /// </summary>
    ItemRenderData = 303,

    /// <summary>
    /// Item stats component.
    /// </summary>
    ItemStats = 304,

    /// <summary>
    /// Item sockets component.
    /// </summary>
    ItemSockets = 305,

    /// <summary>
    /// Item talent grids component.
    /// </summary>
    ItemTalentGrids = 306,

    /// <summary>
    /// Item common data component.
    /// </summary>
    ItemCommonData = 307,

    /// <summary>
    /// Item plug states component.
    /// </summary>
    ItemPlugStates = 308,

    /// <summary>
    /// Item plug objectives component.
    /// </summary>
    ItemPlugObjectives = 309,

    /// <summary>
    /// Item reusable plugs component.
    /// </summary>
    ItemReusablePlugs = 310,

    /// <summary>
    /// Vendors component.
    /// </summary>
    Vendors = 400,

    /// <summary>
    /// Vendor categories component.
    /// </summary>
    VendorCategories = 401,

    /// <summary>
    /// Vendor sales component.
    /// </summary>
    VendorSales = 402,

    /// <summary>
    /// Kiosks component.
    /// </summary>
    Kiosks = 500,

    /// <summary>
    /// Currency lookups component.
    /// </summary>
    CurrencyLookups = 600,

    /// <summary>
    /// Presentation nodes component.
    /// </summary>
    PresentationNodes = 700,

    /// <summary>
    /// Collectibles component.
    /// </summary>
    Collectibles = 800,

    /// <summary>
    /// Records component.
    /// </summary>
    Records = 900,

    /// <summary>
    /// Transitory component.
    /// </summary>
    Transitory = 1000,

    /// <summary>
    /// Metrics component.
    /// </summary>
    Metrics = 1100,

    /// <summary>
    /// String variables component.
    /// </summary>
    StringVariables = 1200,

    /// <summary>
    /// Craftables component.
    /// </summary>
    Craftables = 1300,

    /// <summary>
    /// Social commendations component.
    /// </summary>
    SocialCommendations = 1400,
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
