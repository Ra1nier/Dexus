namespace RequestManager.Models.ItemDetails;

using System.Text.Json.Serialization;
public class ItemDetails
{
    [JsonPropertyName("displayProperties")]
    public DisplayProperties DisplayProperties { get; set; }

    [JsonPropertyName("tooltipNotifications")]
    public List<object> TooltipNotifications { get; set; }

    [JsonPropertyName("collectibleHash")]
    public long CollectibleHash { get; set; }

    [JsonPropertyName("iconWatermark")]
    public string IconWatermark { get; set; }

    [JsonPropertyName("backgroundColor")]
    public ItemColor BackgroundColor { get; set; }

    [JsonPropertyName("screenshot")]
    public string Screenshot { get; set; }

    [JsonPropertyName("itemTypeDisplayName")]
    public string ItemTypeDisplayName { get; set; }

    [JsonPropertyName("flavorText")]
    public string FlavorText { get; set; }

    [JsonPropertyName("uiItemDisplayStyle")]
    public string UiItemDisplayStyle { get; set; }

    [JsonPropertyName("itemTypeAndTierDisplayName")]
    public string ItemTypeAndTierDisplayName { get; set; }

    [JsonPropertyName("displaySource")]
    public string DisplaySource { get; set; }

    [JsonPropertyName("action")]
    public ActionDetails Action { get; set; }

    [JsonPropertyName("inventory")]
    public Inventory Inventory { get; set; }

    [JsonPropertyName("stats")]
    public Stats Stats { get; set; }

    [JsonPropertyName("equippingBlock")]
    public EquippingBlock EquippingBlock { get; set; }

    [JsonPropertyName("translationBlock")]
    public TranslationBlock TranslationBlock { get; set; }

    [JsonPropertyName("preview")]
    public Preview Preview { get; set; }

    [JsonPropertyName("quality")]
    public Quality Quality { get; set; }

    [JsonPropertyName("sockets")]
    public Sockets Sockets { get; set; }

    [JsonPropertyName("investmentStats")]
    public List<InvestmentStat> InvestmentStats { get; set; }

    [JsonPropertyName("perks")]
    public List<Perk> Perks { get; set; }

    [JsonPropertyName("hash")]
    public long Hash { get; set; }

    [JsonPropertyName("index")]
    public int Index { get; set; }

    [JsonPropertyName("redacted")]
    public bool Redacted { get; set; }

    [JsonPropertyName("blacklisted")]
    public bool Blacklisted { get; set; }
}

public class DisplayProperties
{
    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("icon")]
    public string Icon { get; set; }

    [JsonPropertyName("hasIcon")]
    public bool HasIcon { get; set; }
}

public class ItemColor
{
    [JsonPropertyName("red")]
    public int Red { get; set; }

    [JsonPropertyName("green")]
    public int Green { get; set; }

    [JsonPropertyName("blue")]
    public int Blue { get; set; }

    [JsonPropertyName("alpha")]
    public int Alpha { get; set; }
}

public class ActionDetails
{
    [JsonPropertyName("verbName")]
    public string VerbName { get; set; }

    [JsonPropertyName("verbDescription")]
    public string VerbDescription { get; set; }

    [JsonPropertyName("isPositive")]
    public bool IsPositive { get; set; }

    [JsonPropertyName("requiredCooldownSeconds")]
    public int RequiredCooldownSeconds { get; set; }

    [JsonPropertyName("requiredItems")]
    public List<object> RequiredItems { get; set; }

    [JsonPropertyName("progressionRewards")]
    public List<object> ProgressionRewards { get; set; }

    [JsonPropertyName("actionTypeLabel")]
    public string ActionTypeLabel { get; set; }

    [JsonPropertyName("deleteOnAction")]
    public bool DeleteOnAction { get; set; }
}

public class Inventory
{
    [JsonPropertyName("maxStackSize")]
    public int MaxStackSize { get; set; }

    [JsonPropertyName("tierType")]
    public int TierType { get; set; }

    [JsonPropertyName("bucketTypeHash")]
    public uint BucketTypeHash { get; set; }
}

public class Stats
{
    [JsonPropertyName("disablePrimaryStatDisplay")]
    public bool DisablePrimaryStatDisplay { get; set; }

    [JsonPropertyName("stats")]
    public Dictionary<string, Stat> StatDetails { get; set; }
}

public class Stat
{
    [JsonPropertyName("statHash")]
    public long StatHash { get; set; }

    [JsonPropertyName("value")]
    public int Value { get; set; }
}

public class EquippingBlock
{
    [JsonPropertyName("uniqueLabel")]
    public string UniqueLabel { get; set; }
}

public class TranslationBlock
{
    [JsonPropertyName("lockedDyes")]
    public List<Dye> LockedDyes { get; set; }

    [JsonPropertyName("arrangements")]
    public List<Arrangement> Arrangements { get; set; }
}

public class Dye
{
    [JsonPropertyName("channelHash")]
    public long ChannelHash { get; set; }

    [JsonPropertyName("dyeHash")]
    public long DyeHash { get; set; }
}

public class Arrangement
{
    [JsonPropertyName("classHash")]
    public long ClassHash { get; set; }
}

public class Preview
{
    [JsonPropertyName("screenStyle")]
    public string ScreenStyle { get; set; }
}

public class Quality
{
    [JsonPropertyName("qualityLevel")]
    public int QualityLevel { get; set; }
}

public class Sockets
{
    [JsonPropertyName("socketEntries")]
    public List<SocketEntry> SocketEntries { get; set; }
}

public class SocketEntry
{
    [JsonPropertyName("socketTypeHash")]
    public long SocketTypeHash { get; set; }

    [JsonPropertyName("singleInitialItemHash")]
    public long SingleInitialItemHash { get; set; }

    [JsonPropertyName("reusablePlugItems")]
    public List<PlugItem> ReusablePlugItems { get; set; }
}

public class PlugItem
{
    [JsonPropertyName("plugItemHash")]
    public long PlugItemHash { get; set; }
}

public class InvestmentStat
{
    [JsonPropertyName("statTypeHash")]
    public long StatTypeHash { get; set; }

    [JsonPropertyName("value")]
    public int Value { get; set; }
}

public class Perk
{
    [JsonPropertyName("perkHash")]
    public long PerkHash { get; set; }
}
