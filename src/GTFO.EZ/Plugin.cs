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
    public const string VERSION = "1.0.0";

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
            "Multiplies how much of max health can regenerate without a med kit. Vanilla 20% -> 40% at the default 2.0. Regen delay after damage is left at vanilla on purpose: fast regen with no delay makes you nearly unkillable in a slow fight.");

        RemoveCombatStaminaPenalty = Config.Bind("Stamina", "Remove Combat Stamina Penalty", true,
            "Makes stamina regen while in combat as fast as while out of combat, instead of the vanilla combat penalty.");

        FallDamageMultiplier = Config.Bind("Fall Damage", "Fall Damage Multiplier", 1.0f,
            "Multiplies fall damage dealt once you're past the no-damage height. Left at vanilla (1.0) by default - the height change below already does most of the work.");

        FallDamageMinHeightMultiplier = Config.Bind("Fall Damage", "Fall Damage Min Height Multiplier", 2.0f,
            "Multiplies the minimum fall height before any damage is taken. Vanilla 4m -> 8m at the default 2.0.");

        FallDamageMaxHeightMultiplier = Config.Bind("Fall Damage", "Fall Damage Max Height Multiplier", 1.5f,
            "Multiplies the fall height at which damage maxes out. Vanilla 20m -> 30m at the default 1.5.");

        AmmoMultiplier = Config.Bind("Ammo", "Weapon Ammo Multiplier", 1.5f,
            "Multiplies max reserve ammo, starting ammo, and resource pack refill amounts for standard/special/consumable ammo. Deliberately not paired with a damage buff - that combination was reported as overtuned.");

        FlashlightAngleMultiplier = Config.Bind("Lighting", "Flashlight Angle Multiplier", 1.25f,
            "Multiplies the cone angle of headlamps and weapon-mounted lights.");

        FlashlightIntensityMultiplier = Config.Bind("Lighting", "Flashlight Intensity Multiplier", 1.25f,
            "Multiplies the brightness of headlamps and weapon-mounted lights. Kept modest - brighter lights make you more visible to enemies too.");

        DetectionDistanceMultiplier = Config.Bind("Enemy Detection", "Movement Detection Distance Multiplier", 0.75f,
            "Multiplies how far enemies can detect player movement noise. 0.75 on an 8m base gives 6m.");
    }
}
