namespace GuardianVaultManager.Client.Models;

public class Item
{
    public uint ItemHash { get; set; }
    public string ItemInstanceId { get; set; }
    public int Quantity { get; set; }
    public int BindStatus { get; set; }
    public int Location { get; set; }
    public uint BucketHash { get; set; }
    public int TransferStatus { get; set; }
    public bool Lockable { get; set; }
    public int State { get; set; }
    public int DismantlePermission { get; set; }
    public bool IsWrapper { get; set; }
    public List<int> TooltipNotificationIndexes { get; set; }
    public int VersionNumber { get; set; }
}
