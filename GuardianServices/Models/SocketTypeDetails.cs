namespace GuardianServices.Models;

using System.Text.Json.Serialization;

public class SocketTypeDetails
{
    [JsonPropertyName("displayProperties")]
    public DisplayPropertiesData DisplayProperties { get; set; }

    [JsonPropertyName("insertAction")]
    public InsertActionData InsertAction { get; set; }

    [JsonPropertyName("plugWhitelist")]
    public List<PlugWhitelistEntry> PlugWhitelist { get; set; }

    [JsonPropertyName("socketCategoryHash")]
    public long SocketCategoryHash { get; set; }

    [JsonPropertyName("visibility")]
    public int Visibility { get; set; }

    [JsonPropertyName("alwaysRandomizeSockets")]
    public bool AlwaysRandomizeSockets { get; set; }

    [JsonPropertyName("isPreviewEnabled")]
    public bool IsPreviewEnabled { get; set; }

    [JsonPropertyName("hideDuplicateReusablePlugs")]
    public bool HideDuplicateReusablePlugs { get; set; }

    [JsonPropertyName("overridesUiAppearance")]
    public bool OverridesUiAppearance { get; set; }

    [JsonPropertyName("avoidDuplicatesOnInitialization")]
    public bool AvoidDuplicatesOnInitialization { get; set; }

    [JsonPropertyName("currencyScalars")]
    public List<object> CurrencyScalars { get; set; }

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

        [JsonPropertyName("hasIcon")]
        public bool HasIcon { get; set; }
    }

    public class InsertActionData
    {
        [JsonPropertyName("actionExecuteSeconds")]
        public int ActionExecuteSeconds { get; set; }

        [JsonPropertyName("actionSoundHash")]
        public int ActionSoundHash { get; set; }

        [JsonPropertyName("isPositiveAction")]
        public bool IsPositiveAction { get; set; }

        [JsonPropertyName("actionType")]
        public int ActionType { get; set; }
    }

    public class PlugWhitelistEntry
    {
        [JsonPropertyName("categoryHash")]
        public long CategoryHash { get; set; }

        [JsonPropertyName("categoryIdentifier")]
        public string CategoryIdentifier { get; set; }
    }
}