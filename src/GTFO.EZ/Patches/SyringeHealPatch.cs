using System;
using HarmonyLib;

namespace GTFO.EZ.Patches;

// The I2-LP's heal is hardcoded in native code (vanilla rolls 5-15% of max health) - no datablock
// carries it, so it can't be scaled like everything in GameDataTweaks. It reaches the player
// through Dam_SyncedDamageBase.AddHealth, which med kits use too, so the syringe's own apply
// coroutine flags the call window and only AddHealth calls made inside it are rewritten.
[HarmonyPatch(typeof(SyringeFirstPerson._ApplySyringe_d__18), nameof(SyringeFirstPerson._ApplySyringe_d__18.MoveNext))]
internal static class SyringeApply_MoveNext_Patch
{
    [ThreadStatic] internal static bool ApplyingHealthSyringe;

    [HarmonyPrefix]
    private static void Prefix(SyringeFirstPerson._ApplySyringe_d__18 __instance)
    {
        var syringe = __instance.__4__this;
        ApplyingHealthSyringe = syringe != null && syringe.m_type == eSyringeType.Health;
    }

    // A finalizer, not a postfix, so the flag clears even if MoveNext throws - otherwise the next
    // med kit heal would be rewritten as a syringe heal.
    [HarmonyFinalizer]
    private static void Finalizer() => ApplyingHealthSyringe = false;
}

[HarmonyPatch(typeof(Dam_SyncedDamageBase), nameof(Dam_SyncedDamageBase.AddHealth))]
internal static class SyringeAddHealth_Patch
{
    [HarmonyPrefix]
    private static void Prefix(Dam_SyncedDamageBase __instance, ref float addHealth)
    {
        if (!SyringeApply_MoveNext_Patch.ApplyingHealthSyringe) return;

        var fraction = Plugin.HealthSyringeHealFraction.Value;
        if (fraction <= 0f) return;

        var vanillaAmount = addHealth;
        addHealth = __instance.HealthMax * fraction;
        Plugin.Logger.LogInfo($"I2-LP syringe heal: {vanillaAmount:0.##} -> {addHealth:0.##} HP (max {__instance.HealthMax:0.##}).");
    }
}
