using System;
using System.Collections.Generic;
using GameData;
using HarmonyLib;

namespace GTFO.EZ.Patches;

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
            ApplyEnemyBalanceTweaks();
            ApplyLootTweaks();
            ApplyExpeditionBalanceTweaks();
            ApplyMeleeTweaks();
            Plugin.Logger.LogInfo("GTFO.EZ tweaks applied. Goo goo. Gah gah.");
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError($"GTFO.EZ failed to apply tweaks: {e}");
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

        // Equalize rather than multiply: vanilla's "in combat" rates are approximate research
        // figures, but copying the known-good "out of combat" rates is exact regardless of the
        // real vanilla numbers and stays correct if Overkill rebalances them later.
        //
        // The "NotResting" pair was the original target here, but vanilla has both of those at
        // 0 regardless of combat state (stamina only regens while standing still/"resting"), so
        // equalizing them was a no-op - this feature shipped doing nothing. The real combat
        // penalty is twofold: a lower regen rate while resting in combat (0.15/s vs 0.25/s out
        // of combat), and a hard 90% cap on stamina while in combat. Both are fixed here now.
        if (Plugin.RemoveCombatStaminaPenalty.Value)
        {
            player.StaminaRegenNotRestingInCombat = player.StaminaRegenNotRestingOutOfCombat;
            player.StaminaRegenRestingInCombat = player.StaminaRegenRestingOutOfCombat;
            player.StaminaMaximumCapWhenInCombat = 1f;
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

        player.walkMoveSpeed *= Plugin.WalkSpeedMultiplier.Value;
        player.runMoveSpeed *= Plugin.RunSpeedMultiplier.Value;
        player.airMoveSpeed *= Plugin.AirSpeedMultiplier.Value;
        player.crouchMoveSpeed *= Plugin.CrouchSpeedMultiplier.Value;
        player.ladderMoveSpeed *= Plugin.LadderSpeedMultiplier.Value;
        player.jumpVelInitial *= Plugin.JumpHeightMultiplier.Value;

        player.friendlyFireMulti *= Plugin.FriendlyFireMultiplier.Value;

        player.battery = ScaleInt(player.battery, Plugin.BatteryCapacityMultiplier.Value);
        player.smallBatteryConsumtionPerSec *= Plugin.SmallBatteryDrainMultiplier.Value;
        player.mediumBatteryConsumtionPerSec *= Plugin.MediumBatteryDrainMultiplier.Value;
        player.largeBatteryConsumtionPerSec *= Plugin.LargeBatteryDrainMultiplier.Value;

        player.noAirDamageRel *= Plugin.NoAirDamageMultiplier.Value;
        player.noAirTimeToEmpty *= Plugin.NoAirDepletionTimeMultiplier.Value;
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

    // One EnemyBalancingDataBlock per enemy type. Health is a nested struct, so it's copied out,
    // modified, and written back rather than mutated in place.
    private static void ApplyEnemyBalanceTweaks()
    {
        var healthMult = Plugin.EnemyHealthMultiplier.Value;
        var meleeMult = Plugin.EnemyMeleeDamageMultiplier.Value;
        var tentacleMult = Plugin.EnemyTentacleDamageMultiplier.Value;

        foreach (var block in GameDataBlockBase<EnemyBalancingDataBlock>.GetAllBlocks())
        {
            var health = block.Health;
            health.HealthMax *= healthMult;
            block.Health = health;

            block.MeleeAttackDamage *= meleeMult;
            block.TentacleAttackDamage *= tentacleMult;
        }
    }

    // Consumables (med/ammo packs, syringes) and big pickups (fog turbines, artifacts) are
    // separate distribution datablocks, each with its own spawns-per-zone count.
    private static void ApplyLootTweaks()
    {
        var consumableMult = Plugin.ConsumableSpawnMultiplier.Value;
        foreach (var block in GameDataBlockBase<ConsumableDistributionDataBlock>.GetAllBlocks())
        {
            block.SpawnsPerZone = ScaleInt(block.SpawnsPerZone, consumableMult);

            // Each consumable is drawn from a shared weighted lottery per spawn slot, so scaling
            // an item's weight retunes the odds between items rather than the total amount of
            // loot (that's the SpawnsPerZone scale above). Both glow stick colors share one entry.
            foreach (var entry in block.SpawnData)
            {
                if (ConsumableWeightMultipliers.TryGetValue(entry.ItemID, out var getMultiplier))
                {
                    entry.Weight *= getMultiplier();
                }
            }
        }

        var bigPickupMult = Plugin.BigPickupSpawnMultiplier.Value;
        foreach (var block in GameDataBlockBase<BigPickupDistributionDataBlock>.GetAllBlocks())
        {
            block.SpawnsPerZone = ScaleInt(block.SpawnsPerZone, bigPickupMult);
        }
    }

    // ItemIDs confirmed by logging GameDataBlockBase<ItemDataBlock> entries referenced from
    // ConsumableDistributionDataBlock.SpawnData at runtime - not documented anywhere official.
    private static readonly Dictionary<uint, Func<float>> ConsumableWeightMultipliers = new()
    {
        { 30, () => Plugin.FlashlightItemSpawnWeightMultiplier.Value },   // Long Range Flashlight
        { 114, () => Plugin.GlowstickSpawnWeightMultiplier.Value },        // Glow Stick
        { 174, () => Plugin.GlowstickSpawnWeightMultiplier.Value },        // Glow Stick Yellow
        { 115, () => Plugin.CFoamGrenadeSpawnWeightMultiplier.Value },     // C-Foam Grenade
        { 117, () => Plugin.FogRepellerSpawnWeightMultiplier.Value },      // Fog Repeller
        { 116, () => Plugin.LockMelterSpawnWeightMultiplier.Value },       // Lock Melter
        { 139, () => Plugin.ExplosiveTripMineSpawnWeightMultiplier.Value }, // Explosive Trip Mine
        { 142, () => Plugin.MeleeBuffSyringeSpawnWeightMultiplier.Value }, // IIx Syringe
        { 140, () => Plugin.HealthSyringeSpawnWeightMultiplier.Value },    // I2-LP Syringe
        { 144, () => Plugin.CFoamTripmineSpawnWeightMultiplier.Value },    // C-Foam Tripmine
    };

    // ExpeditionBalanceDataBlock is the per-difficulty-tier resource/enemy/door budget table -
    // one block per tier (Easy/Normal/Hard/...), so every tier gets scaled by the same factor.
    private static void ApplyExpeditionBalanceTweaks()
    {
        foreach (var block in GameDataBlockBase<ExpeditionBalanceDataBlock>.GetAllBlocks())
        {
            block.HealthPerZone *= Plugin.HealthPackResourceMultiplier.Value;
            block.DisinfectionPerZone *= Plugin.DisinfectionResourceMultiplier.Value;
            block.WeaponAmmoPerZone *= Plugin.WeaponAmmoResourceMultiplier.Value;
            block.ToolAmmoPerZone *= Plugin.ToolAmmoResourceMultiplier.Value;
            block.CommodityValuePerZone *= Plugin.CommodityValueMultiplier.Value;
            block.ChanceToPutCommodityInResourceContainer *= Plugin.CommodityInContainerChanceMultiplier.Value;
            block.ChanceToPutArtifactInResourceContainer *= Plugin.ArtifactInContainerChanceMultiplier.Value;
            block.ChanceToSpawnCommodityLargePack *= Plugin.LargeCommodityPackChanceMultiplier.Value;
            block.ChanceToSpawnCommodityMediumPack *= Plugin.MediumCommodityPackChanceMultiplier.Value;
            block.ChanceToReUseResourceContainer *= Plugin.ResourceContainerReuseChanceMultiplier.Value;
            block.MaxPacksPerResourceContainer = ScaleInt(block.MaxPacksPerResourceContainer, Plugin.MaxPacksPerContainerMultiplier.Value);
            block.EmptyWeakResourceContainersPerZone *= Plugin.EmptyWeakContainersPerZoneMultiplier.Value;
            block.EmptySecureResourceContainersPerZone *= Plugin.EmptySecureContainersPerZoneMultiplier.Value;
            block.LootPerZone *= Plugin.LootPerZoneMultiplier.Value;
            block.AirPerZone *= Plugin.AirPerZoneMultiplier.Value;
            block.AirPerZoneInNoAir *= Plugin.AirPerZoneInNoAirZoneMultiplier.Value;
            block.TerminalsPerZone *= Plugin.TerminalsPerZoneMultiplier.Value;
            block.WeakResourceContainerWithPackChanceForLocked *= Plugin.LockedWeakContainerWithPackChanceMultiplier.Value;

            var packSizeMult = Plugin.ResourcePackSizeMultiplier.Value;
            for (var i = 0; i < block.ResourcePackSizes.Count; i++)
            {
                block.ResourcePackSizes[i] *= packSizeMult;
            }

            block.EnemyPatrolGroupsPerZone = ScaleInt(block.EnemyPatrolGroupsPerZone, Plugin.EnemyPatrolGroupsPerZoneMultiplier.Value);
            block.StaticEnemiesMaxPerZone = ScaleInt(block.StaticEnemiesMaxPerZone, Plugin.StaticEnemiesMaxPerZoneMultiplier.Value);
            block.StaticEnemiesMaxSmallArea = ScaleInt(block.StaticEnemiesMaxSmallArea, Plugin.StaticEnemiesMaxSmallAreaMultiplier.Value);
            block.StaticEnemiesMaxMediumArea = ScaleInt(block.StaticEnemiesMaxMediumArea, Plugin.StaticEnemiesMaxMediumAreaMultiplier.Value);
            block.StaticEnemiesMaxLargeArea = ScaleInt(block.StaticEnemiesMaxLargeArea, Plugin.StaticEnemiesMaxLargeAreaMultiplier.Value);
            block.StaticEnemiesMaxHugeArea = ScaleInt(block.StaticEnemiesMaxHugeArea, Plugin.StaticEnemiesMaxHugeAreaMultiplier.Value);
            block.EnemyPopulationPerZone *= Plugin.EnemyPopulationPerZoneMultiplier.Value;

            block.VoxelCoverageAreaMultiplier *= Plugin.LevelGenVoxelCoverageMultiplier.Value;
            block.VoxelCoverageAreaScoringRandomMultiplier *= Plugin.LevelGenVoxelCoverageRandomnessMultiplier.Value;
            block.ArtifactsPerSegment = ScaleInt(block.ArtifactsPerSegment, Plugin.ArtifactsPerSegmentMultiplier.Value);
            block.ArtifactsPerLayer = ScaleInt(block.ArtifactsPerLayer, Plugin.ArtifactsPerLayerMultiplier.Value);

            block.WeakDoor4x4Health *= Plugin.WeakDoor4x4HealthMultiplier.Value;
            block.WeakDoor8x4Health *= Plugin.WeakDoor8x4HealthMultiplier.Value;
            block.WeakDoorChanceLockWeightNoLock *= Plugin.WeakDoorNoLockWeightMultiplier.Value;
            block.WeakDoorChanceLockWeightMeleeLock *= Plugin.WeakDoorMeleeLockWeightMultiplier.Value;
            block.WeakDoorChanceLockWeightHackableLock *= Plugin.WeakDoorHackableLockWeightMultiplier.Value;
            block.WeakDoorUnlockedChanceForOpen *= Plugin.WeakDoorUnlockedOpenChanceMultiplier.Value;
            block.WeakDoorOpenChanceForWallRemoverUsed *= Plugin.WeakDoorWallRemoverOpenChanceMultiplier.Value;
            block.WeakDoorLockHealth *= Plugin.WeakDoorLockHealthMultiplier.Value;
            block.GlueVolumeToDoorHealthConversion *= Plugin.GlueVolumeToDoorHealthConversionMultiplier.Value;
            block.GlueVolumeForDoorGlueMaxState *= Plugin.GlueVolumeForDoorMaxStateMultiplier.Value;

            ApplyStaticEnemyData(block.TentacleTraps,
                Plugin.TentacleTrapHealthMultiplier.Value,
                Plugin.TentacleTrapAttackDamageMultiplier.Value,
                Plugin.TentacleTrapMaxPerZoneMultiplier.Value,
                Plugin.TentacleTrapMaxSmallAreaMultiplier.Value,
                Plugin.TentacleTrapMaxMediumAreaMultiplier.Value,
                Plugin.TentacleTrapMaxLargeAreaMultiplier.Value,
                Plugin.TentacleTrapMaxHugeAreaMultiplier.Value);

            ApplyStaticEnemyData(block.ParasiteNests,
                Plugin.ParasiteNestHealthMultiplier.Value,
                Plugin.ParasiteNestAttackDamageMultiplier.Value,
                Plugin.ParasiteNestMaxPerZoneMultiplier.Value,
                Plugin.ParasiteNestMaxSmallAreaMultiplier.Value,
                Plugin.ParasiteNestMaxMediumAreaMultiplier.Value,
                Plugin.ParasiteNestMaxLargeAreaMultiplier.Value,
                Plugin.ParasiteNestMaxHugeAreaMultiplier.Value);
        }
    }

    // TentacleTraps/ParasiteNests are each a StaticEnemyData reference, not every tier has one
    // (e.g. tiers with no tentacle traps leave the property null), so this is a reference type
    // mutated in place rather than a struct copied out and written back like EnemyBalancingDataBlock.Health.
    private static void ApplyStaticEnemyData(
        StaticEnemyData? data, float healthMult, float attackDamageMult, float maxPerZoneMult,
        float maxSmallAreaMult, float maxMediumAreaMult, float maxLargeAreaMult, float maxHugeAreaMult)
    {
        if (data == null) return;

        data.Health *= healthMult;
        data.AttackDamage *= attackDamageMult;
        data.MaxPerZone = ScaleInt(data.MaxPerZone, maxPerZoneMult);
        data.MaxSmallArea = ScaleInt(data.MaxSmallArea, maxSmallAreaMult);
        data.MaxMediumArea = ScaleInt(data.MaxMediumArea, maxMediumAreaMult);
        data.MaxLargeArea = ScaleInt(data.MaxLargeArea, maxLargeAreaMult);
        data.MaxHugeArea = ScaleInt(data.MaxHugeArea, maxHugeAreaMult);
    }

    // One MeleeArchetypeDataBlock per weapon type - real PublicName values are Bat, Hammer,
    // Knife, Spear. Looked up by "{PublicName}.{FieldKey}" against the dictionary Plugin builds
    // from MeleeStatDefs x MeleeArchetypes, rather than 76 named fields. An archetype the game
    // adds later (unknown PublicName) is silently left at vanilla rather than throwing.
    // Stamina costs are ActionCost structs, copied out and written back like
    // EnemyBalancingDataBlock.Health. PublicName, NoiseLevel, the animation/SFX set IDs, and the
    // boolean behavior flags (SkipLimbDestruction, CanHitMultipleEnemies, etc.) aren't exposed -
    // none of them are multiplier-friendly balance values.
    private static void ApplyMeleeTweaks()
    {
        foreach (var block in GameDataBlockBase<MeleeArchetypeDataBlock>.GetAllBlocks())
        {
            var name = block.PublicName;

            block.LightAttackDamage *= MeleeMult(name, "LightAttackDamage");
            block.ChargedAttackDamage *= MeleeMult(name, "ChargedAttackDamage");
            block.LightStaggerMulti *= MeleeMult(name, "LightStaggerMulti");
            block.ChargedStaggerMulti *= MeleeMult(name, "ChargedStaggerMulti");
            block.LightPrecisionMulti *= MeleeMult(name, "LightPrecisionMulti");
            block.ChargedPrecisionMulti *= MeleeMult(name, "ChargedPrecisionMulti");
            block.LightEnvironmentMulti *= MeleeMult(name, "LightEnvironmentMulti");
            block.ChargedEnvironmentMulti *= MeleeMult(name, "ChargedEnvironmentMulti");
            block.LightBackstabberMulti *= MeleeMult(name, "LightBackstabberMulti");
            block.ChargedBackstabberMulti *= MeleeMult(name, "ChargedBackstabberMulti");
            block.LightSleeperMulti *= MeleeMult(name, "LightSleeperMulti");
            block.ChargedSleeperMulti *= MeleeMult(name, "ChargedSleeperMulti");
            block.CameraDamageRayLength *= MeleeMult(name, "CameraDamageRayLength");
            block.AttackSphereRadius *= MeleeMult(name, "AttackSphereRadius");
            block.PushDamageSphereRadius *= MeleeMult(name, "PushDamageSphereRadius");
            block.PlayerRunSpeedMultiWhileCharging *= MeleeMult(name, "PlayerRunSpeedMultiWhileCharging");

            var lightMult = MeleeMult(name, "LightAttackStaminaCost");
            var lightCost = block.LightAttackStaminaCost;
            lightCost.baseStaminaCostInCombat *= lightMult;
            lightCost.baseStaminaCostOutOfCombat *= lightMult;
            block.LightAttackStaminaCost = lightCost;

            var chargedMult = MeleeMult(name, "ChargedAttackStaminaCost");
            var chargedCost = block.ChargedAttackStaminaCost;
            chargedCost.baseStaminaCostInCombat *= chargedMult;
            chargedCost.baseStaminaCostOutOfCombat *= chargedMult;
            block.ChargedAttackStaminaCost = chargedCost;

            var pushMult = MeleeMult(name, "PushStaminaCost");
            var pushCost = block.PushStaminaCost;
            pushCost.baseStaminaCostInCombat *= pushMult;
            pushCost.baseStaminaCostOutOfCombat *= pushMult;
            block.PushStaminaCost = pushCost;
        }
    }

    private static float MeleeMult(string archetypeName, string fieldKey) =>
        Plugin.MeleeMultipliers.TryGetValue($"{archetypeName}.{fieldKey}", out var entry) ? entry.Value : 1f;

    private static int ScaleInt(int value, float multiplier) => (int)Math.Round(value * multiplier);
}
