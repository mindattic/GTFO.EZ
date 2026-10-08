using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using BepInEx.Logging;
using HarmonyLib;

namespace GTFO.EZ;

[BepInPlugin(GUID, NAME, VERSION)]
[BepInProcess("GTFO.exe")]
public class Plugin : BasePlugin
{
    public const string GUID = "MindAttic.GTFO.EZ";
    public const string NAME = "GTFO.EZ";
    public const string VERSION = "5.0.0";

    // Shared between the rundown-select header and the in-level warden-intel message so the two
    // in-game surfaces never drift apart the way they did before this was pulled out. Major
    // version only, per the project's vN convention, rather than the full x.y.z.
    internal static readonly string EasterEggMessage = $"GTFO.EZ v{VERSION.Split('.')[0]} Active - Goo goo. Gah gah.";

    internal static ManualLogSource Logger = null!;

    internal static ConfigEntry<float> HealthMultiplier = null!;
    internal static ConfigEntry<float> HealthRegenRateMultiplier = null!;
    internal static ConfigEntry<float> HealthRegenCapMultiplier = null!;
    internal static ConfigEntry<bool> RemoveCombatStaminaPenalty = null!;
    internal static ConfigEntry<float> FallDamageMultiplier = null!;
    internal static ConfigEntry<float> FallDamageMinHeightMultiplier = null!;
    internal static ConfigEntry<float> FallDamageMaxHeightMultiplier = null!;
    internal static ConfigEntry<float> AmmoMultiplier = null!;
    internal static ConfigEntry<float> FlashlightAngleMultiplier = null!;
    internal static ConfigEntry<float> FlashlightIntensityMultiplier = null!;
    internal static ConfigEntry<float> DetectionDistanceMultiplier = null!;
    internal static ConfigEntry<float> WalkSpeedMultiplier = null!;
    internal static ConfigEntry<float> RunSpeedMultiplier = null!;
    internal static ConfigEntry<float> AirSpeedMultiplier = null!;
    internal static ConfigEntry<float> CrouchSpeedMultiplier = null!;
    internal static ConfigEntry<float> LadderSpeedMultiplier = null!;
    internal static ConfigEntry<float> JumpHeightMultiplier = null!;
    internal static ConfigEntry<float> FriendlyFireMultiplier = null!;
    internal static ConfigEntry<float> BatteryCapacityMultiplier = null!;
    internal static ConfigEntry<float> SmallBatteryDrainMultiplier = null!;
    internal static ConfigEntry<float> MediumBatteryDrainMultiplier = null!;
    internal static ConfigEntry<float> LargeBatteryDrainMultiplier = null!;
    internal static ConfigEntry<float> NoAirDamageMultiplier = null!;
    internal static ConfigEntry<float> NoAirDepletionTimeMultiplier = null!;
    internal static ConfigEntry<float> EnemyHealthMultiplier = null!;
    internal static ConfigEntry<float> EnemyMeleeDamageMultiplier = null!;
    internal static ConfigEntry<float> EnemyTentacleDamageMultiplier = null!;
    internal static ConfigEntry<float> ConsumableSpawnMultiplier = null!;
    internal static ConfigEntry<float> BigPickupSpawnMultiplier = null!;
    internal static ConfigEntry<float> ReviveDurationMultiplier = null!;
    internal static ConfigEntry<float> FlashlightItemSpawnWeightMultiplier = null!;
    internal static ConfigEntry<float> GlowstickSpawnWeightMultiplier = null!;
    internal static ConfigEntry<float> CFoamGrenadeSpawnWeightMultiplier = null!;
    internal static ConfigEntry<float> FogRepellerSpawnWeightMultiplier = null!;
    internal static ConfigEntry<float> LockMelterSpawnWeightMultiplier = null!;
    internal static ConfigEntry<float> ExplosiveTripMineSpawnWeightMultiplier = null!;
    internal static ConfigEntry<float> MeleeBuffSyringeSpawnWeightMultiplier = null!;
    internal static ConfigEntry<float> HealthSyringeSpawnWeightMultiplier = null!;
    internal static ConfigEntry<float> CFoamTripmineSpawnWeightMultiplier = null!;

    // ExpeditionBalanceDataBlock - the per-difficulty-tier resource/enemy/door budget table.
    internal static ConfigEntry<float> HealthPackResourceMultiplier = null!;
    internal static ConfigEntry<float> DisinfectionResourceMultiplier = null!;
    internal static ConfigEntry<float> WeaponAmmoResourceMultiplier = null!;
    internal static ConfigEntry<float> ToolAmmoResourceMultiplier = null!;
    internal static ConfigEntry<float> CommodityValueMultiplier = null!;
    internal static ConfigEntry<float> CommodityInContainerChanceMultiplier = null!;
    internal static ConfigEntry<float> ArtifactInContainerChanceMultiplier = null!;
    internal static ConfigEntry<float> LargeCommodityPackChanceMultiplier = null!;
    internal static ConfigEntry<float> MediumCommodityPackChanceMultiplier = null!;
    internal static ConfigEntry<float> ResourceContainerReuseChanceMultiplier = null!;
    internal static ConfigEntry<float> MaxPacksPerContainerMultiplier = null!;
    internal static ConfigEntry<float> EmptyWeakContainersPerZoneMultiplier = null!;
    internal static ConfigEntry<float> EmptySecureContainersPerZoneMultiplier = null!;
    internal static ConfigEntry<float> LootPerZoneMultiplier = null!;
    internal static ConfigEntry<float> AirPerZoneMultiplier = null!;
    internal static ConfigEntry<float> AirPerZoneInNoAirZoneMultiplier = null!;
    internal static ConfigEntry<float> TerminalsPerZoneMultiplier = null!;
    internal static ConfigEntry<float> LockedWeakContainerWithPackChanceMultiplier = null!;
    internal static ConfigEntry<float> ResourcePackSizeMultiplier = null!;

    internal static ConfigEntry<float> EnemyPatrolGroupsPerZoneMultiplier = null!;
    internal static ConfigEntry<float> StaticEnemiesMaxPerZoneMultiplier = null!;
    internal static ConfigEntry<float> StaticEnemiesMaxSmallAreaMultiplier = null!;
    internal static ConfigEntry<float> StaticEnemiesMaxMediumAreaMultiplier = null!;
    internal static ConfigEntry<float> StaticEnemiesMaxLargeAreaMultiplier = null!;
    internal static ConfigEntry<float> StaticEnemiesMaxHugeAreaMultiplier = null!;
    internal static ConfigEntry<float> EnemyPopulationPerZoneMultiplier = null!;

    // TentacleTraps and ParasiteNests on ExpeditionBalanceDataBlock are each a StaticEnemyData
    // reference - the same 7-field shape (Health/AttackDamage/MaxPerZone/MaxSmallArea/
    // MaxMediumArea/MaxLargeArea/MaxHugeArea) already used for roaming/sleeper enemies above.
    internal static ConfigEntry<float> TentacleTrapHealthMultiplier = null!;
    internal static ConfigEntry<float> TentacleTrapAttackDamageMultiplier = null!;
    internal static ConfigEntry<float> TentacleTrapMaxPerZoneMultiplier = null!;
    internal static ConfigEntry<float> TentacleTrapMaxSmallAreaMultiplier = null!;
    internal static ConfigEntry<float> TentacleTrapMaxMediumAreaMultiplier = null!;
    internal static ConfigEntry<float> TentacleTrapMaxLargeAreaMultiplier = null!;
    internal static ConfigEntry<float> TentacleTrapMaxHugeAreaMultiplier = null!;

    internal static ConfigEntry<float> ParasiteNestHealthMultiplier = null!;
    internal static ConfigEntry<float> ParasiteNestAttackDamageMultiplier = null!;
    internal static ConfigEntry<float> ParasiteNestMaxPerZoneMultiplier = null!;
    internal static ConfigEntry<float> ParasiteNestMaxSmallAreaMultiplier = null!;
    internal static ConfigEntry<float> ParasiteNestMaxMediumAreaMultiplier = null!;
    internal static ConfigEntry<float> ParasiteNestMaxLargeAreaMultiplier = null!;
    internal static ConfigEntry<float> ParasiteNestMaxHugeAreaMultiplier = null!;

    internal static ConfigEntry<float> LevelGenVoxelCoverageMultiplier = null!;
    internal static ConfigEntry<float> LevelGenVoxelCoverageRandomnessMultiplier = null!;
    internal static ConfigEntry<float> ArtifactsPerSegmentMultiplier = null!;
    internal static ConfigEntry<float> ArtifactsPerLayerMultiplier = null!;

    internal static ConfigEntry<float> WeakDoor4x4HealthMultiplier = null!;
    internal static ConfigEntry<float> WeakDoor8x4HealthMultiplier = null!;
    internal static ConfigEntry<float> WeakDoorNoLockWeightMultiplier = null!;
    internal static ConfigEntry<float> WeakDoorMeleeLockWeightMultiplier = null!;
    internal static ConfigEntry<float> WeakDoorHackableLockWeightMultiplier = null!;
    internal static ConfigEntry<float> WeakDoorUnlockedOpenChanceMultiplier = null!;
    internal static ConfigEntry<float> WeakDoorWallRemoverOpenChanceMultiplier = null!;
    internal static ConfigEntry<float> WeakDoorLockHealthMultiplier = null!;
    internal static ConfigEntry<float> GlueVolumeToDoorHealthConversionMultiplier = null!;
    internal static ConfigEntry<float> GlueVolumeForDoorMaxStateMultiplier = null!;

    // MeleeArchetypeDataBlock - one entry per melee weapon type: Bat, Hammer, Knife, Spear (real
    // in-game names, confirmed from PublicName - "Hammer" covers what's commonly called the
    // sledgehammer). Organized by archetype, then by action type (Light/Charged/Push/General),
    // so with 4 archetypes x 19 stats each this is a lookup table rather than 76 named fields.
    internal static readonly Dictionary<string, ConfigEntry<float>> MeleeMultipliers = new();

    internal static readonly string[] MeleeArchetypes = { "Bat", "Hammer", "Knife", "Spear" };

    // FieldKey matches the MeleeArchetypeDataBlock property name this multiplies, so
    // GameDataTweaks can look it up directly without a second mapping table.
    private static readonly (string FieldKey, string ActionType, string DisplayName, string Description)[] MeleeStatDefs =
    {
        ("LightAttackDamage", "Light Attack", "Damage Multiplier", "Multiplies light-attack damage."),
        ("LightStaggerMulti", "Light Attack", "Stagger Multiplier", "Multiplies stagger force from a light attack."),
        ("LightPrecisionMulti", "Light Attack", "Precision Damage Multiplier", "Multiplies the weak-point damage bonus on a light attack."),
        ("LightEnvironmentMulti", "Light Attack", "Environment Damage Multiplier", "Multiplies light-attack damage dealt to doors/props/environment."),
        ("LightBackstabberMulti", "Light Attack", "Backstab Damage Multiplier", "Multiplies the backstab damage bonus on a light attack."),
        ("LightSleeperMulti", "Light Attack", "Sleeper Damage Multiplier", "Multiplies the sleeping-enemy damage bonus on a light attack."),
        ("LightAttackStaminaCost", "Light Attack", "Stamina Cost Multiplier", "Multiplies stamina cost for a light attack."),

        ("ChargedAttackDamage", "Charged Attack", "Damage Multiplier", "Multiplies charged (heavy) attack damage."),
        ("ChargedStaggerMulti", "Charged Attack", "Stagger Multiplier", "Multiplies stagger force from a charged attack."),
        ("ChargedPrecisionMulti", "Charged Attack", "Precision Damage Multiplier", "Multiplies the weak-point damage bonus on a charged attack."),
        ("ChargedEnvironmentMulti", "Charged Attack", "Environment Damage Multiplier", "Multiplies charged-attack damage dealt to doors/props/environment."),
        ("ChargedBackstabberMulti", "Charged Attack", "Backstab Damage Multiplier", "Multiplies the backstab damage bonus on a charged attack."),
        ("ChargedSleeperMulti", "Charged Attack", "Sleeper Damage Multiplier", "Multiplies the sleeping-enemy damage bonus on a charged attack."),
        ("ChargedAttackStaminaCost", "Charged Attack", "Stamina Cost Multiplier", "Multiplies stamina cost for a charged attack."),

        ("PushDamageSphereRadius", "Push", "Hit Detection Radius Multiplier", "Multiplies the hit-detection radius of the push attack."),
        ("PushStaminaCost", "Push", "Stamina Cost Multiplier", "Multiplies stamina cost for a push."),

        ("CameraDamageRayLength", "General", "Reach Multiplier", "Multiplies attack reach."),
        ("AttackSphereRadius", "General", "Hit Detection Radius Multiplier", "Multiplies the hit-detection radius of light and charged attacks."),
        ("PlayerRunSpeedMultiWhileCharging", "General", "Charge Move Speed Multiplier", "Multiplies move speed while charging a heavy attack."),
    };

    public override void Load()
    {
        Logger = Log;
        BindConfig();

        new Harmony(GUID).PatchAll();

        Logger.LogInfo($"{NAME} v{VERSION} loaded.");
    }

    // Defaults follow community-consensus values rather than naive "bigger number" guesses:
    // health x2 is the one near-universal agreement among GTFO easy-mode mods (going further,
    // e.g. x4, was reported to make the game too easy and got walked back). Regen rate and cap
    // need separate multipliers because vanilla under-regens relative to its own cap (rate 0.2/s
    // against a 20% cap), so a single shared multiplier can't hit both target numbers at once.
    private void BindConfig()
    {
        HealthMultiplier = Config.Bind("Health", "Max Health Multiplier", 2.0f,
            "Multiplies player max health. Vanilla 25 -> 50 at the default 2.0.");

        HealthRegenRateMultiplier = Config.Bind("Health", "Health Regen Rate Multiplier", 5.0f,
            "Multiplies health regen per second. Vanilla 0.2/s -> 1.0/s at the default 5.0 (about 2% of max health per second).");

        HealthRegenCapMultiplier = Config.Bind("Health", "Health Regen Cap Multiplier", 2.0f,
            "Multiplies how much of max health can regenerate without a med kit. Vanilla 20% -> 40% at the default 2.0. Regen delay is left at vanilla on purpose - fast regen with no delay is overpowered.");

        RemoveCombatStaminaPenalty = Config.Bind("Stamina", "Remove Combat Stamina Penalty", true,
            "Makes stamina regen while in combat as fast as out of combat, and removes the 90% stamina cap while in combat.");

        FallDamageMultiplier = Config.Bind("Fall Damage", "Fall Damage Multiplier", 0.1f,
            "Multiplies fall damage dealt once you're past the no-damage height. Default 0.1 (10% of normal) on top of the height change below.");

        FallDamageMinHeightMultiplier = Config.Bind("Fall Damage", "Fall Damage Min Height Multiplier", 1.0f,
            "Multiplies the minimum fall height before any damage is taken. Left at vanilla (1.0) - the 10% damage multiplier above already does the work.");

        FallDamageMaxHeightMultiplier = Config.Bind("Fall Damage", "Fall Damage Max Height Multiplier", 1.5f,
            "Multiplies the fall height at which damage maxes out. Vanilla 20m -> 30m at the default 1.5.");

        AmmoMultiplier = Config.Bind("Ammo", "Weapon Ammo Multiplier", 1.5f,
            "Multiplies max reserve ammo, starting ammo, and refill amounts for standard/special/consumable ammo. Deliberately not paired with a damage buff - that combination was reported as overtuned.");

        FlashlightAngleMultiplier = Config.Bind("Lighting", "Flashlight Angle Multiplier", 1.0f,
            "Multiplies the cone angle of headlamps and weapon-mounted lights. Left at vanilla (1.0) by default.");

        FlashlightIntensityMultiplier = Config.Bind("Lighting", "Flashlight Intensity Multiplier", 1.0f,
            "Multiplies the brightness of headlamps and weapon-mounted lights. Left at vanilla (1.0) by default - brighter lights make you more visible to enemies too.");

        DetectionDistanceMultiplier = Config.Bind("Enemy Detection", "Movement Detection Distance Multiplier", 0.75f,
            "Multiplies how far enemies can detect player movement noise. 0.75 on an 8m base gives 6m.");

        // Exposed for players who want to retune feel/difficulty further, but every one of these
        // defaults to 1.0 (vanilla, no change) - unlike the stats above, there's no community
        // consensus that these need adjusting for an easier game, so we're not guessing.
        WalkSpeedMultiplier = Config.Bind("Movement", "Walk Speed Multiplier", 1.0f,
            "Multiplies walking move speed. Left at vanilla (1.0) by default.");

        RunSpeedMultiplier = Config.Bind("Movement", "Run Speed Multiplier", 1.0f,
            "Multiplies sprinting move speed. Left at vanilla (1.0) by default.");

        AirSpeedMultiplier = Config.Bind("Movement", "Air Speed Multiplier", 1.0f,
            "Multiplies mid-air move speed (control while jumping/falling). Left at vanilla (1.0) by default.");

        CrouchSpeedMultiplier = Config.Bind("Movement", "Crouch Speed Multiplier", 1.0f,
            "Multiplies crouched move speed. Left at vanilla (1.0) by default.");

        LadderSpeedMultiplier = Config.Bind("Movement", "Ladder Speed Multiplier", 1.0f,
            "Multiplies ladder climb speed. Left at vanilla (1.0) by default.");

        JumpHeightMultiplier = Config.Bind("Movement", "Jump Height Multiplier", 1.0f,
            "Multiplies jump height (initial jump velocity). Left at vanilla (1.0) by default.");

        FriendlyFireMultiplier = Config.Bind("Combat", "Friendly Fire Multiplier", 1.0f,
            "Multiplies damage taken from teammates' weapons. Left at vanilla (1.0) by default.");

        BatteryCapacityMultiplier = Config.Bind("Utility", "Battery Capacity Multiplier", 1.0f,
            "Multiplies max battery charge for flashlights and powered tools. Left at vanilla (1.0) by default.");

        SmallBatteryDrainMultiplier = Config.Bind("Utility", "Small Battery Drain Multiplier", 1.0f,
            "Multiplies battery drain per second for small powered tools. Left at vanilla (1.0) by default.");

        MediumBatteryDrainMultiplier = Config.Bind("Utility", "Medium Battery Drain Multiplier", 1.0f,
            "Multiplies battery drain per second for medium powered tools. Left at vanilla (1.0) by default.");

        LargeBatteryDrainMultiplier = Config.Bind("Utility", "Large Battery Drain Multiplier", 1.0f,
            "Multiplies battery drain per second for large powered tools. Left at vanilla (1.0) by default.");

        NoAirDamageMultiplier = Config.Bind("Environment", "No-Air Damage Multiplier", 1.0f,
            "Multiplies the damage rate from suffocating in no-air zones once your air runs out. Left at vanilla (1.0) by default.");

        NoAirDepletionTimeMultiplier = Config.Bind("Environment", "No-Air Depletion Time Multiplier", 1.0f,
            "Multiplies how long your air meter lasts in no-air zones before it empties. Left at vanilla (1.0) by default.");

        EnemyHealthMultiplier = Config.Bind("Enemy Balance", "Enemy Health Multiplier", 1.0f,
            "Multiplies max health for every enemy type. Left at vanilla (1.0) by default.");

        EnemyMeleeDamageMultiplier = Config.Bind("Enemy Balance", "Enemy Melee Damage Multiplier", 1.0f,
            "Multiplies melee attack damage for every enemy type. Left at vanilla (1.0) by default.");

        EnemyTentacleDamageMultiplier = Config.Bind("Enemy Balance", "Enemy Tentacle Damage Multiplier", 1.0f,
            "Multiplies tentacle attack damage (Striker/Tank grabs, Scout tentacles, etc.) for every enemy type. Left at vanilla (1.0) by default.");

        ConsumableSpawnMultiplier = Config.Bind("Loot", "Consumable Spawn Multiplier", 1.0f,
            "Multiplies how many consumables (med/ammo packs, syringes, etc.) spawn per zone. Left at vanilla (1.0) by default.");

        BigPickupSpawnMultiplier = Config.Bind("Loot", "Big Pickup Spawn Multiplier", 1.0f,
            "Multiplies how many big pickups (fog turbines, artifacts, etc.) spawn per zone. Left at vanilla (1.0) by default.");

        ReviveDurationMultiplier = Config.Bind("Interactions", "Revive Duration Multiplier", 1.0f,
            "Multiplies how long it takes to revive a downed teammate. Lower is faster. Left at vanilla (1.0) by default.");

        // Each consumable is picked from a shared weighted lottery, so these retune the odds
        // between them rather than the total amount of loot (that's ConsumableSpawnMultiplier
        // above). Glow Stick defaults to 0 (disabled) - every other item here is left at vanilla.
        FlashlightItemSpawnWeightMultiplier = Config.Bind("Loot", "Long Range Flashlight Spawn Weight Multiplier", 1.0f,
            "Multiplies how likely a Long Range Flashlight is picked when a consumable spawns. Left at vanilla (1.0) by default.");

        GlowstickSpawnWeightMultiplier = Config.Bind("Loot", "Glow Stick Spawn Weight Multiplier", 0.0f,
            "Multiplies how likely a glow stick (either color) is picked when a consumable spawns. Defaults to 0 (disabled) - other consumables fill those slots.");

        CFoamGrenadeSpawnWeightMultiplier = Config.Bind("Loot", "C-Foam Grenade Spawn Weight Multiplier", 1.0f,
            "Multiplies how likely a C-Foam Grenade is picked when a consumable spawns. Left at vanilla (1.0) by default.");

        FogRepellerSpawnWeightMultiplier = Config.Bind("Loot", "Fog Repeller Spawn Weight Multiplier", 1.0f,
            "Multiplies how likely a Fog Repeller is picked when a consumable spawns. Left at vanilla (1.0) by default.");

        LockMelterSpawnWeightMultiplier = Config.Bind("Loot", "Lock Melter Spawn Weight Multiplier", 1.0f,
            "Multiplies how likely a Lock Melter is picked when a consumable spawns. Left at vanilla (1.0) by default.");

        ExplosiveTripMineSpawnWeightMultiplier = Config.Bind("Loot", "Explosive Trip Mine Spawn Weight Multiplier", 1.0f,
            "Multiplies how likely an Explosive Trip Mine is picked when a consumable spawns. Left at vanilla (1.0) by default.");

        MeleeBuffSyringeSpawnWeightMultiplier = Config.Bind("Loot", "Melee Buff Syringe Spawn Weight Multiplier", 1.0f,
            "Multiplies how likely a IIx (melee buff) Syringe is picked when a consumable spawns. Left at vanilla (1.0) by default.");

        HealthSyringeSpawnWeightMultiplier = Config.Bind("Loot", "Health Syringe Spawn Weight Multiplier", 1.0f,
            "Multiplies how likely a I2-LP (health) Syringe is picked when a consumable spawns. Left at vanilla (1.0) by default.");

        CFoamTripmineSpawnWeightMultiplier = Config.Bind("Loot", "C-Foam Tripmine Spawn Weight Multiplier", 1.0f,
            "Multiplies how likely a C-Foam Tripmine is picked when a consumable spawns. Left at vanilla (1.0) by default.");

        // ExpeditionBalanceDataBlock: the per-difficulty-tier resource/enemy/door budget table.
        // Every entry here is left at vanilla (1.0) by default - this is a much bigger tuning
        // surface than the rest of the mod, exposed for players who want to go further themselves.
        HealthPackResourceMultiplier = Config.Bind("Expedition Resources", "Health Pack Resource Multiplier", 1.0f,
            "Multiplies the health-pack resource budget distributed per zone. Left at vanilla (1.0) by default.");

        DisinfectionResourceMultiplier = Config.Bind("Expedition Resources", "Disinfection Resource Multiplier", 1.0f,
            "Multiplies the infection-cure resource budget distributed per zone. Left at vanilla (1.0) by default.");

        WeaponAmmoResourceMultiplier = Config.Bind("Expedition Resources", "Weapon Ammo Resource Multiplier", 1.0f,
            "Multiplies the weapon ammo-pack resource budget distributed per zone. Left at vanilla (1.0) by default.");

        ToolAmmoResourceMultiplier = Config.Bind("Expedition Resources", "Tool Ammo Resource Multiplier", 1.0f,
            "Multiplies the tool ammo (sentries, C-foam launcher, etc.) resource budget per zone. Left at vanilla (1.0) by default.");

        CommodityValueMultiplier = Config.Bind("Expedition Resources", "Commodity Value Multiplier", 1.0f,
            "Multiplies the total commodity (crafting material) value distributed per zone. Left at vanilla (1.0) by default.");

        CommodityInContainerChanceMultiplier = Config.Bind("Expedition Resources", "Commodity In Container Chance Multiplier", 1.0f,
            "Multiplies the chance a resource container holds a commodity. Left at vanilla (1.0) by default.");

        ArtifactInContainerChanceMultiplier = Config.Bind("Expedition Resources", "Artifact In Container Chance Multiplier", 1.0f,
            "Multiplies the chance a resource container holds an artifact. Left at vanilla (1.0) by default.");

        LargeCommodityPackChanceMultiplier = Config.Bind("Expedition Resources", "Large Commodity Pack Chance Multiplier", 1.0f,
            "Multiplies the chance a commodity spawn is a large pack instead of small. Left at vanilla (1.0) by default.");

        MediumCommodityPackChanceMultiplier = Config.Bind("Expedition Resources", "Medium Commodity Pack Chance Multiplier", 1.0f,
            "Multiplies the chance a commodity spawn is a medium pack instead of small. Left at vanilla (1.0) by default.");

        ResourceContainerReuseChanceMultiplier = Config.Bind("Expedition Resources", "Resource Container Reuse Chance Multiplier", 1.0f,
            "Multiplies the chance a resource container gets reused for a second pack. Left at vanilla (1.0) by default.");

        MaxPacksPerContainerMultiplier = Config.Bind("Expedition Resources", "Max Packs Per Container Multiplier", 1.0f,
            "Multiplies the max number of packs a single resource container can hold. Left at vanilla (1.0) by default.");

        EmptyWeakContainersPerZoneMultiplier = Config.Bind("Expedition Resources", "Empty Weak Containers Per Zone Multiplier", 1.0f,
            "Multiplies how many empty weak (cardboard box) containers spawn per zone. Left at vanilla (1.0) by default.");

        EmptySecureContainersPerZoneMultiplier = Config.Bind("Expedition Resources", "Empty Secure Containers Per Zone Multiplier", 1.0f,
            "Multiplies how many empty secure (locker) containers spawn per zone. Left at vanilla (1.0) by default.");

        LootPerZoneMultiplier = Config.Bind("Expedition Resources", "Loot Per Zone Multiplier", 1.0f,
            "Multiplies the general loot budget distributed per zone. Left at vanilla (1.0) by default.");

        AirPerZoneMultiplier = Config.Bind("Expedition Resources", "Air Per Zone Multiplier", 1.0f,
            "Multiplies the breathable-air resource budget per zone. Left at vanilla (1.0) by default.");

        AirPerZoneInNoAirZoneMultiplier = Config.Bind("Expedition Resources", "Air Per Zone In No-Air Zone Multiplier", 1.0f,
            "Multiplies the air resource budget per zone specifically inside no-air areas. Left at vanilla (1.0) by default.");

        TerminalsPerZoneMultiplier = Config.Bind("Expedition Resources", "Terminals Per Zone Multiplier", 1.0f,
            "Multiplies how many terminals spawn per zone. Left at vanilla (1.0) by default.");

        LockedWeakContainerWithPackChanceMultiplier = Config.Bind("Expedition Resources", "Locked Weak Container With Pack Chance Multiplier", 1.0f,
            "Multiplies the chance a weak container holding a pack is locked. Left at vanilla (1.0) by default.");

        ResourcePackSizeMultiplier = Config.Bind("Expedition Resources", "Resource Pack Size Multiplier", 1.0f,
            "Multiplies the size of every resource pack tier (small/medium/large). Left at vanilla (1.0) by default.");

        EnemyPatrolGroupsPerZoneMultiplier = Config.Bind("Expedition Enemies", "Enemy Patrol Groups Per Zone Multiplier", 1.0f,
            "Multiplies how many roaming enemy patrol groups spawn per zone. Left at vanilla (1.0) by default.");

        StaticEnemiesMaxPerZoneMultiplier = Config.Bind("Expedition Enemies", "Static Enemies Max Per Zone Multiplier", 1.0f,
            "Multiplies the max number of stationary (sleeper) enemies per zone. Left at vanilla (1.0) by default.");

        StaticEnemiesMaxSmallAreaMultiplier = Config.Bind("Expedition Enemies", "Static Enemies Max Small Area Multiplier", 1.0f,
            "Multiplies the max stationary enemies allowed in a small area. Left at vanilla (1.0) by default.");

        StaticEnemiesMaxMediumAreaMultiplier = Config.Bind("Expedition Enemies", "Static Enemies Max Medium Area Multiplier", 1.0f,
            "Multiplies the max stationary enemies allowed in a medium area. Left at vanilla (1.0) by default.");

        StaticEnemiesMaxLargeAreaMultiplier = Config.Bind("Expedition Enemies", "Static Enemies Max Large Area Multiplier", 1.0f,
            "Multiplies the max stationary enemies allowed in a large area. Left at vanilla (1.0) by default.");

        StaticEnemiesMaxHugeAreaMultiplier = Config.Bind("Expedition Enemies", "Static Enemies Max Huge Area Multiplier", 1.0f,
            "Multiplies the max stationary enemies allowed in a huge area. Left at vanilla (1.0) by default.");

        EnemyPopulationPerZoneMultiplier = Config.Bind("Expedition Enemies", "Enemy Population Per Zone Multiplier", 1.0f,
            "Multiplies the overall enemy population budget per zone. Left at vanilla (1.0) by default.");

        TentacleTrapHealthMultiplier = Config.Bind("Expedition Enemies", "Tentacle Trap Health Multiplier", 1.0f,
            "Multiplies max health for tentacle traps. Left at vanilla (1.0) by default.");

        TentacleTrapAttackDamageMultiplier = Config.Bind("Expedition Enemies", "Tentacle Trap Attack Damage Multiplier", 1.0f,
            "Multiplies attack damage dealt by tentacle traps. Left at vanilla (1.0) by default.");

        TentacleTrapMaxPerZoneMultiplier = Config.Bind("Expedition Enemies", "Tentacle Trap Max Per Zone Multiplier", 1.0f,
            "Multiplies the max number of tentacle traps per zone. Left at vanilla (1.0) by default.");

        TentacleTrapMaxSmallAreaMultiplier = Config.Bind("Expedition Enemies", "Tentacle Trap Max Small Area Multiplier", 1.0f,
            "Multiplies the max tentacle traps allowed in a small area. Left at vanilla (1.0) by default.");

        TentacleTrapMaxMediumAreaMultiplier = Config.Bind("Expedition Enemies", "Tentacle Trap Max Medium Area Multiplier", 1.0f,
            "Multiplies the max tentacle traps allowed in a medium area. Left at vanilla (1.0) by default.");

        TentacleTrapMaxLargeAreaMultiplier = Config.Bind("Expedition Enemies", "Tentacle Trap Max Large Area Multiplier", 1.0f,
            "Multiplies the max tentacle traps allowed in a large area. Left at vanilla (1.0) by default.");

        TentacleTrapMaxHugeAreaMultiplier = Config.Bind("Expedition Enemies", "Tentacle Trap Max Huge Area Multiplier", 1.0f,
            "Multiplies the max tentacle traps allowed in a huge area. Left at vanilla (1.0) by default.");

        ParasiteNestHealthMultiplier = Config.Bind("Expedition Enemies", "Parasite Nest Health Multiplier", 1.0f,
            "Multiplies max health for parasite nests. Left at vanilla (1.0) by default.");

        ParasiteNestAttackDamageMultiplier = Config.Bind("Expedition Enemies", "Parasite Nest Attack Damage Multiplier", 1.0f,
            "Multiplies attack damage dealt by parasite nests. Left at vanilla (1.0) by default.");

        ParasiteNestMaxPerZoneMultiplier = Config.Bind("Expedition Enemies", "Parasite Nest Max Per Zone Multiplier", 1.0f,
            "Multiplies the max number of parasite nests per zone. Left at vanilla (1.0) by default.");

        ParasiteNestMaxSmallAreaMultiplier = Config.Bind("Expedition Enemies", "Parasite Nest Max Small Area Multiplier", 1.0f,
            "Multiplies the max parasite nests allowed in a small area. Left at vanilla (1.0) by default.");

        ParasiteNestMaxMediumAreaMultiplier = Config.Bind("Expedition Enemies", "Parasite Nest Max Medium Area Multiplier", 1.0f,
            "Multiplies the max parasite nests allowed in a medium area. Left at vanilla (1.0) by default.");

        ParasiteNestMaxLargeAreaMultiplier = Config.Bind("Expedition Enemies", "Parasite Nest Max Large Area Multiplier", 1.0f,
            "Multiplies the max parasite nests allowed in a large area. Left at vanilla (1.0) by default.");

        ParasiteNestMaxHugeAreaMultiplier = Config.Bind("Expedition Enemies", "Parasite Nest Max Huge Area Multiplier", 1.0f,
            "Multiplies the max parasite nests allowed in a huge area. Left at vanilla (1.0) by default.");

        // Internal level-generation tuning, not a gameplay-difficulty stat - exposed for
        // completeness, but retuning it changes how levels get built, not just how hard they are.
        LevelGenVoxelCoverageMultiplier = Config.Bind("Expedition Level Gen", "Voxel Coverage Area Multiplier", 1.0f,
            "Multiplies an internal level-generation area-coverage factor. Left at vanilla (1.0) by default.");

        LevelGenVoxelCoverageRandomnessMultiplier = Config.Bind("Expedition Level Gen", "Voxel Coverage Area Randomness Multiplier", 1.0f,
            "Multiplies the randomness factor in that same level-generation coverage scoring. Left at vanilla (1.0) by default.");

        ArtifactsPerSegmentMultiplier = Config.Bind("Expedition Level Gen", "Artifacts Per Segment Multiplier", 1.0f,
            "Multiplies how many artifacts can spawn per level segment. Left at vanilla (1.0) by default.");

        ArtifactsPerLayerMultiplier = Config.Bind("Expedition Level Gen", "Artifacts Per Layer Multiplier", 1.0f,
            "Multiplies how many artifacts can spawn per level layer. Left at vanilla (1.0) by default.");

        WeakDoor4x4HealthMultiplier = Config.Bind("Expedition Doors", "Weak Door 4x4 Health Multiplier", 1.0f,
            "Multiplies the health of small (4x4) weak doors. Left at vanilla (1.0) by default.");

        WeakDoor8x4HealthMultiplier = Config.Bind("Expedition Doors", "Weak Door 8x4 Health Multiplier", 1.0f,
            "Multiplies the health of large (8x4) weak doors. Left at vanilla (1.0) by default.");

        WeakDoorNoLockWeightMultiplier = Config.Bind("Expedition Doors", "Weak Door No-Lock Weight Multiplier", 1.0f,
            "Multiplies the odds a weak door spawns with no lock at all. Left at vanilla (1.0) by default.");

        WeakDoorMeleeLockWeightMultiplier = Config.Bind("Expedition Doors", "Weak Door Melee Lock Weight Multiplier", 1.0f,
            "Multiplies the odds a weak door spawns with a melee-breakable lock. Left at vanilla (1.0) by default.");

        WeakDoorHackableLockWeightMultiplier = Config.Bind("Expedition Doors", "Weak Door Hackable Lock Weight Multiplier", 1.0f,
            "Multiplies the odds a weak door spawns with a hackable lock. Left at vanilla (1.0) by default.");

        WeakDoorUnlockedOpenChanceMultiplier = Config.Bind("Expedition Doors", "Weak Door Unlocked Open Chance Multiplier", 1.0f,
            "Multiplies the chance an unlocked weak door starts already open. Left at vanilla (1.0) by default.");

        WeakDoorWallRemoverOpenChanceMultiplier = Config.Bind("Expedition Doors", "Weak Door Wall Remover Open Chance Multiplier", 1.0f,
            "Multiplies the chance a door starts open where a wall-remover charge was used. Left at vanilla (1.0) by default.");

        WeakDoorLockHealthMultiplier = Config.Bind("Expedition Doors", "Weak Door Lock Health Multiplier", 1.0f,
            "Multiplies the health of a weak door's lock itself (separate from the door). Left at vanilla (1.0) by default.");

        GlueVolumeToDoorHealthConversionMultiplier = Config.Bind("Expedition Doors", "Glue Volume To Door Health Conversion Multiplier", 1.0f,
            "Multiplies how effectively C-Foam volume converts into door damage. Left at vanilla (1.0) by default.");

        GlueVolumeForDoorMaxStateMultiplier = Config.Bind("Expedition Doors", "Glue Volume For Door Max State Multiplier", 1.0f,
            "Multiplies how much C-Foam volume is needed to fully seal a door. Left at vanilla (1.0) by default.");

        // MeleeArchetypeDataBlock: one weapon type per archetype (Bat, Hammer, Knife, Spear),
        // one section per archetype+action-type combination. Every entry defaults to vanilla (1.0).
        foreach (var archetype in MeleeArchetypes)
        {
            foreach (var stat in MeleeStatDefs)
            {
                var section = $"Melee - {archetype} - {stat.ActionType}";
                var entry = Config.Bind(section, stat.DisplayName, 1.0f,
                    $"{stat.Description} Left at vanilla (1.0) by default.");
                MeleeMultipliers[$"{archetype}.{stat.FieldKey}"] = entry;
            }
        }
    }
}
