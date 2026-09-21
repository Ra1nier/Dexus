namespace GuardianServices.Models;

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

public class PerkDetails
{
    [JsonPropertyName("displayProperties")]
    public DisplayPropertiesData DisplayProperties { get; set; }

    [JsonPropertyName("iconSequence")]
    public IconSequenceData IconSequence { get; set; }

    [JsonPropertyName("destinyPerk")]
    public DestinyPerkData DestinyPerk { get; set; }

    public class DisplayPropertiesData
    {
        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("icon")]
        public string Icon { get; set; }

        [JsonPropertyName("iconSequences")]
        public List<IconSequenceData> IconSequences { get; set; }

        [JsonPropertyName("hasIcon")]
        public bool HasIcon { get; set; }

        public class IconSequenceData
        {
            [JsonPropertyName("frames")]
            public List<string> Frames { get; set; }
        }
    }

    public class IconSequenceData
    {
        [JsonPropertyName("frames")]
        public List<string> Frames { get; set; }
    }

    public class DestinyPerkData
    {
        [JsonPropertyName("displayProperties")]
        public DisplayPropertiesData DisplayProperties { get; set; }

        [JsonPropertyName("isDisplayable")]
        public bool IsDisplayable { get; set; }

        [JsonPropertyName("damageType")]
        public int DamageType { get; set; }

        [JsonPropertyName("hash")]
        public long Hash { get; set; }

        [JsonPropertyName("index")]
        public int Index { get; set; }

        [JsonPropertyName("redacted")]
        public bool Redacted { get; set; }

        [JsonPropertyName("blacklisted")]
        public bool Blacklisted { get; set; }
    }
}