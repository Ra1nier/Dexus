// <copyright file="Character.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace GuardianVaultManager.Client.Models.Characters;

using System.Text.Json.Serialization;

/// <summary>
/// Represents a Destiny 2 character.
/// </summary>
public class Character
{
    /// <summary>
    /// Gets or sets the MembershipID.
    /// </summary>
    [JsonPropertyName("membershipID")]
    public string? MembershipID { get; set; }

    /// <summary>
    /// Gets or sets the MembershipType.
    /// </summary>
    [JsonPropertyName("membershipType")]
    public int? MembershipType { get; set; }

    /// <summary>
    /// Gets or sets the CharacterId.
    /// </summary>
    [JsonPropertyName("characterId")]
    public string? CharacterId { get; set; }

    /// <summary>
    /// Gets or sets the DateLastPlayed.
    /// </summary>
    [JsonPropertyName("dateLastPlayed")]
    public string? DateLastPlayed { get; set; }

    /// <summary>
    /// Gets or sets the MinutesPlayedThisSession.
    /// </summary>
    [JsonPropertyName("minutesPlayedThisSession")]
    public string? MinutesPlayedThisSession { get; set; }

    /// <summary>
    /// Gets or sets the MinutesPlayedTotal.
    /// </summary>
    [JsonPropertyName("minutesPlayedTotal")]
    public string? MinutesPlayedTotal { get; set; }

    /// <summary>
    /// Gets or sets the Light.
    /// </summary>
    [JsonPropertyName("light")]
    public int? Light { get; set; }

    /// <summary>
    /// Gets or sets the Stats.
    /// </summary>
    [JsonPropertyName("stats")]
    public Stats? Stats { get; set; }

    /// <summary>
    /// Gets or sets the RaceHash.
    /// </summary>
    [JsonPropertyName("raceHash")]
    public long? RaceHash { get; set; }

    /// <summary>
    /// Gets or sets the genderHash.
    /// </summary>
    [JsonPropertyName("GenderHash")]
    public long? GenderHash { get; set; }

    /// <summary>
    /// Gets or sets the ClassHash.
    /// </summary>
    [JsonPropertyName("classHash")]
    public long? ClassHash { get; set; }

    /// <summary>
    /// Gets or sets the RaceType.
    /// </summary>
    [JsonPropertyName("raceType")]
    public int? RaceType { get; set; }

    /// <summary>
    /// Gets or sets the ClassType.
    /// </summary>
    [JsonPropertyName("classType")]
    public int? ClassType { get; set; }

    /// <summary>
    /// Gets or sets the GenderType.
    /// </summary>
    [JsonPropertyName("genderType")]
    public int? GenderType { get; set; }

    /// <summary>
    /// Gets or sets the EmblemPath.
    /// </summary>
    [JsonPropertyName("emblemPath")]
    public string? EmblemPath { get; set; }

    /// <summary>
    /// Gets or sets the EmblemBackgroundPath.
    /// </summary>
    [JsonPropertyName("emblemBackgroundPath")]
    public string? EmblemBackgroundPath { get; set; }

    /// <summary>
    /// Gets or sets the EmblemHash.
    /// </summary>
    [JsonPropertyName("emblemHash")]
    public long? EmblemHash { get; set; }

    /// <summary>
    /// Gets or sets the EmblemColor.
    /// </summary>
    [JsonPropertyName("emblemColor")]
    public EmblemColor? EmblemColor { get; set; }

    /// <summary>
    /// Gets or sets the LevelProgression.
    /// </summary>
    [JsonPropertyName("levelProgression")]
    public LevelProgression? LevelProgression { get; set; }

    /// <summary>
    /// Gets or sets the BaseCharacterLevel.
    /// </summary>
    [JsonPropertyName("baseCharacterLevel")]
    public int? BaseCharacterLevel { get; set; }

    /// <summary>
    /// Gets or sets the PercentToNextLevel.
    /// </summary>
    [JsonPropertyName("percentToNextLevel")]
    public double? PercentToNextLevel { get; set; }

    /// <summary>
    /// Gets or sets the TitleRecordHash.
    /// </summary>
    [JsonPropertyName("titleRecordHash")]
    public int? TitleRecordHash { get; set; }
}