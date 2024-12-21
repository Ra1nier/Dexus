using System.Text.Json.Serialization;

namespace GuardianVaultManager.Client.Models.Characters;

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
