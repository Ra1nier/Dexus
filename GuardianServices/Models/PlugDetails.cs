namespace GuardianServices.Models;

using System.Collections.Generic;
using System.Text.Json.Serialization;

public class ItemSockets
{
    [JsonPropertyName("plugs")]
    public Dictionary<int, List<Plug>> Plugs { get; set; } = new();
}

public class Plug
{
    [JsonPropertyName("plugItemHash")]
    public uint PlugItemHash { get; set; }

    [JsonPropertyName("canInsert")]
    public bool CanInsert { get; set; }

    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; }

    [JsonPropertyName("insertFailIndexes")]
    public List<int>? InsertFailIndexes { get; set; }
}