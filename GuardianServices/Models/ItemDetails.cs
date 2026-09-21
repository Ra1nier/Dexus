namespace GuardianServices.Models;

using System.Text.Json.Serialization;

public class ItemDetails
{
    [JsonPropertyName("displayProperties")]
    public DisplayPropertiesData DisplayProperties { get; set; }

    [JsonPropertyName("tooltipNotifications")]
    public List<object> TooltipNotifications { get; set; }

    [JsonPropertyName("collectibleHash")]
    public long CollectibleHash { get; set; }

    [JsonPropertyName("iconWatermark")]
    public string IconWatermark { get; set; }

    [JsonPropertyName("backgroundColor")]
    public ItemColorData BackgroundColor { get; set; }

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
    public ActionDetailsData Action { get; set; }

    [JsonPropertyName("inventory")]
    public InventoryData Inventory { get; set; }

    [JsonPropertyName("stats")]
    public StatsData Stats { get; set; }

    [JsonPropertyName("equippingBlock")]
    public EquippingBlockData EquippingBlock { get; set; }

    [JsonPropertyName("translationBlock")]
    public TranslationBlockData TranslationBlock { get; set; }

    [JsonPropertyName("preview")]
    public PreviewData Preview { get; set; }

    [JsonPropertyName("quality")]
    public QualityData Quality { get; set; }

    [JsonPropertyName("sockets")]
    public SocketsData Sockets { get; set; }

    [JsonPropertyName("investmentStats")]
    public List<InvestmentStatData> InvestmentStats { get; set; }

    [JsonPropertyName("perks")]
    public List<PerkData> Perks { get; set; }

    [JsonPropertyName("hash")]
    public long Hash { get; set; }

    [JsonPropertyName("index")]
    public int Index { get; set; }

    [JsonPropertyName("redacted")]
    public bool Redacted { get; set; }

    [JsonPropertyName("blacklisted")]
    public bool Blacklisted { get; set; }

    public class DisplayPropertiesData
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

    public class ItemColorData
    {
        [JsonPropertyName("red")] public int Red { get; set; }
        [JsonPropertyName("green")] public int Green { get; set; }
        [JsonPropertyName("blue")] public int Blue { get; set; }
        [JsonPropertyName("alpha")] public int Alpha { get; set; }
    }

    public class ActionDetailsData
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

    public class InventoryData
    {
        [JsonPropertyName("maxStackSize")]
        public int MaxStackSize { get; set; }

        [JsonPropertyName("tierType")]
        public int TierType { get; set; }

        [JsonPropertyName("bucketTypeHash")]
        public uint BucketTypeHash { get; set; }
    }

    public class StatsData
    {
        [JsonPropertyName("disablePrimaryStatDisplay")]
        public bool DisablePrimaryStatDisplay { get; set; }

        [JsonPropertyName("stats")]
        public Dictionary<string, StatData> StatDetails { get; set; }
    }

    public class StatData
    {
        [JsonPropertyName("statHash")] public long StatHash { get; set; }
        [JsonPropertyName("value")] public int Value { get; set; }
    }

    public class EquippingBlockData
    {
        [JsonPropertyName("uniqueLabel")]
        public string UniqueLabel { get; set; }
    }

    public class TranslationBlockData
    {
        [JsonPropertyName("lockedDyes")]
        public List<DyeData> LockedDyes { get; set; }

        [JsonPropertyName("arrangements")]
        public List<ArrangementData> Arrangements { get; set; }
    }

    public class DyeData
    {
        [JsonPropertyName("channelHash")]
        public long ChannelHash { get; set; }

        [JsonPropertyName("dyeHash")]
        public long DyeHash { get; set; }
    }

    public class ArrangementData
    {
        [JsonPropertyName("classHash")]
        public long ClassHash { get; set; }
    }

    public class PreviewData
    {
        [JsonPropertyName("screenStyle")]
        public string ScreenStyle { get; set; }
    }

    public class QualityData
    {
        [JsonPropertyName("qualityLevel")]
        public int QualityLevel { get; set; }
    }

    public class SocketsData
    {
        [JsonPropertyName("socketEntries")]
        public List<SocketEntryData> SocketEntries { get; set; }
    }

    public class SocketEntryData
    {
        [JsonPropertyName("socketTypeHash")]
        public long SocketTypeHash { get; set; }

        [JsonPropertyName("singleInitialItemHash")]
        public long SingleInitialItemHash { get; set; }

        [JsonPropertyName("reusablePlugItems")]
        public List<PlugItemData> ReusablePlugItems { get; set; }
    }

    public class PlugItemData
    {
        [JsonPropertyName("plugItemHash")]
        public long PlugItemHash { get; set; }
    }

    public class InvestmentStatData
    {
        [JsonPropertyName("statTypeHash")]
        public long StatTypeHash { get; set; }

        [JsonPropertyName("value")]
        public int Value { get; set; }
    }

    public class PerkData
    {
        [JsonPropertyName("perkHash")]
        public long PerkHash { get; set; }
    }
}
