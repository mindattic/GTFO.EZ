using System.Collections.Generic;
using GameData;
using Player;

namespace GTFO.EZ.Patches;

// The kits for Consumables Are Kits: the game's own health, tool and ammo resource packs. Those
// normally get their contents from the zone's resource budget; one spawned as a consumable has
// none of its own, so ConsumableAmmoMin/Max are set to Kit Contents Min/Max (percent of a pack).
// Same approach as GTFO.AbsurdWeapons' LootMix, which runs after this: it sees these kits-only
// tables, keeps the kits (and their contents) and adds its Hand Grenade and Stim Pack alongside.
internal static class KitLoot
{
    internal static List<(uint Id, float Weight)> Resolve()
    {
        var min = System.Math.Max(0, Plugin.KitContentsMin.Value);
        var max = System.Math.Max(min, Plugin.KitContentsMax.Value);

        var mix = new List<(uint, float)>();
        Add(mix, Find("health", min, max, "medi", "health"), Plugin.HealthKitWeight.Value, "health");
        Add(mix, Find("tool", min, max, "tool"), Plugin.ToolKitWeight.Value, "tool");
        Add(mix, Find("ammo", min, max, "ammo"), Plugin.AmmoKitWeight.Value, "ammo");
        Plugin.Logger.LogInfo($"Consumables Are Kits: {mix.Count} kit type(s), {min}-{max}% each.");
        return mix;
    }

    private static void Add(List<(uint, float)> mix, uint? id, float weight, string what)
    {
        if (id == null) Plugin.Logger.LogWarning($"Consumables Are Kits: no {what} kit item found - left out.");
        else if (weight > 0f) mix.Add((id.Value, weight));
    }

    // The resource pack whose name has one of `keys` in it. Tool packs are skipped when looking for
    // ammo (a tool refill's name can contain "ammo"); disinfection packs never match.
    private static uint? Find(string what, int min, int max, params string[] keys)
    {
        foreach (var block in GameDataBlockBase<ItemDataBlock>.GetAllBlocks())
        {
            if (block == null || block.inventorySlot != InventorySlot.ResourcePack) continue;
            var name = (block.name ?? string.Empty).ToLowerInvariant();
            if (name.Contains("disinfect") || (what == "ammo" && name.Contains("tool"))) continue;

            var match = false;
            foreach (var key in keys) match |= name.Contains(key);
            if (!match) continue;

            Plugin.Logger.LogInfo($"Consumables Are Kits: {what} kit is '{block.name}' (item {block.persistentID}).");
            block.ConsumableAmmoMin = min;
            block.ConsumableAmmoMax = max;
            return block.persistentID;
        }
        return null;
    }
}
