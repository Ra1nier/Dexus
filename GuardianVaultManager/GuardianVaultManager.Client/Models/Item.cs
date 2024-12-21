namespace GuardianVaultManager.Client.Models;

using System.Text.Json.Serialization;

public class Item
{
    [JsonPropertyName("itemHash")]
    public long ItemHash { get; set; }

    [JsonPropertyName("itemInstanceId")]
    public string? ItemInstanceId { get; set; }

    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }

    [JsonPropertyName("bindStatus")]
    public int BindStatus { get; set; }

    [JsonPropertyName("location")]
    public int Location { get; set; }

    [JsonPropertyName("bucketHash")]
    public long BucketHash { get; set; }

    [JsonPropertyName("transferStatus")]
    public int TransferStatus { get; set; }

    [JsonPropertyName("lockable")]
    public bool Lockable { get; set; }

    [JsonPropertyName("state")]
    public int State { get; set; }

    [JsonPropertyName("dismantlePermission")]
    public int DismantlePermission { get; set; }

    [JsonPropertyName("isWrapper")]
    public bool IsWrapper { get; set; }

    [JsonPropertyName("tooltipNotificationIndexes")]
    public List<int> TooltipNotificationIndexes { get; set; }

    [JsonPropertyName("versionNumber")]
    public int? VersionNumber { get; set; }

    [JsonIgnore]
    public string? Identifier { get; set; }
}
