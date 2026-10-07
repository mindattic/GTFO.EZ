using HarmonyLib;

namespace GTFO.EZ.Patches;

/// <summary>
/// Scales how long a revive interaction takes. Interact_Revive inherits InteractDuration from the
/// generic Interact_Timed base, but each instance (revive, door hacking, item pickups, etc.) owns
/// its own value, so patching Setup here only ever touches revives.
/// </summary>
[HarmonyPatch(typeof(Interact_Revive), nameof(Interact_Revive.Setup))]
internal static class Interact_Revive_Setup_Patch
{
    [HarmonyPostfix]
    private static void Postfix(Interact_Revive __instance)
    {
        __instance.InteractDuration *= Plugin.ReviveDurationMultiplier.Value;
    }
}
