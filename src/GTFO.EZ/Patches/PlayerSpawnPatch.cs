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
    private const float EasyModeAnnouncementDelaySeconds = 90f;
    private const float EasyModeAnnouncementDurationSeconds = 30f;

    [HarmonyPostfix]
    private static void Postfix(PlayerAgent __instance)
    {
        // The host's bots are locally owned too; only the player's own agent announces.
        if (!__instance.IsLocallyOwned || __instance.Owner == null || __instance.Owner.IsBot) return;
        GuiManager.PlayerLayer?.ShowWardenIntel(Plugin.EasterEggMessage, EasyModeAnnouncementDelaySeconds, EasyModeAnnouncementDurationSeconds);
    }
}
