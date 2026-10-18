using System;
using System.Collections.Generic;
using Agents;
using GameData;
using HarmonyLib;
using LevelGeneration;
using Player;

namespace GTFO.EZ.Patches;

// Artifact pickups give the player who picked one up a buff for the rest of the level: each Bold
// (Uncommon) +5% max health, each Aggressive (Rare) +5% damage dealt, each Muted (Common) 5% harder
// for enemies to detect. Counts are kept per player on every client - OnSyncStateChange runs
// everywhere with the picker - because enemy detection runs on the host and damage on whoever
// dealt it, so each needs everyone's counts; max health only ever changes on the owner's own game.
internal static class ArtifactBuffs
{
    private static readonly Dictionary<ulong, int[]> CountsByPlayer = new();
    private static readonly Dictionary<IntPtr, (ulong Player, ArtifactCategory Category)> Counted = new();

    private static IntPtr _healthOwner;
    private static float _baseMaxHealth;

    internal static int Count(PlayerAgent? player, ArtifactCategory category)
    {
        var owner = player?.Owner;
        if (owner == null || !CountsByPlayer.TryGetValue(owner.Lookup, out var counts)) return 0;
        return counts[(int)category];
    }

    // A checkpoint restart puts artifacts picked up after it back in the level (a recall with
    // PlacedInLevel), so that pickup's buff is taken back off; a pickup is only counted once.
    internal static void OnArtifactState(ArtifactPickup_Core artifact, ePickupItemStatus status, PlayerAgent? player)
    {
        var key = artifact.Pointer;
        var category = artifact.m_artifactCategory;

        if (status == ePickupItemStatus.PickedUp)
        {
            var owner = player?.Owner;
            if (owner == null || Counted.ContainsKey(key)) return;
            Counted[key] = (owner.Lookup, category);
            Change(owner.Lookup, category, +1);
            Plugin.Logger.LogInfo($"Artifact buffs: {owner.NickName} picked up a {Name(category)} artifact ({Count(player, category)} this level).");
        }
        else if (Counted.TryGetValue(key, out var counted))
        {
            Counted.Remove(key);
            Change(counted.Player, counted.Category, -1);
        }
    }

    private static void Change(ulong player, ArtifactCategory category, int by)
    {
        if (!CountsByPlayer.TryGetValue(player, out var counts)) CountsByPlayer[player] = counts = new int[(int)ArtifactCategory._COUNT];
        counts[(int)category] = Math.Max(0, counts[(int)category] + by);
        if (category == ArtifactCategory.Uncommon) ApplyMaxHealth();
    }

    // The local player's max health: the level's starting max (GTFO.EZ's health multiplier already
    // in it) plus BoldMaxHealthPercent per Bold pickup. The gain is healed too, so a pickup shows.
    internal static void ApplyMaxHealth()
    {
        if (!PlayerManager.TryGetLocalPlayerAgent(out var agent) || agent == null) return;
        var damage = agent.Damage?.TryCast<Dam_PlayerDamageLocal>();
        if (damage == null) return;
        if (damage.Pointer != _healthOwner)
        {
            _healthOwner = damage.Pointer;
            _baseMaxHealth = damage.HealthMax;
        }

        var target = _baseMaxHealth * (1f + Plugin.ArtifactBoldMaxHealthPercent.Value / 100f * Count(agent, ArtifactCategory.Uncommon));
        var gain = target - damage.HealthMax;
        if (Math.Abs(gain) < 0.01f) return;
        damage.HealthMax = target;
        damage.SendSetHealth(Math.Min(target, Math.Max(0.01f, damage.Health + Math.Max(0f, gain))));
    }

    internal static void Reset()
    {
        CountsByPlayer.Clear();
        Counted.Clear();
        _healthOwner = IntPtr.Zero; // the next ApplyMaxHealth reads this level's starting max fresh
    }

    internal static float DamageMultiplier(Agent? source)
    {
        var player = source?.TryCast<PlayerAgent>();
        return 1f + Plugin.ArtifactAggressiveDamagePercent.Value / 100f * Count(player, ArtifactCategory.Rare);
    }

    internal static float DetectionMultiplier(PlayerAgent player) =>
        Math.Max(0.25f, 1f - Plugin.ArtifactMutedQuieterPercent.Value / 100f * Count(player, ArtifactCategory.Common));

    private static string Name(ArtifactCategory category) => category switch
    {
        ArtifactCategory.Common => "Muted",
        ArtifactCategory.Uncommon => "Bold",
        ArtifactCategory.Rare => "Aggressive",
        _ => category.ToString(),
    };
}

[HarmonyPatch]
internal static class ArtifactBuffs_Patches
{
    [HarmonyPatch(typeof(ArtifactPickup_Core), nameof(ArtifactPickup_Core.OnSyncStateChange))]
    [HarmonyPostfix]
    private static void AfterArtifactState(ArtifactPickup_Core __instance, ePickupItemStatus status, PlayerAgent player)
    {
        if (!Plugin.ArtifactBuffsEnabled.Value) return;
        try
        {
            ArtifactBuffs.OnArtifactState(__instance, status, player);
        }
        catch (Exception e)
        {
            Plugin.Logger.LogWarning($"Artifact buffs: couldn't count a pickup. {e}");
        }
    }

    [HarmonyPatch(typeof(Dam_PlayerDamageLocal), nameof(Dam_PlayerDamageLocal.Setup))]
    [HarmonyPostfix]
    private static void AfterLevelSetup() => ArtifactBuffs.Reset();

    [HarmonyPatch(typeof(Dam_EnemyDamageLimb), nameof(Dam_EnemyDamageLimb.BulletDamage))]
    [HarmonyPrefix]
    private static void BeforeBulletDamage(ref float dam, Agent sourceAgent)
    {
        if (Plugin.ArtifactBuffsEnabled.Value) dam *= SafeDamageMultiplier(sourceAgent);
    }

    [HarmonyPatch(typeof(Dam_EnemyDamageLimb), nameof(Dam_EnemyDamageLimb.MeleeDamage))]
    [HarmonyPrefix]
    private static void BeforeMeleeDamage(ref float dam, Agent sourceAgent)
    {
        if (Plugin.ArtifactBuffsEnabled.Value) dam *= SafeDamageMultiplier(sourceAgent);
    }

    [HarmonyPatch(typeof(PlayerAgent), nameof(PlayerAgent.GetDetectionMod))]
    [HarmonyPostfix]
    private static void AfterDetectionMod(PlayerAgent __instance, ref float __result)
    {
        if (!Plugin.ArtifactBuffsEnabled.Value) return;
        try
        {
            __result *= ArtifactBuffs.DetectionMultiplier(__instance);
        }
        catch (Exception)
        {
            // leave detection as the game computed it
        }
    }

    private static float SafeDamageMultiplier(Agent sourceAgent)
    {
        try
        {
            return ArtifactBuffs.DamageMultiplier(sourceAgent);
        }
        catch (Exception)
        {
            return 1f;
        }
    }
}
