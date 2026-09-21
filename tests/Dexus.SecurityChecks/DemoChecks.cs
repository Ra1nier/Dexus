using Dexus.Demo;

internal static class DemoChecks
{
    public static void Run()
    {
        var first = new DemoInventory();
        var second = new DemoInventory();

        Check(first.Items.Count == 21, "Demo inventory does not contain the curated item set.");
        Check(first.Guardians.Count == 3, "Demo inventory does not contain three Guardians.");
        Check(first.Categories.Count == 5, "Demo inventory categories changed unexpectedly.");

        DemoItem vaultedRocket = first.Items.Single(item => item.Id == "rocket-01");
        Check(first.Move(vaultedRocket, "hunter"), "A valid demo transfer was rejected.");
        Check(vaultedRocket.Location == "hunter", "Demo transfer did not update the item location.");
        Check(second.Items.Single(item => item.Id == "rocket-01").Location == "Vault",
            "Two demo visitors shared mutable inventory state.");

        DemoItem replacementHelmet = first.Items.Single(item => item.Id == "mask-01");
        DemoItem originalHelmet = first.Items.Single(item => item.Id == "hood-01");
        Check(first.Move(replacementHelmet, "hunter-equipped"), "Demo equip was rejected.");
        Check(replacementHelmet.Location == "hunter-equipped", "Demo equip did not update the replacement.");
        Check(originalHelmet.Location == "hunter", "Previously equipped demo item was not returned to inventory.");

        Check(!first.Move(replacementHelmet, "not-a-real-location"), "Demo accepted an invalid destination.");

        first.Reset();
        Check(first.Items.Single(item => item.Id == "rocket-01").Location == "Vault",
            "Demo reset did not restore the curated inventory.");
        Check(first.Items.Single(item => item.Id == "hood-01").Location == "hunter-equipped",
            "Demo reset did not restore equipped items.");

        string[] references = typeof(DemoInventory).Assembly.GetReferencedAssemblies().Select(reference => reference.Name ?? "").ToArray();
        Check(!references.Contains("ContentManager")
            && !references.Contains("RequestManager")
            && !references.Contains("OAuthService")
            && !references.Contains("ClientProfile"),
            "The isolated demo project references production account or Bungie services.");

        Console.WriteLine("Demo checks passed: curated inventory, isolated visitors, transfers, equip replacement, reset, and no production service references.");
    }

    private static void Check(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }
}
