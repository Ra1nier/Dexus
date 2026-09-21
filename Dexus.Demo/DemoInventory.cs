namespace Dexus.Demo;

public sealed record DemoGuardian(string Id, string ClassName, string Accent, string Initial);

public sealed class DemoItem
{
    public required string Id { get; init; }

    public required string Name { get; init; }

    public required string Category { get; init; }

    public required string TypeName { get; init; }

    public required string Rarity { get; init; }

    public required string Description { get; init; }

    public required string Accent { get; init; }

    public required string Glyph { get; init; }

    public required int Power { get; init; }

    public required string Location { get; set; }
}

public sealed class DemoInventory
{
    private static readonly HashSet<string> AllowedCategories =
    [
        "Kinetic Weapons",
        "Energy Weapons",
        "Power Weapons",
        "Helmets",
        "Chest Armor",
    ];

    public DemoInventory()
    {
        Reset();
    }

    public IReadOnlyList<DemoGuardian> Guardians { get; } =
    [
        new("hunter", "Hunter", "#65c8ff", "H"),
        new("warlock", "Warlock", "#ffd166", "W"),
        new("titan", "Titan", "#ff6b7a", "T"),
    ];

    public IReadOnlyList<string> Categories { get; } =
    [
        "Kinetic Weapons",
        "Energy Weapons",
        "Power Weapons",
        "Helmets",
        "Chest Armor",
    ];

    public List<DemoItem> Items { get; private set; } = [];

    public int Revision { get; private set; }

    public bool Move(DemoItem item, string destination)
    {
        if (!Items.Contains(item) || !IsValidDestination(destination))
        {
            return false;
        }

        if (destination.EndsWith("-equipped", StringComparison.Ordinal))
        {
            string guardian = destination[..destination.IndexOf("-equipped", StringComparison.Ordinal)];
            DemoItem? previouslyEquipped = Items.FirstOrDefault(candidate =>
                candidate != item
                && candidate.Category == item.Category
                && candidate.Location == destination);

            if (previouslyEquipped is not null)
            {
                previouslyEquipped.Location = guardian;
            }
        }

        item.Location = destination;
        Revision++;
        return true;
    }

    public void Reset()
    {
        Items =
        [
            Item("pulse-01", "Northstar Pulse", "Kinetic Weapons", "Pulse Rifle", "Legendary", "A stable synthetic pulse rifle for medium-range encounters.", "#6ea8fe", "NP", 2010, "hunter-equipped"),
            Item("scout-01", "Wayfinder SR4", "Kinetic Weapons", "Scout Rifle", "Legendary", "A lightweight mock scout rifle with a crisp sight.", "#73d2de", "WS", 2003, "hunter"),
            Item("hand-01", "Quiet Resolve", "Kinetic Weapons", "Hand Cannon", "Exotic", "A fictional hand cannon created exclusively for this demonstration.", "#f5c451", "QR", 2007, "Vault"),
            Item("auto-01", "Foundry Pattern 7", "Kinetic Weapons", "Auto Rifle", "Legendary", "A dependable synthetic auto rifle.", "#9b8afb", "FP", 1998, "titan-equipped"),
            Item("smg-01", "Signal Runner", "Kinetic Weapons", "Submachine Gun", "Legendary", "A compact mock weapon tuned for mobility.", "#ff8fab", "SR", 2001, "warlock-equipped"),

            Item("fusion-01", "Prismatic Current", "Energy Weapons", "Fusion Rifle", "Legendary", "A fictional Arc fusion rifle.", "#7bdff2", "PC", 2008, "warlock-equipped"),
            Item("bow-01", "Emberline", "Energy Weapons", "Combat Bow", "Exotic", "A synthetic Solar bow with an unmistakable silhouette.", "#ff9f68", "EL", 2010, "hunter-equipped"),
            Item("sidearm-01", "Copper Circuit", "Energy Weapons", "Sidearm", "Legendary", "A mock sidearm assembled from experimental parts.", "#f4a261", "CC", 1997, "Vault"),
            Item("shotgun-01", "Rampart Echo", "Energy Weapons", "Shotgun", "Legendary", "A close-range synthetic defensive weapon.", "#ef476f", "RE", 2005, "titan-equipped"),

            Item("rocket-01", "Orbital Refrain", "Power Weapons", "Rocket Launcher", "Exotic", "A fictional launcher built for dramatic demo transfers.", "#ffd166", "OR", 2010, "Vault"),
            Item("sword-01", "Cerulean Edge", "Power Weapons", "Sword", "Legendary", "A mock adaptive sword with a blue alloy blade.", "#4cc9f0", "CE", 2004, "hunter-equipped"),
            Item("machine-01", "Long Watch", "Power Weapons", "Machine Gun", "Legendary", "A synthetic high-capacity machine gun.", "#90be6d", "LW", 2002, "titan-equipped"),
            Item("linear-01", "Event Horizon", "Power Weapons", "Linear Fusion Rifle", "Legendary", "A fictional precision power weapon.", "#c77dff", "EH", 2006, "warlock-equipped"),

            Item("hood-01", "Wayfarer's Hood", "Helmets", "Hunter Helmet", "Legendary", "Synthetic armor for the demo Hunter.", "#65c8ff", "WH", 2009, "hunter-equipped"),
            Item("cowl-01", "Archive Cowl", "Helmets", "Warlock Helmet", "Legendary", "Synthetic armor for the demo Warlock.", "#ffd166", "AC", 2008, "warlock-equipped"),
            Item("helm-01", "Citadel Helm", "Helmets", "Titan Helmet", "Exotic", "Synthetic armor for the demo Titan.", "#ff6b7a", "CH", 2010, "titan-equipped"),
            Item("mask-01", "Midnight Survey", "Helmets", "Helmet", "Legendary", "A spare fictional helmet kept in the vault.", "#adb5bd", "MS", 1999, "Vault"),

            Item("vest-01", "Cloudstep Vest", "Chest Armor", "Hunter Chest Armor", "Legendary", "Synthetic chest armor for the demo Hunter.", "#65c8ff", "CV", 2007, "hunter-equipped"),
            Item("robes-01", "Heliograph Robes", "Chest Armor", "Warlock Chest Armor", "Exotic", "Synthetic chest armor for the demo Warlock.", "#ffd166", "HR", 2010, "warlock-equipped"),
            Item("plate-01", "Bastion Plate", "Chest Armor", "Titan Chest Armor", "Legendary", "Synthetic chest armor for the demo Titan.", "#ff6b7a", "BP", 2006, "titan-equipped"),
            Item("coat-01", "Deepfield Coat", "Chest Armor", "Chest Armor", "Legendary", "A spare fictional armor piece stored in the vault.", "#9b8afb", "DC", 2001, "Vault"),
        ];

        Revision++;
    }

    private bool IsValidDestination(string destination)
    {
        if (destination == "Vault")
        {
            return true;
        }

        return Guardians.Any(guardian =>
            destination == guardian.Id || destination == guardian.Id + "-equipped");
    }

    private static DemoItem Item(
        string id,
        string name,
        string category,
        string type,
        string rarity,
        string description,
        string accent,
        string glyph,
        int power,
        string location)
    {
        if (!AllowedCategories.Contains(category))
        {
            throw new ArgumentOutOfRangeException(nameof(category));
        }

        return new DemoItem
        {
            Id = id,
            Name = name,
            Category = category,
            TypeName = type,
            Rarity = rarity,
            Description = description,
            Accent = accent,
            Glyph = glyph,
            Power = power,
            Location = location,
        };
    }
}
