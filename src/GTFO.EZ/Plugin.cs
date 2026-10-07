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
            "Makes stamina regen while in combat as fast as while out of combat, instead of the vanilla combat penalty.");

        FallDamageMultiplier = Config.Bind("Fall Damage", "Fall Damage Multiplier", 1.0f,
            "Multiplies fall damage dealt once you're past the no-damage height. Left at vanilla (1.0) by default - the height change below already does most of the work.");

        FallDamageMinHeightMultiplier = Config.Bind("Fall Damage", "Fall Damage Min Height Multiplier", 2.0f,
            "Multiplies the minimum fall height before any damage is taken. Vanilla 4m -> 8m at the default 2.0.");

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
    }
}
