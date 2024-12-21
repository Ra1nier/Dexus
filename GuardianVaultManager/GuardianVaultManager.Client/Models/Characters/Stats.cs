using System.Text.Json.Serialization;

namespace GuardianVaultManager.Client.Models.Characters;

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
