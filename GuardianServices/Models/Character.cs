// <copyright file="Character.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace GuardianServices.Models;

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
    public uint RaceHash { get; set; }

    /// <summary>
    /// Gets or sets the genderHash.
    /// </summary>
    [JsonPropertyName("GenderHash")]
    public uint GenderHash { get; set; }

    /// <summary>
    /// Gets or sets the ClassHash.
    /// </summary>
    [JsonPropertyName("classHash")]
    public uint ClassHash { get; set; }

    /// <summary>
    /// Gets or sets the Class.
    /// </summary>
    [JsonIgnore]
    public string? Class { get; set; }

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

/// <summary>
/// Represents an emblem color object.
/// </summary>
public class EmblemColor
{
    /// <summary>
    /// Gets or sets the Red.
    /// </summary>
    [JsonPropertyName("red")]
    public int? Red { get; set; }

    /// <summary>
    /// Gets or sets the Green.
    /// </summary>
    [JsonPropertyName("green")]
    public int? Green { get; set; }

    /// <summary>
    /// Gets or sets the Blue.
    /// </summary>
    [JsonPropertyName("blue")]
    public int? Blue { get; set; }

    /// <summary>
    /// Gets or sets the Alpha.
    /// </summary>
    [JsonPropertyName("alpha")]
    public int? Alpha { get; set; }
}

/// <summary>
/// Represents the level progression of a character.
/// </summary>
public class LevelProgression
{
    /// <summary>
    /// Gets or sets the ProgressionHash.
    /// </summary>
    [JsonPropertyName("progressionHash")]
    public int? ProgressionHash { get; set; }

    /// <summary>
    /// Gets or sets the DailyProgress.
    /// </summary>
    [JsonPropertyName("dailyProgress")]
    public int? DailyProgress { get; set; }

    /// <summary>
    /// Gets or sets the DailyLimit.
    /// </summary>
    [JsonPropertyName("DailyLimit")]
    public int? DailyLimit { get; set; }

    /// <summary>
    /// Gets or sets the WeeklyProgress.
    /// </summary>
    [JsonPropertyName("weeklyProgress")]
    public int? WeeklyProgress { get; set; }

    /// <summary>
    /// Gets or sets the WeeklyLimit.
    /// </summary>
    [JsonPropertyName("weeklyLimit")]
    public int? WeeklyLimit { get; set; }

    /// <summary>
    /// Gets or sets the CurrentProgress.
    /// </summary>
    [JsonPropertyName("currentProgress")]
    public int? CurrentProgress { get; set; }

    /// <summary>
    /// Gets or sets the Level.
    /// </summary>
    [JsonPropertyName("level")]
    public int? Level { get; set; }

    /// <summary>
    /// Gets or sets the LevelCap.
    /// </summary>
    [JsonPropertyName("levelCap")]
    public int? LevelCap { get; set; }

    /// <summary>
    /// Gets or sets the StepIndex.
    /// </summary>
    [JsonPropertyName("stepIndex")]
    public int? StepIndex { get; set; }

    /// <summary>
    /// Gets or sets the ProgressToNextLevel.
    /// </summary>
    [JsonPropertyName("progressToNextLevel")]
    public int? ProgressToNextLevel { get; set; }

    /// <summary>
    /// Gets or sets the NextLevelAt.
    /// </summary>
    [JsonPropertyName("nextLevelAt")]
    public int? NextLevelAt { get; set; }
}

/// <summary>
/// Represents a characters stats distributions.
/// </summary>
public class Stats
{
    /// <summary>
    /// Gets or sets the Light.
    /// </summary>
    [JsonPropertyName("1935470627")]
    public int? Light { get; set; }

    /// <summary>
    /// Gets or sets the Mobility.
    /// </summary>
    [JsonPropertyName("2996146975")]
    public int? Mobility { get; set; }

    /// <summary>
    /// Gets or sets the Resilience.
    /// </summary>
    [JsonPropertyName("392767087")]
    public int? Resilience { get; set; }

    /// <summary>
    /// Gets or sets the Recovery.
    /// </summary>
    [JsonPropertyName("1943323491")]
    public int? Recovery { get; set; }

    /// <summary>
    /// Gets or sets the Discipline.
    /// </summary>
    [JsonPropertyName("1735777505")]
    public int? Discipline { get; set; }

    /// <summary>
    /// Gets or sets the Intellect.
    /// </summary>
    [JsonPropertyName("144602215")]
    public int? Intellect { get; set; }

    /// <summary>
    /// Gets or sets the Strength.
    /// </summary>
    [JsonPropertyName("4244567218")]
    public int? Strength { get; set; }
}