using System;
using System.Collections.Generic;
using GameData;
using Gear;
using HarmonyLib;
using LevelGeneration;
using Player;
using UnityEngine;

namespace GTFO.EZ.Patches;

// The kits for Consumables Are Kits: the game's own health, tool and ammo resource packs, found
// sparsely in lockers and out in the open on top of the zone's normal resource packs (which are
// left as they are). Each holds Kit Charges Min-Max charges - one charge is one use, 20% of a
// pack - and there are only as many as add up to Kit Loot (%) of the zone's resource budget,
// split between the three kinds the way that budget is. A kit spawned as a consumable has no
// contents of its own, so ConsumableAmmoMin/Max are set here, and each one is snapped to whole
// charges once it spawns (KitCharges_SetupAsConsumable_Patch below).
// Same approach as GTFO.AbsurdWeapons' LootMix, which takes precedence: with its Loot Mix on, this
// isn't run at all (see GameDataTweaks.AbsurdWeaponsLootMixOn) and its own mix fills the tables.
internal static class KitLoot
{
    internal const int ChargePercent = 20;

    private static readonly HashSet<uint> KitIds = new();

    internal static int MinCharges { get; private set; }
    internal static int MaxCharges { get; private set; }

    internal static bool IsKit(uint itemId) => KitIds.Contains(itemId);

    internal static List<(uint Id, float Weight)> Resolve()
    {
        KitIds.Clear();
        MinCharges = Math.Max(1, Plugin.KitChargesMin.Value);
        MaxCharges = Math.Max(MinCharges, Plugin.KitChargesMax.Value);

        var (health, weaponAmmo, toolAmmo) = AverageBudgetPerZone();

        var mix = new List<(uint, float)>();
        Add(mix, Find("health", "medi", "health"), health * Plugin.HealthKitWeight.Value, "health");
        Add(mix, Find("tool", "tool"), toolAmmo * Plugin.ToolKitWeight.Value, "tool");
        Add(mix, Find("ammo", "ammo"), weaponAmmo * Plugin.AmmoKitWeight.Value, "ammo");
        Plugin.Logger.LogInfo($"Consumables Are Kits: {mix.Count} kit type(s), {MinCharges}-{MaxCharges} charge(s) each; " +
            $"budget per zone {health:0.##} health, {weaponAmmo:0.##} ammo, {toolAmmo:0.##} tool packs.");
        return mix;
    }

    // How many kits a zone gets: Kit Loot (%) of the zone's health + ammo + tool pack budget, in
    // packs, divided by what an average kit holds.
    internal static float KitsPerZone()
    {
        var (health, weaponAmmo, toolAmmo) = AverageBudgetPerZone();
        var averageKit = (MinCharges + MaxCharges) / 2f * ChargePercent / 100f;
        return Mathf.Max(0f, Plugin.KitLootPercent.Value) / 100f * (health + weaponAmmo + toolAmmo) / averageKit;
    }

    // Averaged over the difficulty tiers - a consumable table isn't tied to one tier, so the kit
    // count can only follow the typical zone, not each one.
    private static (float Health, float WeaponAmmo, float ToolAmmo) AverageBudgetPerZone()
    {
        float health = 0f, weaponAmmo = 0f, toolAmmo = 0f;
        var tiers = 0;
        foreach (var block in GameDataBlockBase<ExpeditionBalanceDataBlock>.GetAllBlocks())
        {
            if (block == null) continue;
            health += block.HealthPerZone;
            weaponAmmo += block.WeaponAmmoPerZone;
            toolAmmo += block.ToolAmmoPerZone;
            tiers++;
        }
        return tiers == 0 ? (1f, 1f, 1f) : (health / tiers, weaponAmmo / tiers, toolAmmo / tiers);
    }

    private static void Add(List<(uint, float)> mix, uint? id, float weight, string what)
    {
        if (id == null) Plugin.Logger.LogWarning($"Consumables Are Kits: no {what} kit item found - left out.");
        else if (weight > 0f) mix.Add((id.Value, weight));
    }

    // The resource pack whose name has one of `keys` in it. Tool packs are skipped when looking for
    // ammo (a tool refill's name can contain "ammo"); disinfection packs never match.
    private static uint? Find(string what, params string[] keys)
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
            block.ConsumableAmmoMin = MinCharges * ChargePercent;
            block.ConsumableAmmoMax = MaxCharges * ChargePercent;
            KitIds.Add(block.persistentID);
            return block.persistentID;
        }
        return null;
    }
}

// The game rolls a consumable kit's contents anywhere between ConsumableAmmoMin and Max, so a kit
// could hold 1.4 charges. Rounded to whole charges as it spawns. Level generation is seeded the
// same on every client, so each one rounds the same roll the same way.
[HarmonyPatch(typeof(LG_PickupItem))]
internal static class KitCharges_SetupAsConsumable_Patch
{
    private static int _logged;

    [HarmonyPatch(nameof(LG_PickupItem.SetupAsConsumable), new[] { typeof(int), typeof(uint) })]
    [HarmonyPostfix]
    private static void AfterSetup(LG_PickupItem __instance) => Snap(__instance);

    [HarmonyPatch(nameof(LG_PickupItem.SetupAsConsumable), new[] { typeof(ConsumableDistributionDataBlock), typeof(int), typeof(uint) })]
    [HarmonyPostfix]
    private static void AfterSetupFromTable(LG_PickupItem __instance) => Snap(__instance);

    private static void Snap(LG_PickupItem pickup)
    {
        try
        {
            var pack = pickup.GetComponentInChildren<ResourcePackPickup>();
            if (pack == null || pack.ItemDataBlock == null || !KitLoot.IsKit(pack.ItemDataBlock.persistentID)) return;

            var custom = pack.GetCustomData();
            var charges = Mathf.Clamp(Mathf.RoundToInt(custom.ammo / KitLoot.ChargePercent), KitLoot.MinCharges, KitLoot.MaxCharges);
            var ammo = charges * KitLoot.ChargePercent;
            if (custom.ammo == ammo) return;

            if (_logged++ < 3) Plugin.Logger.LogInfo($"Consumables Are Kits: '{pack.ItemDataBlock.name}' rolled {custom.ammo:0.#}%, rounded to {charges} charge(s).");
            custom.ammo = ammo;
            pack.SetCustomData(custom, false);
        }
        catch (Exception e)
        {
            Plugin.Logger.LogWarning($"Consumables Are Kits: couldn't round a kit to whole charges. {e.Message}");
        }
    }
}
