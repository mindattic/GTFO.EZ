using System;
using GameData;
using HarmonyLib;

namespace GtfoEZ.Patches;

/// <summary>
/// Applies all stat tweaks as multipliers against whatever GameDataInit just loaded, instead of
/// shipping replacement datablock files. Survives game patches that rebalance base values, and
/// only breaks if Overkill renames/restructures a field this touches.
/// </summary>
[HarmonyPatch(typeof(GameDataInit), nameof(GameDataInit.Initialize))]
internal static class GameDataInit_Initialize_Patch
{
    [HarmonyPostfix]
    private static void Postfix()
    {
        try
        {
            ApplyPlayerTweaks();
            ApplyFlashlightTweaks();
            ApplyDetectionTweaks();
            Plugin.Logger.LogInfo("GtfoEZ tweaks applied. Goo goo. Gah gah.");
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError($"GtfoEZ failed to apply tweaks: {e}");
        }
    }

    // Player datablock is hardcoded to id 1 in GTFO.
    private static void ApplyPlayerTweaks()
    {
        var player = GameDataBlockBase<PlayerDataBlock>.GetBlock(1);
        if (player == null)
        {
            Plugin.Logger.LogWarning("PlayerDataBlock (id 1) not found, skipping player tweaks.");
            return;
        }

        var healthMult = Plugin.HealthMultiplier.Value;
        player.health *= healthMult;

        // Rate and cap are deliberately separate multipliers: vanilla under-regens relative to
        // its own cap, so hitting both target numbers (1.0/s against a 40% cap) needs different
        // factors. Regen delay after damage is left untouched on purpose - see config comment.
        player.healthRegenPerSecond *= Plugin.HealthRegenRateMultiplier.Value;
        player.healthRegenRelMax = Math.Min(1f, player.healthRegenRelMax * Plugin.HealthRegenCapMultiplier.Value);

        // Equalize rather than multiply: vanilla's "in combat" rate is an approximate research
        // figure, but copying the known-good "out of combat" rate is exact regardless of the
        // real vanilla numbers and stays correct if Overkill rebalances them later.
        if (Plugin.RemoveCombatStaminaPenalty.Value)
        {
            player.StaminaRegenNotRestingInCombat = player.StaminaRegenNotRestingOutOfCombat;
        }

        var fallDamageMult = Plugin.FallDamageMultiplier.Value;
        player.fallDamageMin *= fallDamageMult;
        player.fallDamageMax *= fallDamageMult;

        player.fallDamageMinHeight *= Plugin.FallDamageMinHeightMultiplier.Value;
        player.fallDamageMaxHeight *= Plugin.FallDamageMaxHeightMultiplier.Value;

        var ammoMult = Plugin.AmmoMultiplier.Value;
        player.AmmoStandardInitial = ScaleInt(player.AmmoStandardInitial, ammoMult);
        player.AmmoStandardMaxCap = ScaleInt(player.AmmoStandardMaxCap, ammoMult);
        player.AmmoStandardResourcePackMaxCap = ScaleInt(player.AmmoStandardResourcePackMaxCap, ammoMult);
        player.AmmoSpecialInitial = ScaleInt(player.AmmoSpecialInitial, ammoMult);
        player.AmmoSpecialMaxCap = ScaleInt(player.AmmoSpecialMaxCap, ammoMult);
        player.AmmoSpecialResourcePackMaxCap = ScaleInt(player.AmmoSpecialResourcePackMaxCap, ammoMult);
        player.AmmoClassInitial = ScaleInt(player.AmmoClassInitial, ammoMult);
        player.AmmoClassMaxCap = ScaleInt(player.AmmoClassMaxCap, ammoMult);
        player.AmmoClassResourcePackMaxCap = ScaleInt(player.AmmoClassResourcePackMaxCap, ammoMult);
    }

    // Covers both headlamps and weapon-mounted lights: both reference the same datablock type.
    // Angle + intensity only, not range - a longer beam reaches further into the dark than the
    // level's lighting was designed for, while angle/intensity just make the existing cone better.
    private static void ApplyFlashlightTweaks()
    {
        var angleMult = Plugin.FlashlightAngleMultiplier.Value;
        var intensityMult = Plugin.FlashlightIntensityMultiplier.Value;

        foreach (var block in GameDataBlockBase<FlashlightSettingsDataBlock>.GetAllBlocks())
        {
            block.angle *= angleMult;
            block.intensity *= intensityMult;
        }
    }

    // Multiple detection profiles exist (e.g. normal enemies vs. sleepers); scale all of them.
    private static void ApplyDetectionTweaks()
    {
        var mult = Plugin.DetectionDistanceMultiplier.Value;

        foreach (var block in GameDataBlockBase<EnemyDetectionDataBlock>.GetAllBlocks())
        {
            block.movementDetectionDistance *= mult;
        }
    }

    private static int ScaleInt(int value, float multiplier) => (int)Math.Round(value * multiplier);
}
