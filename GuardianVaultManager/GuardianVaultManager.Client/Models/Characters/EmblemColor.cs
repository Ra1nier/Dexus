using System.Text.Json.Serialization;

namespace GuardianVaultManager.Client.Models.Characters;

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
