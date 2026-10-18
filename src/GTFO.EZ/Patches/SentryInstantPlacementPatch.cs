using GameData;
using HarmonyLib;

namespace GTFO.EZ.Patches;

// m_deployPickupInteractionDuration is a plain instance field set up in Setup(), not something
// GameDataTweaks' usual data-block edits reach - it governs how long you hold the interact key to
// both place and retrieve a sentry gun. Zeroing it here removes the charge-up/wait entirely.
[HarmonyPatch(typeof(SentryGunFirstPerson), nameof(SentryGunFirstPerson.Setup))]
internal static class SentryGunFirstPerson_Setup_Patch
{
    [HarmonyPostfix]
    private static void Postfix(SentryGunFirstPerson __instance, ItemDataBlock data)
    {
        __instance.m_deployPickupInteractionDuration *= Plugin.SentryDeployPickupInteractionDurationMultiplier.Value;
    }
}
