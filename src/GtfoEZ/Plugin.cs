using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using BepInEx.Logging;
using HarmonyLib;

namespace GtfoEZ;

[BepInPlugin(GUID, NAME, VERSION)]
[BepInProcess("GTFO.exe")]
public class Plugin : BasePlugin
{
    public const string GUID = "MindAttic.GtfoEZ";
    public const string NAME = "GtfoEZ";
    public const string VERSION = "1.0.0";

    internal static ManualLogSource Logger = null!;

    internal static ConfigEntry<float> HealthMultiplier = null!;
    internal static ConfigEntry<float> HealthRegenMultiplier = null!;
    internal static ConfigEntry<float> StaminaRegenMultiplier = null!;
    internal static ConfigEntry<float> FallDamageMultiplier = null!;
    internal static ConfigEntry<float> FallDamageHeightMultiplier = null!;
    internal static ConfigEntry<float> AmmoMultiplier = null!;
    internal static ConfigEntry<float> FlashlightRangeMultiplier = null!;
    internal static ConfigEntry<float> FlashlightIntensityMultiplier = null!;
    internal static ConfigEntry<float> DetectionDistanceMultiplier = null!;

    public override void Load()
    {
        Logger = Log;
        BindConfig();

        new Harmony(GUID).PatchAll();

        Logger.LogInfo($"{NAME} v{VERSION} loaded.");
    }

    private void BindConfig()
    {
        HealthMultiplier = Config.Bind("Health", "Max Health Multiplier", 2.0f,
            "Multiplies player max health. 2.0 = double health.");

        HealthRegenMultiplier = Config.Bind("Health", "Health Regen Multiplier", 2.0f,
            "Multiplies health regen rate and shortens the delay before regen starts.");

        StaminaRegenMultiplier = Config.Bind("Stamina", "Stamina Regen Multiplier", 1.5f,
            "Multiplies all stamina regen rates (resting/not resting, in/out of combat).");

        FallDamageMultiplier = Config.Bind("Fall Damage", "Fall Damage Multiplier", 0.5f,
            "Multiplies fall damage dealt. 0.5 = half damage.");

        FallDamageHeightMultiplier = Config.Bind("Fall Damage", "Fall Damage Height Multiplier", 1.5f,
            "Multiplies the fall height thresholds before damage starts/maxes out. Higher = have to fall further to take damage.");

        AmmoMultiplier = Config.Bind("Ammo", "Weapon Ammo Multiplier", 1.5f,
            "Multiplies max reserve ammo, starting ammo, and resource pack refill amounts for standard/special/consumable ammo.");

        FlashlightRangeMultiplier = Config.Bind("Lighting", "Flashlight Range Multiplier", 1.3f,
            "Multiplies the range of headlamps and weapon-mounted lights.");

        FlashlightIntensityMultiplier = Config.Bind("Lighting", "Flashlight Intensity Multiplier", 1.3f,
            "Multiplies the brightness of headlamps and weapon-mounted lights.");

        DetectionDistanceMultiplier = Config.Bind("Enemy Detection", "Movement Detection Distance Multiplier", 0.75f,
            "Multiplies how far enemies can detect player movement noise. 0.75 on an 8m base gives 6m.");
    }
}
