using HarmonyLib;
using Player;

namespace GTFO.EZ.Patches;

/// <summary>
/// Shows the "Goo goo. Gah gah." easter egg as an in-level warden-intel message once gameplay
/// actually starts, rather than only in the BepInEx log and on the rundown-select screen. Hooks
/// the local player's spawn instead of a level-load event, since GuiManager.PlayerLayer isn't
/// guaranteed to exist until the player agent does.
/// </summary>
[HarmonyPatch(typeof(PlayerAgent), nameof(PlayerAgent.Setup))]
internal static class PlayerAgent_Setup_Patch
{
    private const float DelaySeconds = 90f;
    private const float DurationSeconds = 5f;

    [HarmonyPostfix]
    private static void Postfix(PlayerAgent __instance)
    {
        if (!__instance.IsLocallyOwned) return;
        GuiManager.PlayerLayer?.ShowWardenIntel(Plugin.EasterEggMessage, DelaySeconds, DurationSeconds);
    }
}
