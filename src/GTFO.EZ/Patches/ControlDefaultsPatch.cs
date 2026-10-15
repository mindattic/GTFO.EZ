using System;
using HarmonyLib;

namespace GTFO.EZ.Patches;

// Sets the four "make controls less fiddly" game options exactly once per install. These are the
// game's own settings (saved by GTFO, shared across every mod profile), so forcing them on every
// launch would stop a player ever changing them back in the game's menu - after the one-time
// apply they're the player's to change. Runs after PostSetup, once saved settings are loaded.
[HarmonyPatch(typeof(CellSettingsManager), nameof(CellSettingsManager.PostSetup))]
internal static class ControlDefaultsPatch
{
    [HarmonyPostfix]
    private static void Postfix()
    {
        if (Plugin.ControlDefaultsApplied.Value) return;

        try
        {
            CellSettingsManager.SetBoolValue(eCellSettingID.Gameplay_HoldToShowComsList, false); // Q toggles the comms menu
            CellSettingsManager.SetBoolValue(eCellSettingID.Gameplay_SprintToggle, true);
            CellSettingsManager.SetBoolValue(eCellSettingID.Gameplay_AimToggle, true);
            CellSettingsManager.SetBoolValue(eCellSettingID.Gameplay_StandWhenJumpToggle, true); // jumping stands you up instead of staying crouched
            CellSettingsManager.ApplyAndSave();

            Plugin.ControlDefaultsApplied.Value = true;
            Plugin.Logger.LogInfo("GTFO.EZ applied one-time control defaults: comms menu toggle, sprint toggle, aim toggle, stand when jumping.");
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError($"GTFO.EZ failed to apply control defaults: {e}");
        }
    }
}
